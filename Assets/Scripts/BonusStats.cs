using System;
using UnityEngine;

[System.Serializable]
public class BonusStats : MonoBehaviour
{
    public float bonusDamage;
    public float bonusCooldown;
    public float bonusProjectileSpeed;
    public float bonusDuration;
    public float bonusArea;
    public int bonusProjectileAmount;
    public int bonusKnockback;
    public int bonusBounces;

    public void ResetBonuses()
    {
        bonusDamage = 0;
        bonusCooldown = 0;
        bonusProjectileSpeed = 0;
        bonusDuration = 0;
        bonusArea = 0;
        bonusProjectileAmount = 0;
        bonusKnockback = 0;
        bonusBounces = 0;
    }
}