using System.Collections;
using UnityEngine;

/// <summary>
/// Hava: oyuncunun etrafında dönen hortumlar.
/// </summary>
public class AirSpell : Spell
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float tornadoFlipDuration = 0.2f;
    [SerializeField] private float tornadoFlipVariance = 0.1f;

    private float orbitRadius;
    private float orbitSpeed;
    private float orbitAngle;

    public override IEnumerator CastLoop(SlotWeapon weapon)
    {
        while (true)
        {
            SpellStatValues s = weapon.Stats;

            for (int i = 0; i < s.projectileAmount; i++)
            {
                AirSpell tornado = SpawnCopy<AirSpell>(weapon, weapon.Owner.position, Quaternion.identity);
                if (tornado != null)
                    tornado.StartOrbit(i, s.projectileAmount);
            }

            // Hortumlar süreleri bitince kendini yok eder, ardından cooldown beklenir
            yield return new WaitForSeconds(s.duration + s.cooldown);
        }
    }

    private void StartOrbit(int index, int total)
    {
        orbitRadius = stats.area / 7f;
        transform.localScale = Vector3.one * (stats.area / 5f);
        orbitSpeed = stats.projectileSpeed * 50f;
        orbitAngle = (360f / total) * index;

        UpdateOrbitPosition();
        StartCoroutine(FlipTornadoLoop());
        Destroy(gameObject, stats.duration);
    }

    private void Update()
    {
        if (owner == null) return;

        orbitAngle += orbitSpeed * Time.deltaTime;
        UpdateOrbitPosition();
    }

    private void UpdateOrbitPosition()
    {
        if (owner == null) return;

        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        transform.position = owner.position + offset;
    }

    private IEnumerator FlipTornadoLoop()
    {
        while (sprite != null)
        {
            sprite.flipX = !sprite.flipX;
            float wait = tornadoFlipDuration + Random.Range(-tornadoFlipVariance, tornadoFlipVariance);
            yield return new WaitForSeconds(Mathf.Max(0.01f, wait));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        DamageEnemy(collision, true);
    }
}
