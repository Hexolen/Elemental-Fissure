using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Oyuncunun tek bir silah slotu.
/// İçine konan elementleri (geliş sırası ve PURE bilgisiyle) ve bunlardan gelen bonusu tutar.
/// Slotun hangi büyüyü atacağına AllSpellsSO karar verir.
/// </summary>
[Serializable]
public class WeaponSlot
{
    [SerializeField] private List<SlotElement> elements = new List<SlotElement>();

    [Tooltip("PURE elementlerden otomatik hesaplanır")]
    [SerializeField] private BonusStats bonusStats = new BonusStats();

    public IReadOnlyList<SlotElement> Elements => elements;
    public int Count => elements.Count;
    public bool IsEmpty => elements.Count == 0;
    public BonusStats BonusStats => bonusStats;
    public ElementCombo Combo => ElementCombo.From(elements);

    public int PureCount
    {
        get
        {
            int count = 0;
            foreach (SlotElement e in elements)
                if (e.isPure) count++;
            return count;
        }
    }

    /// <summary>
    /// Slottaki elementlerin hepsi PURE mi (ileride özel güçlendirme için).
    /// </summary>
    public bool IsAllPure => Count > 0 && PureCount == Count;

    public int CountOf(ElementType element)
    {
        int count = 0;
        foreach (SlotElement e in elements)
            if (e.element == element) count++;
        return count;
    }

    public int PureCountOf(ElementType element)
    {
        int count = 0;
        foreach (SlotElement e in elements)
            if (e.element == element && e.isPure) count++;
        return count;
    }

    public void AddElement(SlotElement element)
    {
        elements.Add(element);
    }

    public void Clear()
    {
        elements.Clear();
        bonusStats.ResetBonuses();
    }

    /// <summary>
    /// Bonusu sıfırdan hesaplar: her PURE element kendi bonusunu ekler.
    /// </summary>
    public void RecalculateBonus(AllSpellsSO spellDatabase)
    {
        bonusStats.ResetBonuses();
        if (spellDatabase == null) return;

        foreach (SlotElement e in elements)
        {
            if (e.isPure)
                bonusStats.Add(spellDatabase.GetPureBonus(e.element));
        }
    }
}
