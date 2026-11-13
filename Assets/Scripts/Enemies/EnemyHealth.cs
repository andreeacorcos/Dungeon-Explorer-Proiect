/*
Viata unui inamic. Fara health bar (cerinta din raport), doar un flash rosu la lovitura.
*/
using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private Color flashColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private float flashDuration = 0.1f;

    private Enemy enemy;
    private EnemyBrain brain;
    private SpriteRenderer spriteRenderer;
    private Color baseColor = Color.white;
    private Coroutine flashRoutine;

    public float CurrentHealth { get; private set; }
    public float MaxHealth { get; private set; }
    public bool IsDead { get; private set; }

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        brain = GetComponent<EnemyBrain>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ResetState(float maxHealth, Color tint)
    {
        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
        IsDead = false;
        baseColor = tint;
        if (spriteRenderer != null) spriteRenderer.color = tint;
    }

    public bool TakeDamage(DamageInfo info)
    {
        if (IsDead) return false;

        CurrentHealth -= info.Amount;
        brain.ApplyKnockback(info.Direction * info.Knockback);
        Flash();

        if (CurrentHealth <= 0.001f)
        {
            IsDead = true;
            enemy.HandleDeath();
        }
        return true;
    }

    private void Flash()
    {
        if (spriteRenderer == null || !isActiveAndEnabled) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = baseColor;
        flashRoutine = null;
    }

    private void OnDisable()
    {
        // Revenit in pool in mijlocul unui flash: resetam culoarea.
        flashRoutine = null;
        if (spriteRenderer != null) spriteRenderer.color = baseColor;
    }
}
