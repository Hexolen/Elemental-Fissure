using System;
using UnityEngine;

/// <summary>
/// Oyuncunun bu seviyede hangi elementten ne kadar puan topladığını tutar.
/// Seviye atlarken element, puanların oranına göre rastgele seçilir (%60 ateş puanı = %60 ateş gelme şansı).
/// Seçilen elementin payı PURE eşiğinin üstündeyse element PURE gelir.
/// Player objesine eklenir.
/// </summary>
public class ElementExpTracker : MonoBehaviour
{
    [Header("Orb Başına Element Puanı")]
    [Tooltip("Nötr orb toplandığında 4 elementin her birine verilen puan")]
    [SerializeField, Min(0)] private int neutralPointsPerElement = 1;
    [Tooltip("Elementli orb toplandığında o elemente verilen puan")]
    [SerializeField, Min(0)] private int elementalPoints = 4;

    [Header("PURE")]
    [Tooltip("Gelen elementin payı bu oranın üstündeyse PURE olur. 0.8 = %80")]
    [SerializeField, Range(0f, 1f)] private float pureThreshold = 0.8f;

    [Header("Bu Seviyede Toplanan (sadece gösterim)")]
    [SerializeField] private int airPoints;
    [SerializeField] private int firePoints;
    [SerializeField] private int waterPoints;
    [SerializeField] private int earthPoints;

    /// <summary>
    /// Puanlar değiştiğinde (orb toplandı / sıfırlandı) çağrılır. İleride UI için.
    /// </summary>
    public event Action OnPointsChanged;

    public float PureThreshold => pureThreshold;
    public int TotalPoints => airPoints + firePoints + waterPoints + earthPoints;

    public int GetPoints(ElementType element)
    {
        switch (element)
        {
            case ElementType.Air: return airPoints;
            case ElementType.Fire: return firePoints;
            case ElementType.Water: return waterPoints;
            case ElementType.Earth: return earthPoints;
            default: return 0;
        }
    }

    /// <summary>
    /// Elementin bu seviyedeki payı (0-1).
    /// </summary>
    public float GetShare(ElementType element)
    {
        int total = TotalPoints;
        return total > 0 ? (float)GetPoints(element) / total : 0f;
    }

    /// <summary>
    /// Toplanan bir EXP orbunun element puanını ekler.
    /// </summary>
    public void AddOrb(ElementType orbElement)
    {
        if (orbElement == ElementType.Neutral)
        {
            foreach (ElementType element in ElementUtility.AllElements)
                AddPoints(element, neutralPointsPerElement);
        }
        else if (ElementUtility.IsPlayable(orbElement))
        {
            AddPoints(orbElement, elementalPoints);
        }
        else
        {
            return;
        }

        OnPointsChanged?.Invoke();
    }

    /// <summary>
    /// Puan oranlarına göre bir element seçer. Hiç puan yoksa 4 elementten eşit şansla seçer (PURE olmaz).
    /// </summary>
    public ElementType RollElement(out bool isPure)
    {
        isPure = false;
        int total = TotalPoints;

        if (total <= 0)
            return ElementUtility.GetRandom();

        int roll = UnityEngine.Random.Range(0, total);
        ElementType rolled = ElementUtility.AllElements[ElementUtility.AllElements.Length - 1];

        foreach (ElementType element in ElementUtility.AllElements)
        {
            int points = GetPoints(element);
            if (roll < points)
            {
                rolled = element;
                break;
            }
            roll -= points;
        }

        isPure = GetShare(rolled) >= pureThreshold;
        return rolled;
    }

    /// <summary>
    /// Yeni seviye için puanları sıfırlar.
    /// </summary>
    public void ResetPoints()
    {
        airPoints = 0;
        firePoints = 0;
        waterPoints = 0;
        earthPoints = 0;
        OnPointsChanged?.Invoke();
    }

    /// <summary>
    /// Log için: "Hava %10, Ateş %60, Su %30, Toprak %0"
    /// </summary>
    public string GetSummary()
    {
        string[] parts = new string[ElementUtility.AllElements.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            ElementType element = ElementUtility.AllElements[i];
            parts[i] = $"{ElementUtility.GetDisplayName(element)} %{GetShare(element) * 100f:0}";
        }
        return string.Join(", ", parts);
    }

    private void AddPoints(ElementType element, int amount)
    {
        switch (element)
        {
            case ElementType.Air: airPoints += amount; break;
            case ElementType.Fire: firePoints += amount; break;
            case ElementType.Water: waterPoints += amount; break;
            case ElementType.Earth: earthPoints += amount; break;
        }
    }
}
