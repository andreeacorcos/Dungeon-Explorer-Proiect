/*This script handles:
// -- Playing weapon animations (TODO)
// -- Handling weapon attacks
// -- Aim weapon towards the mouse
// -- Dealing damage through WeaponHitbox
*/
using UnityEngine;
using System.Collections;

public class WeaponController : MonoBehaviour
{
    [Header("Weapon")]
    [SerializeField] private WeaponData startingWeapon; // "Default" weapon
    [SerializeField] private Transform weaponPivot; // The point which the weapon spins around
    [SerializeField] private SpriteRenderer weaponRenderer; // Shows the weapon sprite
    [SerializeField] private BoxCollider2D hitbox; // Hitbox of the weapon
    [SerializeField] private WeaponHitbox weaponHitbox; // Detects what the hitbox touches

    [Header("Game feel")]
    [SerializeField] private float hitStopDuration = 0.05f; // freeze on impact (0 = off)

    private WeaponData currentWeapon; // The actual weapon
    private bool isAttacking = false; // Used to enable/disable weapon hitbox
    private float nextAttackTime = 0f; // Used for attack cooldown
    private Camera mainCamera;

    public bool IsAttacking => isAttacking;
    public WeaponData CurrentWeapon => currentWeapon;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (weaponHitbox == null && hitbox != null)
            weaponHitbox = hitbox.GetComponent<WeaponHitbox>();
        if (weaponHitbox != null)
            weaponHitbox.Init(this);

        EquipWeapon(startingWeapon);
        hitbox.enabled = false;
    }

    void AimAtMouse(){
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = -mainCamera.transform.position.z;
        mousePosition = mainCamera.ScreenToWorldPoint(mousePosition);
        //converts coordinates from (0,0) - (1920,1080) screen positions to world coordinates
        Vector2 direction = mousePosition - weaponPivot.position;
        //calculates direction between converted mouse position and weapon pivot's position
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        //Atan2 calculates angle of a vector2 relative to x axis. It returns the result in
        //radians therefore we convert it to degrees then compensate for the default weapon pivot offset
        weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    // Public, so that weapon pickups on later levels can change the weapon.
    public void EquipWeapon(WeaponData newWeapon){
        currentWeapon = newWeapon;
        if (newWeapon == null) return;

        // The weapon points along the pivot's local "up" (see the -90 in AimAtMouse).
        if (hitbox != null)
        {
            hitbox.transform.localPosition = new Vector3(0f, newWeapon.HoldDistance, 0f);
            hitbox.size = newWeapon.HitboxSize;
            hitbox.offset = Vector2.zero;
        }
        if (weaponRenderer != null)
            weaponRenderer.sprite = newWeapon.WeaponSprite;
    }

    // Update is called once per frame
    void Update()
    {
        // While attacking, the animation controls the rotation.
        if (!isAttacking)
            AimAtMouse();
        if(Input.GetButton("Fire1"))
            TryAttack();
    }

    void TryAttack(){
        if(currentWeapon == null || isAttacking || Time.time < nextAttackTime)
            return; //If the player doesn't have a weapon, is already attacking or attack is on cooldown, don't attack
        currentWeapon.Attack(this);
    }

    // Called by WeaponHitbox when the weapon touches something damageable.
    public void OnWeaponHit(IDamageable target, Collider2D targetCollider)
    {
        if (currentWeapon == null) return;

        Vector2 direction = (Vector2)(targetCollider.transform.position - transform.position);
        direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : (Vector2)weaponPivot.up;

        DamageInfo info = new DamageInfo(currentWeapon.Damage, direction, currentWeapon.Knockback, gameObject);
        if (target.TakeDamage(info))
            HitStop.Freeze(hitStopDuration);
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
        if (weaponHitbox != null)
            weaponHitbox.BeginAttack();
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
            // Move forward (towards the mouse), then return.
            // First half of the duration: forward. Second half: back.
            Vector3 forward = originalRotation * Vector3.up;

            while (elapsed < weapon.AttackDuration)
            {
                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(
                    elapsed / weapon.AttackDuration);

                float distance;

                if (progress <= 0.5f)
                    distance = Mathf.Lerp(0f, weapon.StabDistance, progress * 2f);
                else
                    distance = Mathf.Lerp(
                        weapon.StabDistance, 0f, (progress - 0.5f) * 2f);

                weaponPivot.localPosition =
                    originalPosition + forward * distance;

                yield return null;
            }
        }

        weaponPivot.localPosition = originalPosition;
        weaponPivot.localRotation = originalRotation;

        hitbox.enabled = false;
        isAttacking = false;
    }

    private void OnDisable()
    {
        // If the player dies mid-attack, leave the weapon in a clean state.
        StopAllCoroutines();
        isAttacking = false;
        if (hitbox != null) hitbox.enabled = false;
    }
}
