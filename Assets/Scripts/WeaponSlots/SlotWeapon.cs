using UnityEngine;

/// <summary>
/// Bir slotun aktif silahı. Oyuncunun child objesi olarak yaşar ve büyünün atış döngüsünü çalıştırır.
/// Slot değişince WeaponSlotManager bunu yok edip yenisini kurar.
/// </summary>
public class SlotWeapon : MonoBehaviour
{
    public int SlotIndex { get; private set; }
    public WeaponSlot Slot { get; private set; }
    public Spell SpellPrefab { get; private set; }
    public Transform Owner { get; private set; }

    /// <summary>
    /// Büyünün son statları: prefabdaki temel değerler + slot bonusu.
    /// Her okunduğunda yeniden hesaplanır, prefab değerleri oyun sırasında değiştirilirse bir sonraki atışta yansır.
    /// </summary>
    public SpellStatValues Stats
    {
        get
        {
            SpellStats baseStats = SpellPrefab != null ? SpellPrefab.BaseStats : null;
            BonusStats bonus = Slot != null ? Slot.BonusStats : null;
            return SpellStatValues.Combine(baseStats, bonus);
        }
    }

    public void Init(int slotIndex, WeaponSlot slot, Spell spellPrefab, Transform owner)
    {
        SlotIndex = slotIndex;
        Slot = slot;
        SpellPrefab = spellPrefab;
        Owner = owner;

        StartCoroutine(spellPrefab.CastLoop(this));
    }
}
