/*
Asculta Enemy.Died (Observer) si decide daca inamicul lasa un coin, folosind un shuffle bag.
Cheia de nivel o lasa WaveSpawner, pentru ca doar el stie care e ultimul inamic.
*/
using UnityEngine;

public class LootDropper : MonoBehaviour
{
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private int coinsInBag = 1;   // 1 coin...
    [SerializeField] private int emptiesInBag = 2; // ...la fiecare 3 inamici

    private readonly ShuffleBag<bool> bag = new ShuffleBag<bool>();

    private void Awake()
    {
        bag.Add(true, coinsInBag);
        bag.Add(false, emptiesInBag);
    }

    private void OnEnable() => Enemy.Died += OnEnemyDied;
    private void OnDisable() => Enemy.Died -= OnEnemyDied;

    private void OnEnemyDied(Enemy enemy)
    {
        if (coinPrefab != null && bag.Next())
            Instantiate(coinPrefab, enemy.transform.position, Quaternion.identity);
    }
}
