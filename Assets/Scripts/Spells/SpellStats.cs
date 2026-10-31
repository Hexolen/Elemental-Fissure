using UnityEngine;

/// <summary>
/// Büyü prefabının temel statları. Slot bonusları SpellStatValues içinde bunun üstüne eklenir.
/// </summary>
public class SpellStats : MonoBehaviour
{
    [Header("Base Stats")]
    [SerializeField] private float baseDamage;
    [SerializeField] private float baseCooldown;
    [SerializeField] private float baseProjectileSpeed;
    [SerializeField] private float baseDuration;
    [SerializeField] private float baseArea;
    [SerializeField] private int baseProjectileAmount;
    [SerializeField] private int baseKnockback;
    [SerializeField] private int baseBounces;

    public float BaseDamage => baseDamage;
    public float BaseCooldown => baseCooldown;
    public float BaseProjectileSpeed => baseProjectileSpeed;
    public float BaseDuration => baseDuration;
    public float BaseArea => baseArea;
    public int BaseProjectileAmount => baseProjectileAmount;
    public int BaseKnockback => baseKnockback;
    public int BaseBounces => baseBounces;
}
