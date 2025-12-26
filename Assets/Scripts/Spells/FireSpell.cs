using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class FireSpell : MonoBehaviour
{
    [SerializeField] private SpellStats spellStats;

    [Header("Physics")]
    [SerializeField] private Rigidbody2D rb;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float targetingRadius = 4f;

    [Header("Sprite orientation")]
    [Tooltip("Derece cinsinden. Prefab sprite'ının 0° referansına göre ek döndürme.")]
    [SerializeField] private float rotationOffset = 90f;

    private Transform playerTransform;

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
        Debug.Log("🔥 FireSpell: Stats changed, cooldown updated → " + spellStats.BaseCooldown);

        PlayerController context = FindAnyObjectByType<PlayerController>();
        if (context == null) return;

        if (spawnCoroutine != null)
            context.StopCoroutine(spawnCoroutine);

        spawnCoroutine = context.StartCoroutine(
            SpawnLoop(context.transform, spellStats.gameObject)
        );
    }

    /// <summary>
    /// PlayerController bunu çağırır
    /// </summary>
    public static void Spawn(Transform player, GameObject spellPrefab, MonoBehaviour context)
    {
        if (spawnCoroutine != null)
            context.StopCoroutine(spawnCoroutine);

        spawnCoroutine = context.StartCoroutine(
            SpawnLoop(player, spellPrefab)
        );
    }

    private static IEnumerator SpawnLoop(Transform player, GameObject spellPrefab)
    {
        SpellStats stats = spellPrefab.GetComponent<SpellStats>();

        while (true)
        {
            float cooldown = stats != null ? stats.BaseCooldown : 1f;

            GameObject instance = Instantiate(spellPrefab, player.position, Quaternion.identity);
            FireSpell fireSpell = instance.GetComponent<FireSpell>();
            if (fireSpell != null)
                fireSpell.Initialize(player);

            yield return new WaitForSeconds(cooldown);
        }
    }

    /// <summary>
    /// Instance üzerinde çağrılır, fireball hareketi başlatır
    /// </summary>
    public void Initialize(Transform player)
    {
        playerTransform = player;
        StartCoroutine(FireRoutine());
    }

    private IEnumerator FireRoutine()
    {
        float speed = spellStats != null ? spellStats.BaseProjectileSpeed : 5f;
        float duration = spellStats != null ? spellStats.BaseDuration : 3f;
        int amount = spellStats != null ? spellStats.BaseProjectileAmount : 1;

        // total cone açısını baseArea'dan hesapla (örnek formül)
        float totalConeAngle = (amount > 1 && spellStats != null) ? spellStats.BaseArea * 4f : 0f;
        Vector3 fireballScale = Vector3.one * (spellStats != null ? spellStats.BaseArea / 5f : 1f);

        // Hedef veya düz yukarı
        Vector3 forward;
        Transform target = FindNearestEnemy(playerTransform.position);
        if (target != null)
            forward = (target.position - transform.position).normalized;
        else
            forward = Vector3.up; // kesin yukarı

        float startAngle = -totalConeAngle / 2f;
        float angleStep = amount > 1 ? totalConeAngle / (amount - 1) : 0f;

        for (int i = 0; i < amount; i++)
        {
            float angleOffset = startAngle + angleStep * i;

            // dir: forward yönünü, Z ekseni etrafında angleOffset kadar döndür
            Vector3 dir = Quaternion.AngleAxis(angleOffset, Vector3.forward) * forward;

            // Prefab'ın kendisini klonla (her klon bir projectile olacak)
            GameObject fireball = Instantiate(gameObject, transform.position, Quaternion.identity);

            // scale uygula
            fireball.transform.localScale = fireballScale;

            // rotasyonu ayarla: dir'e göre açı + sprite'ın doğal yön offset'i
            float angleDeg = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float finalRot = angleDeg + rotationOffset;
            fireball.transform.rotation = Quaternion.Euler(0f, 0f, finalRot);

            // Rigidbody'yi al ve hıza uygula
            Rigidbody2D rbFire = fireball.GetComponent<Rigidbody2D>();
            if (rbFire != null)
            {
                // Unity 6.2 -> linearVelocity
                rbFire.linearVelocity = dir * speed;
            }

            // Her klon kendi süresi sonunda yok olsun
            Destroy(fireball, duration);
        }

        // Orijinal (caster) objeyi yok et
        Destroy(gameObject);
        yield return null;
    }

    private Transform FindNearestEnemy(Vector3 position)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(position, targetingRadius, enemyLayer);
        if (hits.Length == 0) return null;

        Transform nearest = hits[0].transform;
        float minDistSqr = (nearest.position - position).sqrMagnitude;

        for (int i = 1; i < hits.Length; i++)
        {
            float distSqr = (hits[i].transform.position - position).sqrMagnitude;
            if (distSqr < minDistSqr)
            {
                nearest = hits[i].transform;
                minDistSqr = distSqr;
            }
        }

        return nearest;
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
}