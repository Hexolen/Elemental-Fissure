using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Bir slottaki elementlerin sayımı. Sıra önemsiz: "Ateş + Su" ile "Su + Ateş" aynı combo.
/// Büyü tarifleri (AllSpellsSO) bu sayımla birebir eşleşir.
/// </summary>
[Serializable]
public struct ElementCombo : IEquatable<ElementCombo>
{
    [Min(0)] public int air;
    [Min(0)] public int fire;
    [Min(0)] public int water;
    [Min(0)] public int earth;

    public int Total => air + fire + water + earth;
    public bool IsEmpty => Total == 0;

    /// <summary>
    /// Kaç farklı element türü var (ör. 2 Ateş + 1 Su = 2).
    /// </summary>
    public int DistinctCount
    {
        get
        {
            int count = 0;
            if (air > 0) count++;
            if (fire > 0) count++;
            if (water > 0) count++;
            if (earth > 0) count++;
            return count;
        }
    }

    /// <summary>
    /// Sadece tek tür elementten oluşuyorsa o elementi döndürür (ör. 3x Ateş → Fire), karışıksa None.
    /// </summary>
    public ElementType SingleElement
    {
        get
        {
            if (DistinctCount != 1) return ElementType.None;
            if (air > 0) return ElementType.Air;
            if (fire > 0) return ElementType.Fire;
            if (water > 0) return ElementType.Water;
            return ElementType.Earth;
        }
    }

    public int Get(ElementType element)
    {
        switch (element)
        {
            case ElementType.Air: return air;
            case ElementType.Fire: return fire;
            case ElementType.Water: return water;
            case ElementType.Earth: return earth;
            default: return 0;
        }
    }

    /// <summary>
    /// Bu combo'ya bir element eklenmiş halini döndürür (kendisi değişmez).
    /// </summary>
    public ElementCombo With(ElementType element)
    {
        ElementCombo result = this;
        switch (element)
        {
            case ElementType.Air: result.air++; break;
            case ElementType.Fire: result.fire++; break;
            case ElementType.Water: result.water++; break;
            case ElementType.Earth: result.earth++; break;
        }
        return result;
    }

    public static ElementCombo From(IEnumerable<SlotElement> elements)
    {
        ElementCombo combo = default;
        foreach (SlotElement e in elements)
            combo = combo.With(e.element);
        return combo;
    }

    public bool Equals(ElementCombo other)
    {
        return air == other.air && fire == other.fire && water == other.water && earth == other.earth;
    }

    public override bool Equals(object obj) => obj is ElementCombo other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(air, fire, water, earth);
    public static bool operator ==(ElementCombo a, ElementCombo b) => a.Equals(b);
    public static bool operator !=(ElementCombo a, ElementCombo b) => !a.Equals(b);

    public override string ToString()
    {
        if (IsEmpty) return "Boş";

        List<string> parts = new List<string>();
        foreach (ElementType element in ElementUtility.AllElements)
        {
            int count = Get(element);
            if (count > 0)
                parts.Add($"{ElementUtility.GetDisplayName(element)} x{count}");
        }
        return string.Join(" + ", parts);
    }
}
