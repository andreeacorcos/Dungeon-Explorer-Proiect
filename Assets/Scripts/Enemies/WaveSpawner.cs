/*
Ruleaza valurile din WaveData: decide CE inamic apare si CAND, iar EnemyFactory il construieste.
  - inamicii apar in interiorul camerei (GroundTilemap fara margine), departe de jucator;
  - inainte de spawn apare un marcaj, ca jucatorul sa nu primeasca damage din senin;
  - valul urmator incepe dupa ce toti inamicii din valul curent au murit;
  - ultimul inamic din ultimul val lasa cheia pentru nivelul urmator.
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WaveSpawner : MonoBehaviour
{
    public static event Action<int, int> WaveStarted; // (valul curent, total valuri)
    public static event Action AllWavesCleared;

    [SerializeField] private WaveData waveData;
    [SerializeField] private EnemyFactory factory;

    [Header("Spawn area")]
    [SerializeField] private Tilemap spawnArea;          // GroundTilemap
    [SerializeField] private int edgeInset = 1;          // nu spawnam pe peretii de pe margine
    [SerializeField] private float minDistanceFromPlayer = 4f;

    [Header("Telegraph & drops")]
    [SerializeField] private GameObject spawnMarkerPrefab;
    [SerializeField] private float telegraphTime = 0.6f;
    [SerializeField] private GameObject keyPrefab;

    [SerializeField] private bool autoStart = true;

    private int pendingSpawns;      // inamici anuntati de marcaj, dar inca neaparuti
    private bool allSpawnsQueued;   // ultimul val si-a programat toti inamicii

    public int CurrentWave { get; private set; }
    public int TotalWaves => waveData != null ? waveData.Waves.Count : 0;
    public int AliveCount => factory != null ? factory.ActiveCount : 0;
    public bool Finished { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        WaveStarted = null;
        AllWavesCleared = null;
    }

    private void OnEnable() => Enemy.Died += OnEnemyDied;
    private void OnDisable() => Enemy.Died -= OnEnemyDied;

    private void Start()
    {
        if (spawnArea != null) spawnArea.CompressBounds();
        if (autoStart) StartWaves();
    }

    public void StartWaves()
    {
        if (waveData == null || factory == null)
        {
            Debug.LogWarning("WaveSpawner: lipseste WaveData sau EnemyFactory.", this);
            return;
        }
        StopAllCoroutines();
        StartCoroutine(RunWaves());
    }

    private IEnumerator RunWaves()
    {
        for (int i = 0; i < waveData.Waves.Count; i++)
        {
            WaveData.Wave wave = waveData.Waves[i];
            yield return new WaitForSeconds(wave.delayBeforeWave);

            CurrentWave = i + 1;
            WaveStarted?.Invoke(CurrentWave, TotalWaves);

            foreach (WaveData.SpawnGroup group in wave.groups)
            {
                if (group.enemy == null) continue;
                for (int n = 0; n < group.count; n++)
                {
                    StartCoroutine(SpawnWithTelegraph(group.enemy));
                    yield return new WaitForSeconds(group.interval);
                }
            }

            if (i == waveData.Waves.Count - 1)
                allSpawnsQueued = true;

            // Valul se termina cand nu mai e niciun inamic, nici in asteptare, nici in viata.
            yield return new WaitUntil(() => pendingSpawns == 0 && factory.ActiveCount == 0);
        }

        Finished = true;
        AllWavesCleared?.Invoke();
    }

    private IEnumerator SpawnWithTelegraph(EnemyData enemy)
    {
        pendingSpawns++;
        Vector2 position = PickSpawnPosition();

        GameObject marker = null;
        if (spawnMarkerPrefab != null)
            marker = Instantiate(spawnMarkerPrefab, position, Quaternion.identity);

        yield return new WaitForSeconds(telegraphTime);

        if (marker != null) Destroy(marker);
        factory.Create(enemy, position, waveData.HealthMultiplier);
        pendingSpawns--;
    }

    private void OnEnemyDied(Enemy enemy)
    {
        // Inamicul care moare e inca numarat in ActiveCount (iese din pool dupa eveniment).
        bool isLastEnemy = allSpawnsQueued && pendingSpawns == 0 && factory.ActiveCount == 1;
        if (isLastEnemy && keyPrefab != null)
            Instantiate(keyPrefab, enemy.transform.position, Quaternion.identity);
    }

    private Vector2 PickSpawnPosition()
    {
        if (spawnArea == null)
            return transform.position;

        BoundsInt bounds = spawnArea.cellBounds;
        int xMin = bounds.xMin + edgeInset, xMax = bounds.xMax - 1 - edgeInset;
        int yMin = bounds.yMin + edgeInset, yMax = bounds.yMax - 1 - edgeInset;
        if (xMin > xMax || yMin > yMax)
            return spawnArea.GetCellCenterWorld(Vector3Int.RoundToInt(bounds.center));

        Vector2 playerPos = PlayerHealth.Current != null
            ? (Vector2)PlayerHealth.Current.transform.position
            : new Vector2(float.MaxValue, float.MaxValue);

        // Incercam cateva pozitii aleatoare; daca toate sunt prea aproape, o pastram pe cea mai departata.
        Vector2 best = Vector2.zero;
        float bestDistance = -1f;
        for (int attempt = 0; attempt < 20; attempt++)
        {
            var cell = new Vector3Int(UnityEngine.Random.Range(xMin, xMax + 1),
                                      UnityEngine.Random.Range(yMin, yMax + 1), 0);
            if (!spawnArea.HasTile(cell)) continue;

            Vector2 candidate = spawnArea.GetCellCenterWorld(cell);
            float distance = Vector2.Distance(candidate, playerPos);
            if (distance >= minDistanceFromPlayer)
                return candidate;
            if (distance > bestDistance)
            {
                bestDistance = distance;
                best = candidate;
            }
        }
        return bestDistance >= 0f ? best : (Vector2)spawnArea.GetCellCenterWorld(Vector3Int.RoundToInt(bounds.center));
    }
}
