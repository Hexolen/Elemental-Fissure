using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Tüm büyülerin ortak tabanı. Prefab üzerindeki bileşen iki iş görür:
///  - CastLoop: SlotWeapon tarafından prefab referansı üzerinden çalıştırılır ve büyü kopyalarını spawnlar.
///  - Spawnlanan kopyalar: hareket / hasar davranışı, spawn anındaki son statları (stats) kullanır.
/// Her slotun kendi SlotWeapon'ı olduğu için aynı büyü birden fazla slotta aynı anda çalışabilir.
/// </summary>
public abstract class Spell : MonoBehaviour
{
    [Header("Spell")]
    [SerializeField] protected SpellStats spellStats;
    [SerializeField] protected LayerMask enemyLayer;

    /// <summary>Bu kopyanın spawnlandığı andaki son statları (temel + slot bonusu).</summary>
    protected SpellStatValues stats;

    /// <summary>Büyüyü atan oyuncu. Slot silahı yok olsa bile kopya bunu kullanmaya devam eder.</summary>
    protected Transform owner;

    /// <summary>Kopyayı spawnlayan slot (renk, element bilgisi vb. için).</summary>
    protected WeaponSlot sourceSlot;

    public SpellStats BaseStats => spellStats;

    /// <summary>
    /// Büyünün atış döngüsü. Prefab üzerinde çağrılır, coroutine SlotWeapon üzerinde çalışır.
    /// </summary>
    public abstract IEnumerator CastLoop(SlotWeapon weapon);

    /// <summary>
    /// Bu prefabdan bir kopya spawnlar ve slotun güncel statlarını kopyaya verir.
    /// </summary>
    protected T SpawnCopy<T>(SlotWeapon weapon, Vector3 position, Quaternion rotation) where T : Spell
    {
        T copy = Instantiate(this, position, rotation) as T;
        if (copy == null)
        {
            Debug.LogError($"{name}: {typeof(T).Name} olarak spawnlanamadı.");
            return null;
        }

        copy.stats = weapon.Stats;
        copy.owner = weapon.Owner;
        copy.sourceSlot = weapon.Slot;
        return copy;
    }

    protected bool IsEnemy(GameObject target)
    {
        return ((1 << target.layer) & enemyLayer.value) != 0;
    }

    /// <summary>
    /// Düşmansa hasar verir, istenirse büyüden uzağa iter. Hasar verildiyse true döner.
    /// </summary>
    protected bool DamageEnemy(Collider2D target, bool applyKnockback)
    {
        if (!IsEnemy(target.gameObject)) return false;

        Health enemyHealth = target.GetComponent<Health>();
        if (enemyHealth == null) return false;

        enemyHealth.TakeDamage(stats.damage);

        Rigidbody2D enemyRb = target.attachedRigidbody;
        if (applyKnockback && stats.knockback > 0 && enemyRb != null)
        {
            Vector2 direction = (target.transform.position - transform.position).normalized;
            enemyRb.AddForce(direction * stats.knockback, ForceMode2D.Impulse);
        }

        return true;
    }

    /// <summary>
    /// Yarıçap içindeki düşmanları en yakından en uzağa sıralı döndürür (her düşman bir kez).
    /// </summary>
    protected List<Transform> FindEnemiesByDistance(Vector3 center, float radius)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, enemyLayer);
        List<Transform> targets = new List<Transform>();

        foreach (Collider2D hit in hits)
        {
            Transform candidate = hit.attachedRigidbody != null ? hit.attachedRigidbody.transform : hit.transform;
            if (!targets.Contains(candidate))
                targets.Add(candidate);
        }

        targets.Sort((a, b) =>
            (a.position - center).sqrMagnitude.CompareTo((b.position - center).sqrMagnitude));

        return targets;
    }

    protected Transform FindNearestEnemy(Vector3 center, float radius)
    {
        List<Transform> targets = FindEnemiesByDistance(center, radius);
        return targets.Count > 0 ? targets[0] : null;
    }
}
