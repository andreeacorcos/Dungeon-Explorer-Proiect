/*This script handles:
// -- Playing weapon animations (TODO)
// -- Handling weapon attacks
// -- Aim weapon towards the mouse
*/
using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponData startingWeapon; // "Default" weapon
    [SerializeField] private Transform weaponPivot; // The point which the weapon spins around
    //[SerializeField] private SpriteRenderer weaponRenderer;
    [SerializeField] private BoxCollider2D hitbox; // Hitbox of the weapon
    private WeaponData currentWeapon; // The actual weapon
    private bool isAttacking = false; // Used to enable/disable weapon hitbox
    private float nextAttackTime = 0f; // Used for attack cooldown
    
    private void Awake()
    {
        EquipWeapon(startingWeapon);
        hitbox.enabled = false;
    }
    
    void Start()
    {
        
    }
    void AimAtMouse(){
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -Camera.main.transform.position.z;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        //converts coordinates from (0,0) - (1920,1080) screen positions to world coordinates
        Vector2 direction = mousePosition - weaponPivot.position;
        //calculates direction between converted mouse position and weapon pivot's position
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        //Atan2 calculates angle of a vector2 relative to x axis. It returns the result in
        //radians therefore we convert it to degrees then compensate for the default weapon pivot offset
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void EquipWeapon(WeaponData newWeapon){
        currentWeapon = newWeapon;
    }

    // Update is called once per frame
    void Update()
    {
        AimAtMouse();
        if(Input.GetButton("Fire1"))
            TryAttack();
    }

    void TryAttack(){
        if(currentWeapon == null || isAttacking || Time.time < nextAttackTime)
            return; //If the player doesn't have a weapon, is already attacking or attack is on cooldown, don't attack
        currentWeapon.Attack(this);
    }

    public void PerformMeleeAttack(MeleeWeaponData weapon)
    {
        if (isAttacking)
            return;

        StartCoroutine(MeleeAnimation(weapon));
        nextAttackTime = Time.time + 1f / weapon.AttacksPerSecond;
    }

    private IEnumerator MeleeAnimation(MeleeWeaponData weapon)
    {
        isAttacking = true;
        hitbox.enabled = true;

        Vector3 originalPosition = weaponPivot.localPosition;
        Quaternion originalRotation = weaponPivot.localRotation;

        float elapsed = 0f;

        if (weapon.AttackType == MeleeAttackType.Swing)
        {
            // Sweep from one side to the other.
            while (elapsed < weapon.AttackDuration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsed / weapon.AttackDuration);

                float angle = Mathf.Lerp(
                    -weapon.SwingAngle / 2f,
                    weapon.SwingAngle / 2f,
                    progress);

                weaponPivot.localRotation =
                    originalRotation * Quaternion.Euler(0f, 0f, angle);

                yield return null;
            }
        }
        else if (weapon.AttackType == MeleeAttackType.Stab)
        {
            // Move forward, then return.
            float halfDuration = weapon.AttackDuration / 2f;

            while (elapsed < weapon.AttackDuration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsed / halfDuration);

                float distance;

                if (progress <= 1f)
                    distance = Mathf.Lerp(0f, weapon.StabDistance, progress);
                else
                    distance = Mathf.Lerp(
                        weapon.StabDistance, 0f, progress - 1f);

                weaponPivot.localPosition =
                    originalPosition + Vector3.up * distance;

                yield return null;
            }
        }

        weaponPivot.localPosition = originalPosition;
        weaponPivot.localRotation = originalRotation;

        hitbox.enabled = false;
        isAttacking = false;
    }
}
