/*
Contract comun pentru tot ce poate primi damage (player, inamici).
Arma si inamicii nu trebuie sa stie cu cine vorbesc, doar ca obiectul e IDamageable.
*/
using UnityEngine;

public struct DamageInfo
{
    public float Amount;       // cat damage
    public Vector2 Direction;  // directia loviturii (normalizata), folosita la knockback
    public float Knockback;    // cat de tare e impins cel lovit
    public GameObject Source;  // cine a lovit

    public DamageInfo(float amount, Vector2 direction, float knockback, GameObject source)
    {
        Amount = amount;
        Direction = direction;
        Knockback = knockback;
        Source = source;
    }
}

public interface IDamageable
{
    // Returneaza true daca lovitura a avut efect (ex: false daca tinta e invulnerabila sau moarta).
    bool TakeDamage(DamageInfo info);
}
