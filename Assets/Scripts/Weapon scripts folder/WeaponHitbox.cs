/*
Se pune pe obiectul care are collider-ul (trigger) al armei.
Detecteaza ce atinge arma in timpul unui atac si anunta WeaponController.
Fiecare tinta poate fi lovita o singura data per atac.
*/
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WeaponHitbox : MonoBehaviour
{
    private WeaponController owner;
    private readonly HashSet<IDamageable> hitThisAttack = new HashSet<IDamageable>();

    public void Init(WeaponController controller) => owner = controller;

    // Apelat la inceputul fiecarui atac.
    public void BeginAttack() => hitThisAttack.Clear();

    private void OnTriggerEnter2D(Collider2D other) => TryHit(other);

    private void TryHit(Collider2D other)
    {
        if (owner == null || !owner.IsAttacking) return;
        if (other.transform.IsChildOf(owner.transform.root)) return; // nu ne lovim singuri

        IDamageable target = other.GetComponentInParent<IDamageable>();
        if (target == null || !hitThisAttack.Add(target)) return;

        owner.OnWeaponHit(target, other);
    }
}
