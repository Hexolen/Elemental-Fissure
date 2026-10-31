using System;
using UnityEngine;

/// <summary>
/// Bir slota ait stat bonusları. Şimdilik slottaki PURE elementlerden gelir.
/// Float değerler yüzde olarak çalışır (0.2 = %20), int değerler düz eklenir.
/// </summary>
[Serializable]
public class BonusStats
{
    [Tooltip("Hasar artışı. 0.2 = %20 fazla hasar")]
    public float bonusDamage;
    [Tooltip("Bekleme süresi azalması. 0.1 = %10 daha kısa cooldown")]
    public float bonusCooldown;
    [Tooltip("Mermi hızı artışı. 0.2 = %20 daha hızlı")]
    public float bonusProjectileSpeed;
    [Tooltip("Süre artışı. 0.2 = %20 daha uzun")]
    public float bonusDuration;
    [Tooltip("Alan artışı. 0.2 = %20 daha büyük")]
    public float bonusArea;
    [Tooltip("Ek mermi sayısı")]
    public int bonusProjectileAmount;
    [Tooltip("Ek geri itme")]
    public int bonusKnockback;
    [Tooltip("Ek sekme sayısı")]
    public int bonusBounces;

    public void Add(BonusStats other)
    {
        if (other == null) return;

        bonusDamage += other.bonusDamage;
        bonusCooldown += other.bonusCooldown;
        bonusProjectileSpeed += other.bonusProjectileSpeed;
        bonusDuration += other.bonusDuration;
        bonusArea += other.bonusArea;
        bonusProjectileAmount += other.bonusProjectileAmount;
        bonusKnockback += other.bonusKnockback;
        bonusBounces += other.bonusBounces;
    }

    public void ResetBonuses()
    {
        bonusDamage = 0;
        bonusCooldown = 0;
        bonusProjectileSpeed = 0;
        bonusDuration = 0;
        bonusArea = 0;
        bonusProjectileAmount = 0;
        bonusKnockback = 0;
        bonusBounces = 0;
    }
}
