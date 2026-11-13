/*
Test automat de gameplay: porneste GameScene in Play Mode si un "bot" joaca nivelul 1.

  Meniu: Tools > Dungeon Explorer > Run Auto Playtest
  Linie de comanda: Unity.exe -batchmode -projectPath ... -executeMethod AutoPlaytest.RunFromCommandLine

Verifica:
  1. scena e configurata (sisteme, referinte, componente pe Player)
  2. apar marcajele de spawn si apoi inamicii
  3. inamicii vin spre jucator si il lovesc (viata scade, invulnerabilitate)
  4. botul ii omoara; un Zombie moare din exact 2 lovituri de sabie
  5. coin-uri: exact 1 la 3 inamici (shuffle bag), colectate in portofel
  6. ultimul inamic lasa cheia, iar cheia e colectata
  7. toate cele 3 valuri se termina
  8. la 0 vieti scena se reincarca
  + nicio exceptie / eroare in Console pe parcurs
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class AutoPlaytest
{
    private const string ScenePath = "Assets/Scenes/GameScene.unity";
    private const double TimeoutSeconds = 300;
    private const double SettleSeconds = 8; // asteptam ca editorul sa termine importul/indexarea inainte de Play
    private const string ActiveKey = "AutoPlaytest.Active";
    private const string CommandLineKey = "AutoPlaytest.CommandLine";
    private const string RetriesKey = "AutoPlaytest.Retries";

    private static double waitStart;

    // Dupa o reincarcare de scripturi (domain reload) variabilele statice se pierd.
    // SessionState supravietuieste, deci reluam testul de unde trebuie.
    static AutoPlaytest()
    {
        if (!SessionState.GetBool(ActiveKey, false)) return;
        commandLine = SessionState.GetBool(CommandLineKey, false);

        int retries = SessionState.GetInt(RetriesKey, 0);
        if (retries >= 3)
        {
            Debug.LogError("[AutoPlaytest] Prea multe reincarcari de scripturi; renunt.");
            SessionState.EraseBool(ActiveKey);
            if (commandLine) EditorApplication.Exit(2);
            return;
        }
        SessionState.SetInt(RetriesKey, retries + 1);

        // Reincarcarea a stricat jocul in curs (pool-uri, abonari): oprim Play si reluam de la capat.
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlaying)
            {
                EditorApplication.playModeStateChanged += RestartAfterExit;
                EditorApplication.ExitPlaymode();
            }
            else
            {
                Begin();
            }
        };
    }

    private static void RestartAfterExit(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredEditMode) return;
        EditorApplication.playModeStateChanged -= RestartAfterExit;
        Begin();
    }

    private enum Phase { Checks, WaitSpawn, WaitPlayerHit, Fight, CollectKey, DeathTest, WaitRestart, Done }

    private static bool commandLine;
    private static Phase phase;
    private static double startRealtime, phaseStartRealtime;
    private static readonly List<string> results = new List<string>();
    private static readonly List<string> errors = new List<string>();
    private static int failures;

    // date observate
    private static bool sawMarker;
    private static float firstEnemyDistance = -1f;
    private static int deaths, coinsSpawnedSeen, keysSeen;
    private static bool keyCollected, allWavesCleared;
    private static readonly Dictionary<Enemy, int> hitsTaken = new Dictionary<Enemy, int>();
    private static readonly Dictionary<Enemy, float> lastHealth = new Dictionary<Enemy, float>();
    private static readonly List<int> zombieHitsToKill = new List<int>();
    private static readonly HashSet<CoinPickup> seenCoins = new HashSet<CoinPickup>();
    private static float nextAttackTime;
    private static PlayerHealth deadPlayer;

    [MenuItem("Tools/Dungeon Explorer/Run Auto Playtest")]
    public static void RunFromMenu()
    {
        commandLine = false;
        StartSession();
    }

    public static void RunFromCommandLine()
    {
        commandLine = true;
        StartSession();
    }

    private static void StartSession()
    {
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(CommandLineKey, commandLine);
        SessionState.SetInt(RetriesKey, 0);
        Begin();
    }

    private static void Begin()
    {
        results.Clear(); errors.Clear(); failures = 0;
        sawMarker = false; firstEnemyDistance = -1f;
        deaths = coinsSpawnedSeen = keysSeen = 0;
        keyCollected = allWavesCleared = false;
        hitsTaken.Clear(); lastHealth.Clear(); zombieHitsToKill.Clear(); seenCoins.Clear();
        nextAttackTime = 0f; deadPlayer = null;

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        waitStart = EditorApplication.timeSinceStartup;
        EditorApplication.update += WaitThenPlay;
    }

    private static void WaitThenPlay()
    {
        if (EditorApplication.isCompiling || EditorApplication.isUpdating) waitStart = EditorApplication.timeSinceStartup;
        if (EditorApplication.timeSinceStartup - waitStart < SettleSeconds) return;

        EditorApplication.update -= WaitThenPlay;
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        EditorApplication.EnterPlaymode();
    }

    private static void OnPlayModeChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;

        Application.logMessageReceived += OnLog;
        Enemy.Died += OnEnemyDied;
        KeyPickup.KeyCollected += () => keyCollected = true;
        WaveSpawner.AllWavesCleared += () => allWavesCleared = true;

        startRealtime = phaseStartRealtime = EditorApplication.timeSinceStartup;
        phase = Phase.Checks;
        EditorApplication.update += Tick;
    }

    private static void OnLog(string message, string stack, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            errors.Add($"{type}: {message}");
    }

    private static void OnEnemyDied(Enemy enemy)
    {
        deaths++;
        hitsTaken.TryGetValue(enemy, out int hits);
        // a primit si lovitura finala in acelasi cadru (health a ajuns la 0)
        if (lastHealth.TryGetValue(enemy, out float before) && before > enemy.Health.CurrentHealth) hits++;
        if (enemy.Data != null && enemy.Data.name == "Zombie") zombieHitsToKill.Add(hits);
        hitsTaken.Remove(enemy);
        lastHealth.Remove(enemy);
    }

    private static void Check(bool condition, string description)
    {
        results.Add((condition ? "PASS  " : "FAIL  ") + description);
        if (!condition) failures++;
    }

    private static double PhaseTime => EditorApplication.timeSinceStartup - phaseStartRealtime;

    private static void SetPhase(Phase next)
    {
        phase = next;
        phaseStartRealtime = EditorApplication.timeSinceStartup;
    }

    private static void Tick()
    {
        if (!EditorApplication.isPlaying) return;

        if (EditorApplication.timeSinceStartup - startRealtime > TimeoutSeconds)
        {
            Check(false, $"Timeout in faza {phase}");
            Finish();
            return;
        }

        try { Step(); }
        catch (Exception e)
        {
            Check(false, $"Exceptie in bot ({phase}): {e.Message}");
            Finish();
        }
    }

    private static void Step()
    {
        PlayerHealth player = PlayerHealth.Current;
        Observe();

        switch (phase)
        {
            case Phase.Checks:
            {
                var systems = GameObject.Find("EnemySystems");
                Check(systems != null, "GameScene are obiectul EnemySystems");
                var spawner = systems != null ? systems.GetComponent<WaveSpawner>() : null;
                Check(spawner != null && spawner.TotalWaves == 3, "WaveSpawner are 3 valuri (Level1_Waves)");
                Check(systems != null && systems.GetComponent<EnemyFactory>() != null, "EnemyFactory exista");
                Check(systems != null && systems.GetComponent<LootDropper>() != null, "LootDropper exista");
                Check(player != null, "Player are PlayerHealth");
                Check(player != null && player.CompareTag("Player"), "Player are tag-ul Player");
                Check(player != null && player.GetComponent<PlayerWallet>() != null, "Player are PlayerWallet");
                var weapon = player != null ? player.GetComponent<WeaponController>() : null;
                Check(weapon != null && weapon.CurrentWeapon != null && weapon.CurrentWeapon.name == "Sword",
                    "Player are WeaponController cu Sabia echipata");
                var weaponRenderer = player != null ? player.transform.Find("WeaponPivot/WeaponSprite")?.GetComponent<SpriteRenderer>() : null;
                Check(weaponRenderer != null && weaponRenderer.sprite != null, "Arma are sprite vizibil");
                Check(player != null && player.CurrentHealth == 5, "Player porneste cu 5 vieti");
                SetPhase(Phase.WaitSpawn);
                break;
            }

            case Phase.WaitSpawn:
                if (Enemies().Count > 0)
                {
                    Check(sawMarker, "Marcajul de spawn apare inainte de inamic");
                    Enemy e = Enemies()[0];
                    firstEnemyDistance = Vector2.Distance(e.transform.position, player.transform.position);
                    Check(firstEnemyDistance >= 3.9f, $"Inamicul apare departe de jucator ({firstEnemyDistance:F1} unitati, minim 4)");
                    SetPhase(Phase.WaitPlayerHit);
                }
                else if (PhaseTime > 15) { Check(false, "Niciun inamic n-a aparut in 15 s"); Finish(); }
                break;

            case Phase.WaitPlayerHit:
                // Jucatorul sta pe loc; inamicii trebuie sa vina si sa-l loveasca.
                if (player.CurrentHealth < 5)
                {
                    Check(true, $"Inamicii vin spre jucator si il lovesc (viata {player.CurrentHealth}/5)");
                    int healthAfterHit = player.CurrentHealth;
                    bool secondHit = player.TakeDamage(new DamageInfo(1, Vector2.right, 0, null));
                    Check(!secondHit && player.CurrentHealth == healthAfterHit, "Invulnerabilitate imediat dupa lovitura");
                    MakeInvulnerable(player, true); // ca botul sa poata termina nivelul
                    SetPhase(Phase.Fight);
                }
                else if (PhaseTime > 20) { Check(false, "Jucatorul n-a fost lovit in 20 s"); Finish(); }
                break;

            case Phase.Fight:
                FightNearest(player);
                if (allWavesCleared)
                {
                    Check(true, $"Toate cele 3 valuri au fost terminate ({deaths} inamici omorati)");
                    Check(deaths == 15, $"Numar corect de inamici in nivelul 1 (asteptat 15, gasit {deaths})");
                    SetPhase(Phase.CollectKey);
                }
                break;

            case Phase.CollectKey:
            {
                var key = UnityEngine.Object.FindAnyObjectByType<KeyPickup>();
                if (key != null) MoveTowards(player, key.transform.position);
                if (keyCollected || PhaseTime > 15)
                {
                    Check(keysSeen == 1, $"Ultimul inamic a lasat exact o cheie (gasite: {keysSeen})");
                    Check(keyCollected, "Cheia a fost colectata");
                    var wallet = player.GetComponent<PlayerWallet>();
                    Check(coinsSpawnedSeen == deaths / 3, $"Coin-uri: 1 la 3 inamici ({coinsSpawnedSeen} coin-uri la {deaths} inamici)");
                    Check(wallet.Coins == coinsSpawnedSeen, $"Toate coin-urile au fost colectate (portofel: {wallet.Coins})");
                    bool allTwo = zombieHitsToKill.Count > 0 && zombieHitsToKill.TrueForAll(h => h == 2);
                    Check(allTwo, $"Zombie moare din exact 2 lovituri de sabie (observat: {string.Join(",", zombieHitsToKill)})");
                    SetPhase(Phase.DeathTest);
                }
                break;
            }

            case Phase.DeathTest:
                MakeInvulnerable(player, false);
                for (int i = 0; i < 10 && !player.IsDead; i++)
                {
                    player.TakeDamage(new DamageInfo(1, Vector2.right, 0, null));
                    MakeInvulnerable(player, false);
                }
                Check(player.IsDead && player.CurrentHealth == 0, "Jucatorul moare la 0 vieti");
                deadPlayer = player;
                SetPhase(Phase.WaitRestart);
                break;

            case Phase.WaitRestart:
                if (player != null && player != deadPlayer && !player.IsDead)
                {
                    Check(player.CurrentHealth == 5, "Dupa moarte, nivelul se reincarca (viata din nou 5/5)");
                    Finish();
                }
                else if (PhaseTime > 6) { Check(false, "Scena nu s-a reincarcat dupa moarte"); Finish(); }
                break;
        }
    }

    // Urmareste ce exista in scena: marcaje, coin-uri, chei, lovituri primite de inamici.
    private static void Observe()
    {
        if (UnityEngine.Object.FindAnyObjectByType<SpawnMarker>() != null) sawMarker = true;

        foreach (var coin in UnityEngine.Object.FindObjectsByType<CoinPickup>())
            if (seenCoins.Add(coin)) coinsSpawnedSeen++;
        if (keysSeen == 0 && UnityEngine.Object.FindAnyObjectByType<KeyPickup>() != null) keysSeen = 1;

        foreach (Enemy e in Enemies())
        {
            float hp = e.Health.CurrentHealth;
            if (lastHealth.TryGetValue(e, out float before))
            {
                if (hp < before - 0.01f) hitsTaken[e] = (hitsTaken.TryGetValue(e, out int n) ? n : 0) + 1;
                else if (hp > before + 0.01f) hitsTaken[e] = 0; // reutilizat din pool
            }
            lastHealth[e] = hp;
        }
    }

    private static List<Enemy> Enemies()
    {
        var list = new List<Enemy>();
        foreach (Enemy e in UnityEngine.Object.FindObjectsByType<Enemy>())
            if (e.isActiveAndEnabled && !e.Health.IsDead) list.Add(e);
        return list;
    }

    private static void FightNearest(PlayerHealth player)
    {
        Enemy target = null;
        float best = float.MaxValue;
        foreach (Enemy e in Enemies())
        {
            float d = Vector2.Distance(e.transform.position, player.transform.position);
            if (d < best) { best = d; target = e; }
        }

        // Coin-urile ramase pe jos: botul trece pe langa ele cand nu are inamic.
        if (target == null)
        {
            var coin = UnityEngine.Object.FindAnyObjectByType<CoinPickup>();
            if (coin != null) MoveTowards(player, coin.transform.position);
            return;
        }

        if (best > 1.1f)
        {
            MoveTowards(player, target.transform.position);
            return;
        }

        var weapon = player.GetComponent<WeaponController>();
        if (weapon.IsAttacking || Time.time < nextAttackTime) return;

        // Tintim spre inamic exact cum ar face mouse-ul, apoi atacam.
        Transform pivot = player.transform.Find("WeaponPivot");
        Vector2 dir = target.transform.position - pivot.position;
        pivot.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f);
        weapon.CurrentWeapon.Attack(weapon);
        nextAttackTime = Time.time + 1f / weapon.CurrentWeapon.AttacksPerSecond;
    }

    private static void MoveTowards(PlayerHealth player, Vector3 target)
    {
        Vector2 dir = target - player.transform.position;
        if (dir.sqrMagnitude < 0.01f) return;
        // Folosim knockback-ul ca "input" (in batchmode nu exista tastatura).
        player.GetComponent<PlayerScript>().ApplyKnockback(dir.normalized * 5f, 0.05f);
    }

    private static void MakeInvulnerable(PlayerHealth player, bool on)
    {
        FieldInfo field = typeof(PlayerHealth).GetField("invulnerableUntil", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(player, on ? float.MaxValue : 0f);
    }

    private static void Finish()
    {
        EditorApplication.update -= Tick;
        Application.logMessageReceived -= OnLog;
        Enemy.Died -= OnEnemyDied;
        SessionState.EraseBool(ActiveKey);
        SessionState.EraseInt(RetriesKey);

        Check(errors.Count == 0, errors.Count == 0
            ? "Nicio eroare/exceptie in Console"
            : "Erori in Console:\n        " + string.Join("\n        ", errors));

        double seconds = EditorApplication.timeSinceStartup - startRealtime;
        string report = $"===== AUTO PLAYTEST ({seconds:F0} s) =====\n" + string.Join("\n", results) +
                        $"\n===== {(failures == 0 ? "TOATE TESTELE AU TRECUT" : failures + " TESTE ESUATE")} =====";
        Debug.Log(report);

        if (commandLine)
            EditorApplication.Exit(failures == 0 ? 0 : 1);
        else
            EditorApplication.ExitPlaymode();
    }
}
