/*
Script de editor care configureaza automat sistemul de lupta, ca nimeni sa nu trebuiasca
sa lege manual zeci de referinte in Inspector:

  Meniu: Tools > Dungeon Explorer > Setup Combat

Ce face (poate fi rulat de mai multe ori; asset-urile existente NU sunt suprascrise):
  1. sprite-uri provizorii (placeholder) in Assets/Art/Placeholders
  2. armele: Sabie, Sulita, Ciocan (Assets/Data/Weapons)
  3. inamicii: Zombie, Zombie alergator, Zombie bruta (Assets/Data/Enemies)
  4. valurile pentru nivelele 1-3 (Assets/Data/Waves)
  5. prefab-urile: Enemy, Coin, Key, SpawnMarker
  6. Player prefab: tag Player, viata, portofel, arma legata de hitbox
  7. GameScene: obiectul "EnemySystems" (fabrica, valuri, drop-uri, HUD de debug)
  8. Build Profiles: Main Menu (0) + GameScene (1), ca butonul Play din meniu sa mearga
*/
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class DungeonCombatSetup
{
    private const string ArtFolder = "Assets/Art/Placeholders";
    private const string WeaponFolder = "Assets/Data/Weapons";
    private const string EnemyFolder = "Assets/Data/Enemies";
    private const string WaveFolder = "Assets/Data/Waves";
    private const string PrefabFolder = "Assets/Prefabs/Combat";
    private const string PlayerPrefabPath = "Assets/Prefabs/Player.prefab";
    private const string GameScenePath = "Assets/Scenes/GameScene.unity";
    private const string MainMenuScenePath = "Assets/Scenes/Main Menu.unity";
    private const int PixelsPerUnit = 32;

    [MenuItem("Tools/Dungeon Explorer/Setup Combat")]
    public static void Run()
    {
        EnsureFolder(ArtFolder);
        EnsureFolder(WeaponFolder);
        EnsureFolder(EnemyFolder);
        EnsureFolder(WaveFolder);
        EnsureFolder(PrefabFolder);

        // 1. Sprite-uri provizorii
        Sprite enemySprite = MakeSprite("enemy_blob", 32, 32, DrawBlob);
        Sprite coinSprite = MakeSprite("coin", 16, 16, DrawCoin);
        Sprite keySprite = MakeSprite("key", 16, 16, DrawKey);
        Sprite markerSprite = MakeSprite("spawn_marker", 32, 32, DrawRing);
        Sprite swordSprite = MakeSprite("weapon_sword", 8, 32, DrawSword);
        Sprite spearSprite = MakeSprite("weapon_spear", 6, 48, DrawSpear);
        Sprite hammerSprite = MakeSprite("weapon_hammer", 20, 36, DrawHammer);

        // 2. Arme
        MeleeWeaponData sword = MakeWeapon("Sword", swordSprite, MeleeAttackType.Swing,
            damage: 10f, attacksPerSecond: 2f, knockback: 5f, duration: 0.25f, swingAngle: 120f, stabDistance: 0f,
            holdDistance: 0.7f, hitbox: new Vector2(0.35f, 1f));
        MakeWeapon("Spear", spearSprite, MeleeAttackType.Stab,
            damage: 10f, attacksPerSecond: 1.5f, knockback: 4f, duration: 0.3f, swingAngle: 0f, stabDistance: 1f,
            holdDistance: 0.9f, hitbox: new Vector2(0.25f, 1.5f));
        MakeWeapon("Hammer", hammerSprite, MeleeAttackType.Swing,
            damage: 20f, attacksPerSecond: 0.8f, knockback: 10f, duration: 0.4f, swingAngle: 180f, stabDistance: 0f,
            holdDistance: 0.7f, hitbox: new Vector2(0.7f, 1.1f));

        // 3. Inamici (sabia face 10: Zombie = 2 lovituri, Bruta = 5)
        EnemyData zombie = MakeEnemy("Zombie", enemySprite, new Color(0.45f, 0.78f, 0.4f), scale: 1f,
            maxHealth: 20f, speed: 1.6f, knockbackResistance: 0f,
            damage: 1, range: 0.25f, windup: 0.45f, cooldown: 1f, attackKnockback: 6f);
        EnemyData runner = MakeEnemy("ZombieRunner", enemySprite, new Color(0.8f, 0.85f, 0.35f), scale: 0.8f,
            maxHealth: 15f, speed: 3f, knockbackResistance: 0f,
            damage: 1, range: 0.2f, windup: 0.3f, cooldown: 0.8f, attackKnockback: 5f);
        EnemyData brute = MakeEnemy("ZombieBrute", enemySprite, new Color(0.3f, 0.5f, 0.3f), scale: 1.5f,
            maxHealth: 50f, speed: 1f, knockbackResistance: 0.7f,
            damage: 2, range: 0.3f, windup: 0.7f, cooldown: 1.4f, attackKnockback: 10f);

        // 4. Valuri
        WaveData level1 = MakeWaves("Level1_Waves", 1f, new[]
        {
            Wave(2f, G(zombie, 3, 1.2f)),
            Wave(3f, G(zombie, 5, 0.9f)),
            Wave(3f, G(zombie, 7, 0.7f)),
        });
        MakeWaves("Level2_Waves", 1.5f, new[]
        {
            Wave(2f, G(zombie, 4, 1f)),
            Wave(3f, G(zombie, 3, 1f), G(runner, 3, 0.8f)),
            Wave(3f, G(zombie, 4, 0.8f), G(runner, 5, 0.6f)),
        });
        MakeWaves("Level3_Waves", 2f, new[]
        {
            Wave(2f, G(zombie, 4, 0.8f), G(runner, 2, 0.8f)),
            Wave(3f, G(brute, 1, 1f), G(runner, 4, 0.6f)),
            Wave(3f, G(brute, 2, 1.5f), G(zombie, 4, 0.6f), G(runner, 4, 0.5f)),
        });

        // 5. Prefab-uri
        Enemy enemyPrefab = MakeEnemyPrefab(enemySprite);
        GameObject coinPrefab = MakePickupPrefab<CoinPickup>("Coin", coinSprite);
        GameObject keyPrefab = MakePickupPrefab<KeyPickup>("Key", keySprite);
        GameObject markerPrefab = MakeMarkerPrefab(markerSprite);

        // 6. Player
        SetupPlayerPrefab(sword);

        // 7. Scena + 8. Build Profiles
        SetupBuildScenes();
        SetupGameScene(enemyPrefab, level1, coinPrefab, keyPrefab, markerPrefab);

        AssetDatabase.SaveAssets();
        Debug.Log("[Setup Combat] Gata. Deschide GameScene si apasa Play.");
    }

    // ---------------------------------------------------------------- arme, inamici, valuri

    private static MeleeWeaponData MakeWeapon(string name, Sprite sprite, MeleeAttackType type,
        float damage, float attacksPerSecond, float knockback, float duration, float swingAngle,
        float stabDistance, float holdDistance, Vector2 hitbox)
    {
        string path = $"{WeaponFolder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<MeleeWeaponData>(path);
        if (existing != null) return existing;

        var weapon = ScriptableObject.CreateInstance<MeleeWeaponData>();
        AssetDatabase.CreateAsset(weapon, path);

        var so = new SerializedObject(weapon);
        so.FindProperty("damage").floatValue = damage;
        so.FindProperty("attacksPerSecond").floatValue = attacksPerSecond;
        so.FindProperty("knockback").floatValue = knockback;
        so.FindProperty("weaponSprite").objectReferenceValue = sprite;
        so.FindProperty("holdDistance").floatValue = holdDistance;
        so.FindProperty("hitboxSize").vector2Value = hitbox;
        so.FindProperty("attackType").enumValueIndex = (int)type;
        so.FindProperty("attackDuration").floatValue = duration;
        so.FindProperty("swingAngle").floatValue = swingAngle;
        so.FindProperty("stabDistance").floatValue = stabDistance;
        so.ApplyModifiedPropertiesWithoutUndo();
        return weapon;
    }

    private static EnemyData MakeEnemy(string name, Sprite sprite, Color tint, float scale,
        float maxHealth, float speed, float knockbackResistance,
        int damage, float range, float windup, float cooldown, float attackKnockback)
    {
        string path = $"{EnemyFolder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
        if (existing != null) return existing;

        var data = ScriptableObject.CreateInstance<EnemyData>();
        AssetDatabase.CreateAsset(data, path);

        var so = new SerializedObject(data);
        so.FindProperty("sprite").objectReferenceValue = sprite;
        so.FindProperty("tint").colorValue = tint;
        so.FindProperty("scale").floatValue = scale;
        so.FindProperty("maxHealth").floatValue = maxHealth;
        so.FindProperty("moveSpeed").floatValue = speed;
        so.FindProperty("knockbackResistance").floatValue = knockbackResistance;
        so.FindProperty("attackDamage").intValue = damage;
        so.FindProperty("attackRange").floatValue = range;
        so.FindProperty("windupTime").floatValue = windup;
        so.FindProperty("attackCooldown").floatValue = cooldown;
        so.FindProperty("attackKnockback").floatValue = attackKnockback;
        so.ApplyModifiedPropertiesWithoutUndo();
        return data;
    }

    private static WaveData.SpawnGroup G(EnemyData enemy, int count, float interval) =>
        new WaveData.SpawnGroup { enemy = enemy, count = count, interval = interval };

    private static WaveData.Wave Wave(float delay, params WaveData.SpawnGroup[] groups) =>
        new WaveData.Wave { delayBeforeWave = delay, groups = new List<WaveData.SpawnGroup>(groups) };

    private static WaveData MakeWaves(string name, float healthMultiplier, WaveData.Wave[] waves)
    {
        string path = $"{WaveFolder}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<WaveData>(path);
        if (existing != null) return existing;

        var data = ScriptableObject.CreateInstance<WaveData>();
        AssetDatabase.CreateAsset(data, path);

        var so = new SerializedObject(data);
        so.FindProperty("healthMultiplier").floatValue = healthMultiplier;
        SerializedProperty list = so.FindProperty("waves");
        list.arraySize = waves.Length;
        for (int i = 0; i < waves.Length; i++)
        {
            SerializedProperty wave = list.GetArrayElementAtIndex(i);
            wave.FindPropertyRelative("delayBeforeWave").floatValue = waves[i].delayBeforeWave;
            SerializedProperty groups = wave.FindPropertyRelative("groups");
            groups.arraySize = waves[i].groups.Count;
            for (int g = 0; g < waves[i].groups.Count; g++)
            {
                SerializedProperty group = groups.GetArrayElementAtIndex(g);
                group.FindPropertyRelative("enemy").objectReferenceValue = waves[i].groups[g].enemy;
                group.FindPropertyRelative("count").intValue = waves[i].groups[g].count;
                group.FindPropertyRelative("interval").floatValue = waves[i].groups[g].interval;
            }
        }
        so.ApplyModifiedPropertiesWithoutUndo();
        return data;
    }

    // ---------------------------------------------------------------- prefab-uri

    private static Enemy MakeEnemyPrefab(Sprite sprite)
    {
        string path = $"{PrefabFolder}/Enemy.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<Enemy>(path);
        if (existing != null) return existing;

        var go = new GameObject("Enemy");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.4f;

        go.AddComponent<EnemyHealth>();
        go.AddComponent<EnemyBrain>();
        go.AddComponent<Enemy>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab.GetComponent<Enemy>();
    }

    private static GameObject MakePickupPrefab<T>(string name, Sprite sprite) where T : Pickup
    {
        string path = $"{PrefabFolder}/{name}.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject(name);
        go.AddComponent<T>();
        var visual = new GameObject("Visual"); // Pickup face sprite-ul copil sa pluteasca
        visual.transform.SetParent(go.transform, false);
        var sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 1;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static GameObject MakeMarkerPrefab(Sprite sprite)
    {
        string path = $"{PrefabFolder}/SpawnMarker.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("SpawnMarker");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.25f, 0.2f, 0.9f);
        sr.sortingOrder = 1;
        go.AddComponent<SpawnMarker>();

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static void SetupPlayerPrefab(WeaponData startingWeapon)
    {
        GameObject root = PrefabUtility.LoadPrefabContents(PlayerPrefabPath);
        try
        {
            root.tag = "Player";
            GetOrAdd<PlayerHealth>(root);
            GetOrAdd<PlayerWallet>(root);
            WeaponController controller = GetOrAdd<WeaponController>(root);

            Transform pivot = root.transform.Find("WeaponPivot");
            Transform weaponObject = pivot != null ? pivot.Find("WeaponSprite") : null;
            if (pivot == null || weaponObject == null)
            {
                Debug.LogError("[Setup Combat] Nu gasesc WeaponPivot/WeaponSprite in Player.prefab.");
                return;
            }

            var hitbox = weaponObject.GetComponent<BoxCollider2D>();
            var weaponRenderer = weaponObject.GetComponent<SpriteRenderer>();
            WeaponHitbox weaponHitbox = GetOrAdd<WeaponHitbox>(weaponObject.gameObject);

            hitbox.isTrigger = true;
            hitbox.size = startingWeapon.HitboxSize;
            weaponObject.localPosition = new Vector3(0f, startingWeapon.HoldDistance, 0f);
            weaponRenderer.sprite = startingWeapon.WeaponSprite;
            weaponRenderer.sortingOrder = 3;

            var so = new SerializedObject(controller);
            so.FindProperty("startingWeapon").objectReferenceValue = startingWeapon;
            so.FindProperty("weaponPivot").objectReferenceValue = pivot;
            so.FindProperty("weaponRenderer").objectReferenceValue = weaponRenderer;
            so.FindProperty("hitbox").objectReferenceValue = hitbox;
            so.FindProperty("weaponHitbox").objectReferenceValue = weaponHitbox;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    // ---------------------------------------------------------------- scena si build

    private static void SetupGameScene(Enemy enemyPrefab, WaveData waves, GameObject coinPrefab,
        GameObject keyPrefab, GameObject markerPrefab)
    {
        Scene scene = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);

        GameObject systems = null;
        foreach (GameObject rootObject in scene.GetRootGameObjects())
            if (rootObject.name == "EnemySystems") systems = rootObject;
        if (systems == null)
            systems = new GameObject("EnemySystems");

        EnemyFactory factory = GetOrAdd<EnemyFactory>(systems);
        WaveSpawner spawner = GetOrAdd<WaveSpawner>(systems);
        LootDropper loot = GetOrAdd<LootDropper>(systems);
        DebugHUD hud = GetOrAdd<DebugHUD>(systems);

        GameObject groundObject = GameObject.Find("GroundTilemap");
        Tilemap ground = groundObject != null ? groundObject.GetComponent<Tilemap>() : null;
        if (ground == null)
            Debug.LogWarning("[Setup Combat] Nu gasesc GroundTilemap: seteaza manual Spawn Area pe WaveSpawner.");

        // Reincarcam prefab-ul dupa cale: referinta la componenta poate deveni invalida dupa OpenScene.
        GameObject enemyAsset = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabFolder}/Enemy.prefab");
        if (enemyAsset != null) enemyPrefab = enemyAsset.GetComponent<Enemy>();
        if (enemyPrefab == null)
            Debug.LogError("[Setup Combat] Nu gasesc componenta Enemy pe Enemy.prefab.");

        var factorySo = new SerializedObject(factory);
        factorySo.FindProperty("enemyPrefab").objectReferenceValue = enemyPrefab;
        factorySo.ApplyModifiedPropertiesWithoutUndo();

        var spawnerSo = new SerializedObject(spawner);
        spawnerSo.FindProperty("factory").objectReferenceValue = factory;
        if (spawnerSo.FindProperty("waveData").objectReferenceValue == null)
            spawnerSo.FindProperty("waveData").objectReferenceValue = waves;
        if (ground != null)
            spawnerSo.FindProperty("spawnArea").objectReferenceValue = ground;
        spawnerSo.FindProperty("spawnMarkerPrefab").objectReferenceValue = markerPrefab;
        spawnerSo.FindProperty("keyPrefab").objectReferenceValue = keyPrefab;
        spawnerSo.ApplyModifiedPropertiesWithoutUndo();

        var lootSo = new SerializedObject(loot);
        lootSo.FindProperty("coinPrefab").objectReferenceValue = coinPrefab;
        lootSo.ApplyModifiedPropertiesWithoutUndo();

        var hudSo = new SerializedObject(hud);
        hudSo.FindProperty("waveSpawner").objectReferenceValue = spawner;
        hudSo.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static void SetupBuildScenes()
    {
        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(MainMenuScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };
        // Pastram si alte scene adaugate de colegi.
        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
            if (existing.path != MainMenuScenePath && existing.path != GameScenePath)
                scenes.Add(existing);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    // ---------------------------------------------------------------- utilitare

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        T component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }

    // ---------------------------------------------------------------- sprite-uri provizorii

    private delegate Color PixelPainter(int x, int y, int width, int height);

    private static Sprite MakeSprite(string name, int width, int height, PixelPainter painter)
    {
        string assetPath = $"{ArtFolder}/{name}.png";
        if (!File.Exists(assetPath))
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    texture.SetPixel(x, y, painter(x, y, width, height));
            texture.Apply();
            File.WriteAllBytes(assetPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath);

            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    private static readonly Color Clear = new Color(0f, 0f, 0f, 0f);

    // Corp alb (colorat apoi cu tint-ul din EnemyData), contur si ochi inchisi.
    private static Color DrawBlob(int x, int y, int w, int h)
    {
        float dx = x - 15.5f, dy = (y - 14.5f) * 0.9f;
        float d = Mathf.Sqrt(dx * dx + dy * dy);
        if (d > 13.5f) return Clear;
        bool eye = (y >= 17 && y <= 19) && ((x >= 10 && x <= 12) || (x >= 19 && x <= 21));
        if (eye) return new Color(0.1f, 0.05f, 0.05f);
        if (d > 12.3f) return new Color(0.25f, 0.25f, 0.25f);
        return Color.white;
    }

    private static Color DrawCoin(int x, int y, int w, int h)
    {
        float dx = x - 7.5f, dy = y - 7.5f;
        float d = Mathf.Sqrt(dx * dx + dy * dy);
        if (d > 6.5f) return Clear;
        if (d > 5.5f) return new Color(0.7f, 0.45f, 0.05f);
        if (x >= 5 && x <= 6 && y >= 8 && y <= 10) return new Color(1f, 0.97f, 0.7f);
        return new Color(1f, 0.8f, 0.15f);
    }

    private static Color DrawKey(int x, int y, int w, int h)
    {
        Color gold = new Color(1f, 0.82f, 0.2f);
        float dx = x - 4.5f, dy = y - 8.5f;
        float d = Mathf.Sqrt(dx * dx + dy * dy);
        if (d <= 3.6f && d >= 1.6f) return gold;             // inelul
        if (y >= 8 && y <= 9 && x >= 8 && x <= 14) return gold; // tija
        if (x >= 11 && x <= 12 && y >= 5 && y <= 7) return gold; // dinte 1
        if (x >= 13 && x <= 14 && y >= 6 && y <= 7) return gold; // dinte 2
        return Clear;
    }

    private static Color DrawRing(int x, int y, int w, int h)
    {
        float dx = x - 15.5f, dy = y - 15.5f;
        float d = Mathf.Sqrt(dx * dx + dy * dy);
        return d <= 14f && d >= 11.5f ? Color.white : Clear;
    }

    private static Color DrawSword(int x, int y, int w, int h)
    {
        Color steel = new Color(0.85f, 0.88f, 0.92f), edge = new Color(0.55f, 0.6f, 0.68f);
        Color guard = new Color(0.55f, 0.4f, 0.15f), grip = new Color(0.35f, 0.2f, 0.1f);
        if (y <= 5) return (x >= 3 && x <= 4) ? grip : Clear;   // maner
        if (y <= 7) return guard;                               // garda
        if (y >= 30) return (x >= 3 && x <= 4) ? steel : Clear; // varf
        if (x >= 2 && x <= 5) return (x == 2 || x == 5) ? edge : steel;
        return Clear;
    }

    private static Color DrawSpear(int x, int y, int w, int h)
    {
        Color shaft = new Color(0.55f, 0.35f, 0.15f), tip = new Color(0.8f, 0.83f, 0.88f);
        if (y >= 38)
        {
            int half = Mathf.Max(0, (47 - y) / 3);
            return Mathf.Abs(x - 2.5f) <= half + 0.5f ? tip : Clear;
        }
        return (x >= 2 && x <= 3) ? shaft : Clear;
    }

    private static Color DrawHammer(int x, int y, int w, int h)
    {
        Color head = new Color(0.55f, 0.57f, 0.62f), dark = new Color(0.35f, 0.37f, 0.42f);
        Color handle = new Color(0.5f, 0.32f, 0.14f);
        if (y >= 24) return (y == 24 || y == 35 || x == 0 || x == 19) ? dark : head;
        return (x >= 9 && x <= 10) ? handle : Clear;
    }
}
