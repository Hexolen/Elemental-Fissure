using System;
using System.Collections;
using UnityEngine;

public class AirSpell : MonoBehaviour
{
    [SerializeField] private SpellStats spellStats;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float tornadoFlipDuration = 0.2f;
    [SerializeField] private float tornadoFlipVariance = 0.1f;

    private Transform target;
    private float orbitRadius;
    private float orbitSpeed;
    private float orbitAngle;

    private static Coroutine spawnCoroutine;

    private void OnEnable()
    {
        if (spellStats != null)
            spellStats.OnStatsChanged += OnStatsChanged;
    }

    private void OnDisable()
    {
        if (spellStats != null)
            spellStats.OnStatsChanged -= OnStatsChanged;
    }

    private void OnStatsChanged()
    {
        Debug.Log("🌪 AirSpell stats changed!");

        PlayerController context = FindAnyObjectByType<PlayerController>();
        if (context == null) return;

        if (spawnCoroutine != null)
            context.StopCoroutine(spawnCoroutine);

        spawnCoroutine = context.StartCoroutine(
            SpawnLoop(context.transform, spellStats.gameObject)
        );
    }


    /// <summary>
    /// PlayerController bunu çağırır.
    /// </summary>
    public static void Spawn(Transform targetTransform, GameObject spellPrefab, MonoBehaviour context)
    {
        if (spawnCoroutine != null)
            context.StopCoroutine(spawnCoroutine);

        spawnCoroutine = context.StartCoroutine(
            SpawnLoop(targetTransform, spellPrefab)
        );
    }

    // spawn/duration/cooldown döngüsü
    private static IEnumerator SpawnLoop(Transform targetTransform, GameObject spellPrefab)
    {
        // prefab’ın SpellStats’ını okuyoruz
        SpellStats prefabStats = spellPrefab.GetComponent<SpellStats>();

        while (true)
        {
            float duration = prefabStats != null ? prefabStats.BaseDuration : 6f;
            float cooldown = prefabStats != null ? prefabStats.BaseCooldown : 4f;
            int totalSpells = prefabStats != null ? prefabStats.BaseProjectileAmount : 1;

            for (int i = 0; i < totalSpells; i++)
            {
                GameObject instance = Instantiate(spellPrefab, targetTransform.position, Quaternion.identity);
                AirSpell airSpell = instance.GetComponent<AirSpell>();

                if (airSpell != null)
                {
                    airSpell.Setup(targetTransform, i, totalSpells);
                    airSpell.StartCoroutine(airSpell.FlipTornadoLoop());
                    airSpell.StartCoroutine(airSpell.SelfDestruct(duration));
                }
            }
            // duration + cooldown bekle (AirSpell kendi kendini yok edecek)
            yield return new WaitForSeconds(duration + cooldown);
        }
    }

    /// <summary>
    /// Instance üzerinde çağrılır. Orbit parametreleri hesaplar.
    /// </summary>
    private void Setup(Transform targetTransform, int index, int totalSpells)
    {
        target = targetTransform;
        orbitRadius = spellStats != null ? spellStats.BaseArea / 7f : 1f;
        float visualScale = spellStats != null ? spellStats.BaseArea / 5f : 1f;
        transform.localScale = Vector3.one * visualScale;

        orbitSpeed = spellStats != null ? spellStats.BaseProjectileSpeed * 50f : 100f;
        orbitAngle = (360f / totalSpells) * index;
    }

    private void Update()
    {
        if (target == null) return;

        orbitAngle += orbitSpeed * Time.deltaTime;
        float rad = orbitAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;

        transform.position = target.position + offset;
    }

    IEnumerator FlipTornadoLoop()
    {
        while (true)
        {
            sprite.flipX = !sprite.flipX;
            float wait = tornadoFlipDuration + UnityEngine.Random.Range(-tornadoFlipVariance, tornadoFlipVariance);
            wait = Mathf.Max(0.01f, wait);

            yield return new WaitForSeconds(wait);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            Health enemyHealth = collision.GetComponent<Health>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(spellStats.BaseDamage);

                Rigidbody2D enemyRb = collision.attachedRigidbody;
                if (enemyRb != null)
                {
                    // Player'dan uzaklaşacak yönde knockback uygula
                    Vector2 direction = (collision.transform.position - transform.position).normalized;
                    float knockbackForce = spellStats.BaseKnockback;

                    enemyRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
                }
            }
        }
    }

    IEnumerator SelfDestruct(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }
}
