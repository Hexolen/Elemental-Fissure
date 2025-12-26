using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;

    [Header("References")]
    [SerializeField] private Rigidbody2D rb;                // inspector’dan atayabilirsin
    [SerializeField] private SpriteRenderer spriteRenderer; // inspector’dan atayabilirsin

    private Transform target;

    public void SetTarget(Transform player)
    {
        target = player;
    }

    private void FixedUpdate()
    {
        if (target == null || rb == null) return;

        // Hedefe doðru yön
        Vector2 dir = (target.position - transform.position).normalized;

        // Hareket
        rb.linearVelocity = dir * moveSpeed;

        // Yönüne göre sprite’ý çevir
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = dir.x < 0;
        }
    }
}