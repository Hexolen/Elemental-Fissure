using System;
using UnityEngine;

public class SpellStats : MonoBehaviour
{
    public event Action OnStatsChanged;

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

#if UNITY_EDITOR
    private void OnValidate()
    {
        OnStatsChanged?.Invoke();
    }
#endif
}