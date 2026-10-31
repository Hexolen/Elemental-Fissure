using UnityEngine;

/// <summary>
/// Bir büyünün son değerleri: prefabdaki temel statlar (SpellStats) + slot bonusu (BonusStats).
/// </summary>
public struct SpellStatValues
{
    private const float MinCooldown = 0.05f;

    public float damage;
    public float cooldown;
    public float projectileSpeed;
    public float duration;
    public float area;
    public int projectileAmount;
    public int knockback;
    public int bounces;

    public static SpellStatValues Default => new SpellStatValues
    {
        damage = 1f,
        cooldown = 1f,
        projectileSpeed = 5f,
        duration = 2f,
        area = 5f,
        projectileAmount = 1
    };

    public static SpellStatValues Combine(SpellStats baseStats, BonusStats bonus)
    {
        if (baseStats == null) return Default;
        if (bonus == null) bonus = new BonusStats();

        return new SpellStatValues
        {
            damage = baseStats.BaseDamage * Percent(bonus.bonusDamage),
            cooldown = Mathf.Max(MinCooldown, baseStats.BaseCooldown * (1f - bonus.bonusCooldown)),
            projectileSpeed = baseStats.BaseProjectileSpeed * Percent(bonus.bonusProjectileSpeed),
            duration = baseStats.BaseDuration * Percent(bonus.bonusDuration),
            area = baseStats.BaseArea * Percent(bonus.bonusArea),
            projectileAmount = Mathf.Max(0, baseStats.BaseProjectileAmount + bonus.bonusProjectileAmount),
            knockback = Mathf.Max(0, baseStats.BaseKnockback + bonus.bonusKnockback),
            bounces = Mathf.Max(0, baseStats.BaseBounces + bonus.bonusBounces)
        };
    }

    private static float Percent(float bonus)
    {
        return Mathf.Max(0f, 1f + bonus);
    }
}
