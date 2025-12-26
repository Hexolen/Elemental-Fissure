using UnityEngine;
using System.Collections;

public class WaterSpell : MonoBehaviour
{
    [Header("Spell Stats")]
    [SerializeField] private SpellStats spellStats;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private float rotationSpeed = -360f;

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;

    // --- static spawn kontrolü ---
    private static MonoBehaviour spawnContext;   // coroutine'i başlatan MonoBehaviour (genelde PlayerController)
    private static Transform spawnTarget;        // target transform (player)
    private static GameObject spawnPrefab;       // prefab referansı
    private static Coroutine spawnCoroutine;

    private void OnEnable()
    {
        if (spellStats == null)
        {
            Debug.LogError($"{name}: spellStats null! Prefab üzerindeki SpellStats'ı ata.");
            return;
        }

        // instance-level event subscribe (varsa)
        spellStats.OnStatsChanged += OnStatChanged;
    }

    private void OnDisable()
    {
        if (spellStats != null)
            spellStats.OnStatsChanged -= OnStatChanged;
    }

    private void Update()
    {
        AnimateWaterSpell();
    }

    void AnimateWaterSpell()
    {
        if (sprite != null)
            sprite.transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
    }

    /// <summary>
    /// PlayerController gibi dışarıdan çağrılır, spelli başlatır
    /// context -> coroutine başlatmak için kullanılan MonoBehaviour (Stop/Start için saklanır)
    /// targetTransform -> spawn'ların konumu (genelde player.transform)
    /// spellPrefab -> prefab referansı (içinde SpellStats component var)
    /// </summary>
    public static void Spawn(Transform targetTransform, GameObject spellPrefab, MonoBehaviour context)
    {
        // Eğer daha önce çalışıyorsa eski coroutine'i aynı context üzerinden durdur
        if (spawnCoroutine != null && spawnContext != null)
        {
            spawnContext.StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        // Sakla: context/target/prefab
        spawnContext = context;
        spawnTarget = targetTransform;
        spawnPrefab = spellPrefab;

        // Başlat
        spawnCoroutine = spawnContext.StartCoroutine(SpawnLoop(spawnTarget, spawnPrefab));
    }

    // SpawnLoop'un imzası: (Transform, GameObject) -> böylece StartCoroutine çağrıları aynı parametreleri kullanır
    private static IEnumerator SpawnLoop(Transform targetTransform, GameObject spellPrefab)
    {
        SpellStats stats = spellPrefab.GetComponent<SpellStats>();
        float cooldown = stats != null ? stats.BaseCooldown : 1f;

        while (true)
        {
            int count = stats.BaseProjectileAmount;
            float angleStep = 360f / count;
            float startAngle = 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle + angleStep * i;

                // Spawn
                GameObject instance = Instantiate(spellPrefab, targetTransform.position, Quaternion.Euler(0, 0, angle));

                WaterSpell waterSpell = instance.GetComponent<WaterSpell>();

                if (waterSpell != null && stats != null)
                {
                    Vector2 direction = Quaternion.Euler(0, 0, angle) * Vector2.up;
                    waterSpell.rb.linearVelocity = direction * stats.BaseProjectileSpeed;

                    waterSpell.StartCoroutine(waterSpell.SelfDestruct(stats.BaseDuration));
                }
            }

            yield return new WaitForSeconds(cooldown);
        }
    }

    private IEnumerator SelfDestruct(float duration)
    {
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (rb == null) return;

        if (((1 << collision.gameObject.layer) & enemyLayer) != 0)
        {
            Health enemyHealth = collision.gameObject.GetComponent<Health>();
            if (enemyHealth != null && spellStats != null)
            {
                enemyHealth.TakeDamage(spellStats.BaseDamage);
            }
        }

        Vector2 vel = rb.linearVelocity;
        float randomAngle = UnityEngine.Random.Range(-10f, 10f);
        vel = Quaternion.Euler(0, 0, randomAngle) * vel;
        rb.linearVelocity = vel;
    }

    // Event tetiklendiğinde spawn loop'u yeniden başlat (aynı context/target/prefab kullanılarak)
    private void OnStatChanged()
    {
        if (spawnCoroutine != null && spawnContext != null)
        {
            // güvenli durdur/başlat, aynı context üzerinde
            spawnContext.StopCoroutine(spawnCoroutine);
            spawnCoroutine = spawnContext.StartCoroutine(SpawnLoop(spawnTarget, spawnPrefab));
        }
    }
}
