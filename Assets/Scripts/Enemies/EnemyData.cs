/*
Type Object / Flyweight (curs C7): tipul unui inamic e un asset de date, nu o subclasa.
Un inamic nou = un asset nou (Create > Enemies > Enemy Data), zero cod.
Toti zombii de acelasi tip impart acest asset, deci NU se scrie in el in timpul jocului:
starea fiecarui inamic (viata curenta etc.) sta in componentele lui.
*/
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy", menuName = "Enemies/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [Header("Visuals")]
    [SerializeField] private Sprite sprite;
    [SerializeField] private Color tint = Color.white;
    [SerializeField] private float scale = 1f;

    [Header("Stats")]
    [SerializeField] private float maxHealth = 20f;   // sabia face 10 => 2 lovituri
    [SerializeField] private float moveSpeed = 1.6f;
    [Range(0f, 1f)]
    [SerializeField] private float knockbackResistance = 0f; // 1 = nu poate fi impins deloc

    [Header("Melee attack")]
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackRange = 0.25f;   // distanta intre collidere la care incepe atacul
    [SerializeField] private float windupTime = 0.45f;    // pregatirea atacului (avertizeaza jucatorul)
    [SerializeField] private float attackCooldown = 1f;   // pauza dupa atac
    [SerializeField] private float attackKnockback = 6f;

    public Sprite Sprite => sprite;
    public Color Tint => tint;
    public float Scale => scale;
    public float MaxHealth => maxHealth;
    public float MoveSpeed => moveSpeed;
    public float KnockbackResistance => knockbackResistance;
    public int AttackDamage => attackDamage;
    public float AttackRange => attackRange;
    public float WindupTime => windupTime;
    public float AttackCooldown => attackCooldown;
    public float AttackKnockback => attackKnockback;
}
