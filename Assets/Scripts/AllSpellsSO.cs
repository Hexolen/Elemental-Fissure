using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Büyü veritabanı: hangi element / element karışımı hangi büyüyü verir ve PURE elementler hangi bonusu verir.
/// </summary>
[CreateAssetMenu(fileName = "AllSpells", menuName = "Scriptable Objects/AllSpellsSO")]
public class AllSpellsSO : ScriptableObject
{
    [Serializable]
    public class ElementSpellEntry
    {
        public ElementType element;

        [Tooltip("Slotta sadece bu element varsa (1x, 2x, 3x Ateş gibi) çalışacak büyü")]
        public Spell spellPrefab;

        [Tooltip("Bu element PURE gelirse koyulduğu slota verdiği bonus")]
        public BonusStats pureBonus = new BonusStats();
    }

    [Serializable]
    public class SpellRecipe
    {
        public string displayName;

        [Tooltip("Slotta tam olarak bu sayıda element olmalı. Sıra önemsiz.")]
        public ElementCombo combo;

        public Spell spellPrefab;
    }

    [Header("Ana Element Büyüleri")]
    public List<ElementSpellEntry> elementSpells = new List<ElementSpellEntry>();

    [Header("Kombinasyonlar")]
    [Tooltip("Element karışımlarından çıkan büyüler. Burada olmayan karışımlar yedek büyüyü kullanır.")]
    public List<SpellRecipe> recipes = new List<SpellRecipe>();

    [Header("Yedek")]
    [Tooltip("Tarifi olmayan tüm karışımlar bu büyüyü kullanır")]
    public Spell fallbackSpell;

    public ElementSpellEntry GetElementEntry(ElementType element)
    {
        foreach (ElementSpellEntry entry in elementSpells)
        {
            if (entry != null && entry.element == element)
                return entry;
        }
        return null;
    }

    public BonusStats GetPureBonus(ElementType element)
    {
        ElementSpellEntry entry = GetElementEntry(element);
        return entry != null ? entry.pureBonus : null;
    }

    /// <summary>
    /// Bir combo için çalışacak büyüyü bulur.
    /// Sıra: 1) birebir eşleşen tarif, 2) tek tür elementse o elementin büyüsü, 3) yedek büyü.
    /// </summary>
    public Spell GetSpell(ElementCombo combo, out string displayName)
    {
        displayName = "";
        if (combo.IsEmpty) return null;

        foreach (SpellRecipe recipe in recipes)
        {
            if (recipe == null || recipe.spellPrefab == null || recipe.combo != combo) continue;

            displayName = string.IsNullOrEmpty(recipe.displayName) ? recipe.spellPrefab.name : recipe.displayName;
            return recipe.spellPrefab;
        }

        ElementType single = combo.SingleElement;
        if (single != ElementType.None)
        {
            ElementSpellEntry entry = GetElementEntry(single);
            if (entry != null && entry.spellPrefab != null)
            {
                displayName = entry.spellPrefab.name;
                return entry.spellPrefab;
            }
        }

        if (fallbackSpell != null)
            displayName = fallbackSpell.name;

        return fallbackSpell;
    }

    private void OnValidate()
    {
        // Aynı combo'ya iki tarif yazılırsa sadece ilki çalışır, uyar.
        for (int i = 0; i < recipes.Count; i++)
        {
            if (recipes[i] == null || recipes[i].combo.IsEmpty) continue;

            for (int j = i + 1; j < recipes.Count; j++)
            {
                if (recipes[j] != null && recipes[j].combo == recipes[i].combo)
                    Debug.LogWarning($"{name}: '{recipes[i].combo}' için birden fazla tarif var, sadece ilki kullanılır.", this);
            }
        }
    }
}
