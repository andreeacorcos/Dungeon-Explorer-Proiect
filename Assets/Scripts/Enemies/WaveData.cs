/*
Valurile unui nivel, ca date (Create > Enemies > Wave Data).
Exemplu: Val 1 = 3 zombie, cate unul la 1.2 s; Val 2 = ...
Nivelele urmatoare au alt asset, cu healthMultiplier mai mare (zombii mor din mai multe lovituri).
*/
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Waves", menuName = "Enemies/Wave Data")]
public class WaveData : ScriptableObject
{
    [Serializable]
    public class SpawnGroup
    {
        public EnemyData enemy;
        public int count = 3;
        public float interval = 1f; // secunde intre doi inamici din grup
    }

    [Serializable]
    public class Wave
    {
        public float delayBeforeWave = 2f;
        public List<SpawnGroup> groups = new List<SpawnGroup>();
    }

    [SerializeField] private float healthMultiplier = 1f;
    [SerializeField] private List<Wave> waves = new List<Wave>();

    public float HealthMultiplier => healthMultiplier;
    public IReadOnlyList<Wave> Waves => waves;
}
