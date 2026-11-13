/*
Comportamentul unui zombie, ca un mic automat de stari (State, curs C7):
  Chase   -> merge spre jucator
  Windup  -> se opreste si "se umfla" (avertizeaza ca ataca)
  Recover -> dupa atac, pauza scurta
Ataca doar de aproape, fara arme (cerinta din raport).
*/
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBrain : MonoBehaviour
{
    private enum State { Chase, Windup, Recover }

    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float windupScaleBoost = 0.15f; // cat creste inamicul in timpul pregatirii

    private EnemyData data;
    private Rigidbody2D rb;
    private Collider2D body;
    private SpriteRenderer spriteRenderer;
    private State state;
    private float stateEndTime;
    private float knockbackUntil;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        body = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void ResetState(EnemyData enemyData)
    {
        data = enemyData;
        state = State.Chase;
        stateEndTime = 0f;
        knockbackUntil = 0f;
        ApplyWindupScale(0f);
    }

    public void ApplyKnockback(Vector2 force)
    {
        if (data == null) return;
        Vector2 velocity = force * (1f - data.KnockbackResistance);
        if (velocity.sqrMagnitude < 0.01f) return;

        rb.linearVelocity = velocity;
        knockbackUntil = Time.time + knockbackDuration;
    }

    private void FixedUpdate()
    {
        if (data == null) return;
        if (Time.time < knockbackUntil) return; // fizica il impinge, nu-l controlam

        PlayerHealth player = PlayerHealth.Current;
        if (player == null || player.IsDead || player.Body == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 toPlayer = (Vector2)player.transform.position - rb.position;
        Vector2 direction = toPlayer.sqrMagnitude > 0.0001f ? toPlayer.normalized : Vector2.zero;
        // Distanta dintre marginile colliderelor (nu dintre centre), ca sa conteze si marimea.
        float gap = body.Distance(player.Body).distance;

        if (spriteRenderer != null && Mathf.Abs(direction.x) > 0.01f)
            spriteRenderer.flipX = direction.x < 0f;

        switch (state)
        {
            case State.Chase:
                if (gap <= data.AttackRange)
                {
                    state = State.Windup;
                    stateEndTime = Time.time + data.WindupTime;
                    rb.linearVelocity = Vector2.zero;
                }
                else
                {
                    rb.linearVelocity = direction * data.MoveSpeed;
                }
                break;

            case State.Windup:
                rb.linearVelocity = Vector2.zero;
                float progress = data.WindupTime > 0f
                    ? 1f - Mathf.Clamp01((stateEndTime - Time.time) / data.WindupTime)
                    : 1f;
                ApplyWindupScale(progress);

                if (Time.time >= stateEndTime)
                {
                    // Lovim doar daca jucatorul nu s-a indepartat intre timp.
                    if (gap <= data.AttackRange * 1.5f + 0.1f)
                    {
                        player.TakeDamage(new DamageInfo(
                            data.AttackDamage, direction, data.AttackKnockback, gameObject));
                    }
                    ApplyWindupScale(0f);
                    state = State.Recover;
                    stateEndTime = Time.time + data.AttackCooldown;
                }
                break;

            case State.Recover:
                rb.linearVelocity = gap > data.AttackRange ? direction * data.MoveSpeed * 0.5f : Vector2.zero;
                if (Time.time >= stateEndTime)
                    state = State.Chase;
                break;
        }
    }

    private void ApplyWindupScale(float progress)
    {
        float baseScale = data != null ? data.Scale : 1f;
        transform.localScale = Vector3.one * baseScale * (1f + windupScaleBoost * progress);
    }
}
