/*
Factory + Object Pool (curs C7):
  - spawner-ul (WaveSpawner) decide CE inamic apare si CAND;
  - fabrica stie CUM se construieste: ia o instanta din pool, aplica datele din EnemyData,
    scaleaza viata dupa nivel si o pune la pozitie.
Toata lumea care vrea un inamic (valuri, cheat-uri, viitoare capcane) trece prin Create,
deci un bug de initializare se repara intr-un singur loc.
*/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemyFactory : MonoBehaviour
{
    [SerializeField] private Enemy enemyPrefab;
    [SerializeField] private int prewarmCount = 10; // instante create la incarcarea nivelului
    [SerializeField] private int maxPoolSize = 50;

    private ObjectPool<Enemy> pool;
    private readonly List<Enemy> active = new List<Enemy>();

    public int ActiveCount => active.Count;

    private void Awake()
    {
        pool = new ObjectPool<Enemy>(
            createFunc: CreateInstance,
            actionOnGet: e => e.gameObject.SetActive(true),
            actionOnRelease: e => e.gameObject.SetActive(false),
            actionOnDestroy: e => { if (e != null) Destroy(e.gameObject); },
            collectionCheck: true,
            defaultCapacity: prewarmCount,
            maxSize: maxPoolSize);

        Prewarm();
    }

    // Umplem pool-ul la incarcare, nu in timpul primului val (evita sacadarile).
    private void Prewarm()
    {
        var warm = new List<Enemy>(prewarmCount);
        for (int i = 0; i < prewarmCount; i++)
            warm.Add(pool.Get());
        foreach (Enemy e in warm)
            pool.Release(e);
    }

    private Enemy CreateInstance()
    {
        Enemy enemy = Instantiate(enemyPrefab, transform);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    public Enemy Create(EnemyData data, Vector2 position, float healthMultiplier = 1f)
    {
        Enemy enemy = pool.Get();
        enemy.Init(data, healthMultiplier, position, Release);
        active.Add(enemy);
        return enemy;
    }

    private void Release(Enemy enemy)
    {
        active.Remove(enemy);
        pool.Release(enemy);
    }
}
