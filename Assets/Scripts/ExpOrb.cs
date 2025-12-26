using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float pickupDistance = 0.3f;

    [Header("Exp Info")]
    [SerializeField] private string expType = "Small";
    [SerializeField] private int expValue = 10;

    private Transform target;
    private bool isAttracting;

    public void StartAttract(Transform player)
    {
        target = player;
        isAttracting = true;
    }

    private void Update()
    {
        if (!isAttracting || target == null) return;

        // Player’a doðru hýzla yaklaþ
        Vector3 dir = (target.position - transform.position).normalized;
        moveSpeed += acceleration * Time.deltaTime;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // Player’a yeterince yaklaþtýysa topla
        if (Vector3.Distance(transform.position, target.position) < pickupDistance)
        {
            Collect(target.GetComponent<PlayerController>());
        }
    }

    private void Collect(PlayerController player)
    {
        if (player != null)
        {
            player.AddExp(expValue);
        }
        else
        {
            Debug.Log("Player is null! no EXP gained.");
        }

        Destroy(gameObject);
    }

}
