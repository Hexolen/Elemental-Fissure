using System.Collections;
using UnityEngine;

/// <summary>
/// Henüz tarifi olmayan element karışımları için geçici büyü.
/// En yakın düşmana, slottaki elementlerin renk karışımıyla mermi atar ve ilk vurduğunda yok olur.
/// </summary>
public class TestSpell : Spell
{
    [Header("Test Spell")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float targetingRadius = 8f;
    [Tooltip("Birden fazla mermi varsa aralarındaki açı")]
    [SerializeField] private float spreadAngle = 15f;

    public override IEnumerator CastLoop(SlotWeapon weapon)
    {
        while (true)
        {
            SpellStatValues s = weapon.Stats;
            Vector3 origin = weapon.Owner.position;

            Transform target = FindNearestEnemy(origin, targetingRadius);
            Vector3 forward = target != null ? (target.position - origin).normalized : (Vector3)Random.insideUnitCircle.normalized;
            if (forward == Vector3.zero) forward = Vector3.up;

            Color color = GetSlotColor(weapon.Slot);

            for (int i = 0; i < s.projectileAmount; i++)
            {
                float offset = (i - (s.projectileAmount - 1) / 2f) * spreadAngle;
                Vector3 dir = Quaternion.AngleAxis(offset, Vector3.forward) * forward;

                TestSpell bullet = SpawnCopy<TestSpell>(weapon, origin, Quaternion.identity);
                if (bullet != null)
                    bullet.Launch(dir, color);
            }

            yield return new WaitForSeconds(s.cooldown);
        }
    }

    private void Launch(Vector2 direction, Color color)
    {
        transform.localScale = Vector3.one * (stats.area / 5f);

        if (sprite != null) sprite.color = color;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * stats.projectileSpeed;

        Destroy(gameObject, stats.duration);
    }

    /// <summary>
    /// Slottaki tüm elementlerin renklerinin ortalaması.
    /// </summary>
    private static Color GetSlotColor(WeaponSlot slot)
    {
        if (slot == null || slot.IsEmpty) return Color.white;

        Color sum = Color.clear;
        foreach (SlotElement e in slot.Elements)
            sum += ElementUtility.GetColor(e.element);

        Color average = sum / slot.Count;
        average.a = 1f;
        return average;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (DamageEnemy(collision, true))
            Destroy(gameObject);
    }
}
