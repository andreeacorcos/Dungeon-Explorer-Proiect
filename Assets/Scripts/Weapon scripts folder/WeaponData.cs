/*
Generic weapon script
It has characteristic used by any subclass of weapons
*/

using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Scriptable Objects/WeaponData")]
public abstract class WeaponData : ScriptableObject
{
    [Header("Weapon statistics")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attacksPerSecond = 2f;

    [Header("Visuals")]
    [SerializeField] protected Sprite weaponSprite;

    public float Damage => damage;
    public float AttacksPerSecond => attacksPerSecond;
    public Sprite WeaponSprite => weaponSprite;

    public abstract void Attack(WeaponController controller); 
}
