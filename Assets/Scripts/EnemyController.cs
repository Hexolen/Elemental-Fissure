using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private AllEnemiesSO enemiesSO; // ScriptableObject referansý
    [SerializeField] private float enemySpawnInnerLimit = 3f;
    [SerializeField] private float enemySpawnOuterLimit = 6f;
    [SerializeField] private float spawnInterval = 2f;

    [Header("Target")]
    [SerializeField] private Transform player;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private void SpawnEnemy()
    {
        if (player == null || enemiesSO.enemyPrefabs.Count == 0) return;

        // Hangi prefabý spawnlayacaðýný rastgele seç
        GameObject prefab = enemiesSO.enemyPrefabs[Random.Range(0, enemiesSO.enemyPrefabs.Count)];

        // Random pozisyon
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(enemySpawnInnerLimit, enemySpawnOuterLimit);
        Vector3 spawnPos = player.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;

        GameObject enemy = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Hedefi ver
        EnemyMovement move = enemy.GetComponent<EnemyMovement>();
        if (move != null)
            move.SetTarget(player);
    }
}