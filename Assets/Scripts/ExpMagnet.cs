using UnityEngine;

public class ExpMagnet : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private CircleCollider2D col;

    private void Start()
    {
        if (player == null || col == null)
        {
            Debug.LogError($"{name}: PlayerController veya CircleCollider2D eksik!");
            return;
        }

        // Baþlangýçta eþitle
        col.radius = player.SiphonResidue;

        // Deðiþiklikleri dinle
        player.OnSiphonResidueChanged.AddListener(UpdateColliderRadius);
    }

    private void UpdateColliderRadius(float newRadius)
    {
        col.radius = newRadius;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ExpOrb exp = collision.GetComponent<ExpOrb>();
        if (exp != null)
        {
            exp.StartAttract(player.transform);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.TryGetComponent(out ExpOrb orb))
            orb.StartAttract(player.transform);
    }
}
