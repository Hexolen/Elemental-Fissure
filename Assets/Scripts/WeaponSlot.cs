using UnityEngine;

[System.Serializable]
public class WeaponSlot
{
    public ElementType element = ElementType.None;
    public BonusStats bonusStats;

    public bool IsEmpty => element == ElementType.None;

    public WeaponSlot()
    {
        bonusStats = new BonusStats();
    }
}
