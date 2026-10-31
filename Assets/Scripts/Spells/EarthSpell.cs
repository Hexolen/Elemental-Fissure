using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Toprak: en yakın düşmanların altında açılan, içindekilere sürekli hasar veren alan.
/// </summary>
public class EarthSpell : Spell
{
    [Header("Earth")]
    [SerializeField] private float damageTickInterval = 0.5f;
    [SerializeField] private float targetingRadius = 8f;

    private readonly HashSet<Health> enemiesInside = new HashSet<Health>();

    public override IEnumerator CastLoop(SlotWeapon weapon)
    {
        while (true)
        {
            SpellStatValues s = weapon.Stats;

            // Hedef yoksa hiçbir şey spawnlanmaz
            List<Transform> targets = FindEnemiesByDistance(weapon.Owner.position, targetingRadius);
            int spawnCount = Mathf.Min(s.projectileAmount, targets.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                EarthSpell area = SpawnCopy<EarthSpell>(weapon, targets[i].position, Quaternion.identity);
                if (area != null)
                    area.Activate();
            }

            yield return new WaitForSeconds(s.cooldown);
        }
    }

    private void Activate()
    {
        // 4 area = scale 1
        transform.localScale = Vector3.one * (stats.area / 4f);
        Destroy(gameObject, stats.duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsEnemy(collision.gameObject)) return;

        Health enemyHealth = collision.GetComponent<Health>();
        if (enemyHealth == null) return;

        if (enemiesInside.Add(enemyHealth))
            StartCoroutine(DamageOverTime(enemyHealth));
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!IsEnemy(collision.gameObject)) return;

        Health enemyHealth = collision.GetComponent<Health>();
        if (enemyHealth != null)
            enemiesInside.Remove(enemyHealth);
    }

    private IEnumerator DamageOverTime(Health enemy)
    {
        while (enemy != null && enemiesInside.Contains(enemy))
        {
            enemy.TakeDamage(stats.damage);
            yield return new WaitForSeconds(damageTickInterval);
        }
    }
}
