/*
Generic weapon script
It has characteristic used by any subclass of weapons
*/

using UnityEngine;

// Clasa e abstracta, deci nu are CreateAssetMenu: asset-urile se creeaza din subclase (ex: MeleeWeaponData).
public abstract class WeaponData : ScriptableObject
{
    [Header("Weapon statistics")]
    [SerializeField] protected float damage = 10f;
    [SerializeField] protected float attacksPerSecond = 2f;
    [SerializeField] protected float knockback = 5f; // cat de tare impinge inamicul lovit

    [Header("Visuals")]
    [SerializeField] protected Sprite weaponSprite;

    [Header("Hitbox")]
    [SerializeField] protected float holdDistance = 0.7f;                     // distanta fata de pivot (in directia mouse-ului)
    [SerializeField] protected Vector2 hitboxSize = new Vector2(0.35f, 1f);   // marimea collider-ului armei

    public float Damage => damage;
    public float AttacksPerSecond => attacksPerSecond;
    public float Knockback => knockback;
    public Sprite WeaponSprite => weaponSprite;
    public float HoldDistance => holdDistance;
    public Vector2 HitboxSize => hitboxSize;

    public abstract void Attack(WeaponController controller);
}
