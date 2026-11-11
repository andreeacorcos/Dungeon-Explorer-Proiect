/*
This script is a subclass of generic weapons
It adds variables specific to melee weapons
*/

using UnityEngine;

public enum MeleeAttackType
{
    Swing,
    Stab
}

[CreateAssetMenu(fileName = "New Melee Weapon",
    menuName = "Weapons/Melee Weapon")]
public class MeleeWeaponData : WeaponData
{
    [Header("Melee attack")]
    [SerializeField] private MeleeAttackType attackType;

    [SerializeField] private float attackDuration = 0.25f;

    [Header("Swing settings")]
    [SerializeField] private float swingAngle = 120f;

    [Header("Stab settings")]
    [SerializeField] private float stabDistance = 1f;

    public MeleeAttackType AttackType => attackType;
    public float AttackDuration => attackDuration;
    public float SwingAngle => swingAngle;
    public float StabDistance => stabDistance;

    public override void Attack(WeaponController controller)
    {
        controller.PerformMeleeAttack(this);
    }
}