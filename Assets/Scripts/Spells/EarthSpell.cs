using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EarthSpell : MonoBehaviour
{
    [Header("Spell Stats")]
    [SerializeField] private SpellStats spellStats;
    [SerializeField] private float damageTickInterval = 0.5f;

    [Header("Enemy Detection")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float targetingRadius = 8f;

    private readonly HashSet<Health> enemiesInside = new();

    private static Coroutine spawnCoroutine;

    private void OnEnable()
    {
        spellStats.OnStatsChanged += OnStatsChanged;
    }

    private void OnDisable()
    {
        spellStats.OnStatsChanged -= OnStatsChanged;
    }

    private void OnStatsChanged()
    {
        Debug.Log("🪨 EarthSpell → Stats Changed!");

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
        

        // prefab içindeki EarthSpell scriptine erişiyoruz (targetingRadius, enemyLayer burada)
        EarthSpell prefabScript = spellPrefab.GetComponent<EarthSpell>();

        while (true)
        {
            float cooldown = stats != null ? stats.BaseCooldown : 1f;
            int projectileAmount = stats != null ? stats.BaseProjectileAmount : 1;

            // 1) Overlap ile hitleri al
            List<Transform> uniqueTargets = new List<Transform>();
            if (prefabScript != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(player.position, prefabScript.targetingRadius, prefabScript.enemyLayer);

                // 2) Her hit için "ana" transformu al (attachedRigidbody varsa onun transform'u, yoksa collider.transform)
                foreach (var h in hits)
                {
                    Transform candidate = h.attachedRigidbody != null ? h.attachedRigidbody.transform : h.transform;

                    // Tekilleştir (aynı transform zaten ekliyse atla)
                    if (!uniqueTargets.Contains(candidate))
                        uniqueTargets.Add(candidate);
                }

                // 3) Eğer hiç hedef yoksa hiçbir şey spawnlama (istediğin davranış bu)
                if (uniqueTargets.Count > 0)
                {
                    // 4) Player konumuna göre sırala — en yakın ilk
                    uniqueTargets.Sort((a, b) =>
                    {
                        float da = (a.position - player.position).sqrMagnitude;
                        float db = (b.position - player.position).sqrMagnitude;
                        return da.CompareTo(db);
                    });

                    // 5) Kaç tane spawnlayabileceğimizi hesapla
                    int spawnCount = Mathf.Min(projectileAmount, uniqueTargets.Count);

                    // 6) İlk spawnCount hedefin altına instantiate et
                    for (int i = 0; i < spawnCount; i++)
                    {
                        Transform target = uniqueTargets[i];
                        Vector3 spawnPos = target.position;

                        GameObject instance = Instantiate(spellPrefab, spawnPos, Quaternion.identity);
                        EarthSpell earthSpell = instance.GetComponent<EarthSpell>();
                        if (earthSpell != null)
                            earthSpell.Initialize(player);
                    }
                }
            }
            yield return new WaitForSeconds(cooldown);
        }
    }

    public void Initialize(Transform player)
    {
        if (spellStats != null)
        {
            float area = spellStats.BaseArea;
            // pick a scaling rule you like; here 4 area = scale 1
            float scaleFactor = area / 4f;
            transform.localScale = Vector3.one * scaleFactor;

            // lifetime
            float duration = spellStats.BaseDuration;
            Destroy(gameObject, duration);
        }
        else
        {
            // default size/duration if no stats attached
            transform.localScale = Vector3.one;
            Destroy(gameObject, 1.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0) return;

        Health enemyHealth = collision.GetComponent<Health>();
        if (!enemyHealth) return;

        if (enemiesInside.Add(enemyHealth))
        {
            StartCoroutine(DamageOverTime(enemyHealth));
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & enemyLayer) == 0) return;

        Health enemyHealth = collision.GetComponent<Health>();
        if (!enemyHealth) return;

        enemiesInside.Remove(enemyHealth);
    }
    private IEnumerator DamageOverTime(Health enemy)
    {
        while (enemiesInside.Contains(enemy))
        {
            enemy.TakeDamage(spellStats.BaseDamage);
            yield return new WaitForSeconds(damageTickInterval);
        }
    }
}
