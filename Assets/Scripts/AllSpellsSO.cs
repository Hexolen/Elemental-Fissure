using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllSpells", menuName = "Scriptable Objects/AllSpellsSO")]
public class AllSpellsSO : ScriptableObject
{
    [SerializeField] public List<GameObject> Spells = new List<GameObject>();
}
