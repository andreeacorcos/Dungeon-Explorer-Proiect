/*
"Fata" unui inamic: leaga componentele (Component pattern, curs C7) si stie sa se
reseteze cand e refolosit din pool. Comportamentul e in EnemyHealth si EnemyBrain.

Observer: la moarte anunta Enemy.Died. Drop-urile, valurile, scorul etc. asculta
evenimentul, iar inamicul nu stie cine asculta.
*/
using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(EnemyHealth), typeof(EnemyBrain))]
public class Enemy : MonoBehaviour
{
    public static event Action<Enemy> Died;

    public EnemyData Data { get; private set; }
    public EnemyHealth Health { get; private set; }
    public EnemyBrain Brain { get; private set; }

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Action<Enemy> releaseToPool;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() => Died = null;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Health = GetComponent<EnemyHealth>();
        Brain = GetComponent<EnemyBrain>();
    }

    // Contractul pool-ului: tot ce se schimba in viata inamicului revine la starea initiala aici.
    public void Init(EnemyData data, float healthMultiplier, Vector2 position, Action<Enemy> release)
    {
        Data = data;
        releaseToPool = release;

        transform.position = position;
        rb.position = position;
        rb.linearVelocity = Vector2.zero;
        transform.localScale = Vector3.one * data.Scale;

        if (spriteRenderer != null && data.Sprite != null)
            spriteRenderer.sprite = data.Sprite;

        Health.ResetState(data.MaxHealth * healthMultiplier, data.Tint);
        Brain.ResetState(data);
    }

    // Apelat de EnemyHealth cand viata ajunge la 0.
    public void HandleDeath()
    {
        Died?.Invoke(this); // ascultatorii citesc pozitia inainte ca inamicul sa dispara

        if (releaseToPool != null)
            releaseToPool(this);
        else
            Destroy(gameObject); // inamic pus manual in scena, nu din fabrica
    }
}
