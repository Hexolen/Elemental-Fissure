using System.Collections;
using UnityEngine;

/// <summary>
/// Su: oyuncudan her yöne eşit açılarla saçılan, çarptıkça seken küreler.
/// </summary>
public class WaterSpell : Spell
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float rotationSpeed = -360f;

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;

    public override IEnumerator CastLoop(SlotWeapon weapon)
    {
        while (true)
        {
            SpellStatValues s = weapon.Stats;
            int count = s.projectileAmount;

            if (count > 0)
            {
                float angleStep = 360f / count;

                for (int i = 0; i < count; i++)
                {
                    Quaternion rotation = Quaternion.Euler(0f, 0f, angleStep * i);
                    WaterSpell orb = SpawnCopy<WaterSpell>(weapon, weapon.Owner.position, rotation);
                    if (orb != null)
                        orb.Launch(rotation * Vector2.up);
                }
            }

            yield return new WaitForSeconds(s.cooldown);
        }
    }

    private void Launch(Vector2 direction)
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.linearVelocity = direction * stats.projectileSpeed;

        Destroy(gameObject, stats.duration);
    }

    private void Update()
    {
        if (sprite != null)
            sprite.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null) return;

        DamageEnemy(collision.collider, false);

        // Çarpınca hafif rastgele sapma
        float randomAngle = Random.Range(-10f, 10f);
        rb.linearVelocity = Quaternion.Euler(0f, 0f, randomAngle) * rb.linearVelocity;
    }
}
