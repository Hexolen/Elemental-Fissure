using System;
using UnityEngine;

/// <summary>
/// Oyuncunun 4 silah slotunu yönetir: element ekleme, bonus hesaplama ve her slotun silahını kurma.
/// Player objesine eklenir.
/// </summary>
public class WeaponSlotManager : MonoBehaviour
{
    public const int SlotCount = 4;

    [Header("Data")]
    [SerializeField] private AllSpellsSO spellDatabase;

    [Header("Rules")]
    [Tooltip("Bir slota konabilecek en fazla element sayısı")]
    [SerializeField, Min(1)] private int maxElementsPerSlot = 6;

    [Header("Başlangıç (ana menü gelene kadar)")]
    [Tooltip("Oyun başında slota konan element. None = başlangıç elementi yok")]
    [SerializeField] private ElementType startingElement = ElementType.Fire;
    [Tooltip("Başlangıç elementinin konacağı slot (1-4)")]
    [SerializeField, Range(1, SlotCount)] private int startingSlot = 1;
    [SerializeField] private bool startingElementIsPure = false;

    [Header("Slots (test için Inspector'dan element eklenebilir)")]
    [SerializeField] private WeaponSlot[] slots = new WeaponSlot[SlotCount];

    private readonly SlotWeapon[] activeWeapons = new SlotWeapon[SlotCount];

    /// <summary>
    /// Bir slota element eklendiğinde slot index'i ile çağrılır.
    /// </summary>
    public event Action<int> OnSlotChanged;

    public int MaxElementsPerSlot => maxElementsPerSlot;
    public AllSpellsSO SpellDatabase => spellDatabase;

    private void Awake()
    {
        EnsureSlots();

        if (spellDatabase == null)
            Debug.LogError($"{name}: WeaponSlotManager'a AllSpells atanmadı!", this);
    }

    private void Start()
    {
        // Başlangıç elementi (slot Inspector'dan doldurulmuşsa üstüne eklenmez)
        int startIndex = startingSlot - 1;
        if (ElementUtility.IsPlayable(startingElement) && IsValidIndex(startIndex) && slots[startIndex].IsEmpty)
            slots[startIndex].AddElement(new SlotElement(startingElement, startingElementIsPure));

        // Dolu slotların silahlarını kur
        for (int i = 0; i < SlotCount; i++)
        {
            slots[i].RecalculateBonus(spellDatabase);
            RebuildWeapon(i);
        }
    }

    private void OnValidate()
    {
        EnsureSlots();
    }

    public WeaponSlot GetSlot(int index)
    {
        return IsValidIndex(index) ? slots[index] : null;
    }

    public bool CanAddElement(int index)
    {
        return IsValidIndex(index) && slots[index].Count < maxElementsPerSlot;
    }

    /// <summary>
    /// En az bir slotta yer var mı.
    /// </summary>
    public bool HasFreeSpace()
    {
        for (int i = 0; i < SlotCount; i++)
        {
            if (CanAddElement(i)) return true;
        }
        return false;
    }

    /// <summary>
    /// Slot tamamen dolu ve hepsi PURE mi (ileride özel güçlendirme için).
    /// </summary>
    public bool IsSlotFullyPure(int index)
    {
        return IsValidIndex(index) && slots[index].Count >= maxElementsPerSlot && slots[index].IsAllPure;
    }

    public bool AddElement(int index, ElementType element, bool isPure)
    {
        if (!ElementUtility.IsPlayable(element))
        {
            Debug.LogWarning($"WeaponSlotManager: {element} elementi slota eklenemez.");
            return false;
        }

        if (!CanAddElement(index))
        {
            Debug.LogWarning($"WeaponSlotManager: Slot {index + 1} dolu veya geçersiz.");
            return false;
        }

        WeaponSlot slot = slots[index];
        slot.AddElement(new SlotElement(element, isPure));
        slot.RecalculateBonus(spellDatabase);
        RebuildWeapon(index);

        string pureText = isPure ? " (PURE)" : "";
        Debug.Log($"Slot {index + 1}: {ElementUtility.GetDisplayName(element)}{pureText} eklendi → {slot.Combo} = {GetSpellName(index)}");

        OnSlotChanged?.Invoke(index);
        return true;
    }

    /// <summary>
    /// Slotun şu anki büyüsünün adı (boşsa "").
    /// </summary>
    public string GetSpellName(int index)
    {
        if (!IsValidIndex(index) || slots[index].IsEmpty || spellDatabase == null) return "";

        spellDatabase.GetSpell(slots[index].Combo, out string spellName);
        return spellName;
    }

    /// <summary>
    /// Bu slota element eklenirse hangi büyünün çıkacağını döndürür (menüde önizleme için).
    /// </summary>
    public string PreviewSpellName(int index, ElementType element)
    {
        if (!IsValidIndex(index) || spellDatabase == null) return "";

        spellDatabase.GetSpell(slots[index].Combo.With(element), out string spellName);
        return spellName;
    }

    /// <summary>
    /// Slotun eski silahını kaldırır, slotun güncel combo'suna göre yenisini kurar.
    /// </summary>
    private void RebuildWeapon(int index)
    {
        if (activeWeapons[index] != null)
        {
            activeWeapons[index].StopAllCoroutines();
            Destroy(activeWeapons[index].gameObject);
            activeWeapons[index] = null;
        }

        WeaponSlot slot = slots[index];
        if (slot.IsEmpty || spellDatabase == null) return;

        Spell spell = spellDatabase.GetSpell(slot.Combo, out string spellName);
        if (spell == null)
        {
            Debug.LogWarning($"Slot {index + 1}: '{slot.Combo}' için büyü yok ve yedek büyü atanmamış.");
            return;
        }

        GameObject weaponObject = new GameObject($"Slot {index + 1} - {spellName}");
        weaponObject.transform.SetParent(transform, false);

        SlotWeapon weapon = weaponObject.AddComponent<SlotWeapon>();
        weapon.Init(index, slot, spell, transform);
        activeWeapons[index] = weapon;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < SlotCount;
    }

    private void EnsureSlots()
    {
        if (slots == null || slots.Length != SlotCount)
            Array.Resize(ref slots, SlotCount);

        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] == null)
                slots[i] = new WeaponSlot();
        }
    }
}
