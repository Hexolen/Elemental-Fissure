using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AllEnemies", menuName = "Scriptable Objects/AllEnemiesSO")]
public class AllEnemiesSO : ScriptableObject
{
    [SerializeField] public List<GameObject> enemyPrefabs = new List<GameObject>();
}
