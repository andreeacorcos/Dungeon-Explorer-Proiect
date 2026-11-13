/*
Viata jucatorului (minimul necesar): puncte de viata, invulnerabilitate scurta dupa
o lovitura (jucatorul clipeste), knockback si restart la moarte.
Health bar-ul vizual se poate abona la HealthChanged.
*/
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 5;
    [SerializeField] private float invulnerabilityTime = 1f;
    [SerializeField] private float blinkInterval = 0.1f;
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float restartDelay = 1.5f;

    // Jucatorul curent, ca inamicii si pickup-urile sa nu-l caute prin scena in fiecare cadru.
    public static PlayerHealth Current { get; private set; }
    public static event Action<PlayerHealth> Died;
    public event Action<int, int> HealthChanged; // (viata curenta, viata maxima)

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public bool IsDead { get; private set; }
    public Collider2D Body { get; private set; }

    private SpriteRenderer spriteRenderer;
    private PlayerScript movement;
    private WeaponController weapon;
    private float invulnerableUntil;
    private Coroutine blinkRoutine;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        Current = null;
        Died = null;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        movement = GetComponent<PlayerScript>();
        weapon = GetComponent<WeaponController>();
        Body = GetComponent<Collider2D>();
        CurrentHealth = maxHealth;
    }

    private void OnEnable() => Current = this;

    private void OnDisable()
    {
        if (Current == this) Current = null;
    }

    public bool TakeDamage(DamageInfo info)
    {
        if (IsDead || Time.time < invulnerableUntil) return false;

        int amount = Mathf.Max(1, Mathf.RoundToInt(info.Amount));
        CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
        HealthChanged?.Invoke(CurrentHealth, maxHealth);

        if (CurrentHealth == 0)
        {
            Die();
            return true;
        }

        invulnerableUntil = Time.time + invulnerabilityTime;
        if (movement != null)
            movement.ApplyKnockback(info.Direction * info.Knockback, knockbackDuration);

        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = StartCoroutine(Blink());
        return true;
    }

    private IEnumerator Blink()
    {
        while (Time.time < invulnerableUntil)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
        if (spriteRenderer != null) spriteRenderer.enabled = true;
        blinkRoutine = null;
    }

    private void Die()
    {
        IsDead = true;
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
            spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        }
        if (movement != null) movement.enabled = false;
        if (weapon != null) weapon.enabled = false;

        Debug.Log("Player-ul a murit. Restart nivel...");
        Died?.Invoke(this);
        StartCoroutine(RestartAfterDelay());
    }

    private IEnumerator RestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(restartDelay);

        Scene scene = SceneManager.GetActiveScene();
        if (scene.buildIndex >= 0)
        {
            SceneManager.LoadScene(scene.buildIndex);
        }
        else
        {
            // Scena nu e in File > Build Profiles > Scene List, deci nu o putem reincarca.
            Debug.LogWarning($"Scena '{scene.name}' nu e in Build Profiles, nu o pot reincarca.");
        }
    }
}
