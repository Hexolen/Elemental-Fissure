using UnityEngine;

/// <summary>
/// Elementlerle ilgili ortak yardımcılar: isim, renk, rastgele seçim.
/// </summary>
public static class ElementUtility
{
    /// <summary>
    /// None hariç oynanabilir tüm elementler.
    /// </summary>
    public static readonly ElementType[] AllElements =
    {
        ElementType.Air,
        ElementType.Fire,
        ElementType.Water,
        ElementType.Earth
    };

    public static ElementType GetRandom()
    {
        return AllElements[Random.Range(0, AllElements.Length)];
    }

    public static string GetDisplayName(ElementType element)
    {
        switch (element)
        {
            case ElementType.Air: return "Hava";
            case ElementType.Fire: return "Ateş";
            case ElementType.Water: return "Su";
            case ElementType.Earth: return "Toprak";
            default: return "Yok";
        }
    }

    public static Color GetColor(ElementType element)
    {
        switch (element)
        {
            case ElementType.Air: return new Color(0.75f, 0.95f, 0.90f);
            case ElementType.Fire: return new Color(1.00f, 0.45f, 0.20f);
            case ElementType.Water: return new Color(0.25f, 0.55f, 1.00f);
            case ElementType.Earth: return new Color(0.60f, 0.42f, 0.22f);
            default: return Color.white;
        }
    }

    /// <summary>
    /// TextMeshPro için element rengine boyanmış metin.
    /// </summary>
    public static string ColorText(ElementType element, string text)
    {
        return $"<color=#{ColorUtility.ToHtmlStringRGB(GetColor(element))}>{text}</color>";
    }
}
