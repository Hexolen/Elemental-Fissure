using System.Collections;
using UnityEngine;

/// <summary>
/// Ateş: en yakın düşmana doğru koni şeklinde ateş topları.
/// </summary>
public class FireSpell : Spell
{
    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Targeting")]
    [SerializeField] private float targetingRadius = 4f;

    [Header("Sprite orientation")]
    [Tooltip("Derece cinsinden. Prefab sprite'ının 0° referansına göre ek döndürme.")]
    [SerializeField] private float rotationOffset = 90f;

    public override IEnumerator CastLoop(SlotWeapon weapon)
    {
        while (true)
        {
            SpellStatValues s = weapon.Stats;
            FireVolley(weapon, s);
            yield return new WaitForSeconds(s.cooldown);
        }
    }

    private void FireVolley(SlotWeapon weapon, SpellStatValues s)
    {
        Vector3 origin = weapon.Owner.position;

        // Hedef varsa ona, yoksa düz yukarı
        Transform target = FindNearestEnemy(origin, targetingRadius);
        Vector3 forward = target != null ? (target.position - origin).normalized : Vector3.up;

        int amount = s.projectileAmount;
        float totalConeAngle = amount > 1 ? s.area * 4f : 0f;
        float startAngle = -totalConeAngle / 2f;
        float angleStep = amount > 1 ? totalConeAngle / (amount - 1) : 0f;

        for (int i = 0; i < amount; i++)
        {
            Vector3 dir = Quaternion.AngleAxis(startAngle + angleStep * i, Vector3.forward) * forward;
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            FireSpell fireball = SpawnCopy<FireSpell>(weapon, origin, Quaternion.Euler(0f, 0f, angleDeg + rotationOffset));
            if (fireball != null)
                fireball.Launch(dir);
        }
    }

    private void Launch(Vector2 direction)
    {
        transform.localScale = Vector3.one * (stats.area / 5f);

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * stats.projectileSpeed;

        Destroy(gameObject, stats.duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageEnemy(collision, true);
    }
}
