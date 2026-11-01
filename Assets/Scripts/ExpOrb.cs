using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float pickupDistance = 0.3f;

    [Header("Exp Info")]
    [SerializeField] private string expType = "Small";
    [SerializeField] private int expValue = 10;

    [Header("Element")]
    [Tooltip("Orbun elementi. Düşmandan düşerken EnemyDropManager bunu düşmanın elementiyle değiştirir.")]
    [SerializeField] private ElementType element = ElementType.Neutral;
    [Tooltip("Elemente göre boyanacak sprite (boşsa child'larda aranır)")]
    [SerializeField] private SpriteRenderer sprite;

    private Transform target;
    private bool isAttracting;
    private Color neutralColor = Color.white;

    public ElementType Element => element;
    public int ExpValue => expValue;

    private void Awake()
    {
        if (sprite == null)
            sprite = GetComponentInChildren<SpriteRenderer>();

        // Prefabdaki orijinal renk nötr orbun rengi olarak kalır
        if (sprite != null)
            neutralColor = sprite.color;

        ApplyElementColor();
    }

    public void SetElement(ElementType newElement)
    {
        element = newElement;
        ApplyElementColor();
    }

    public void StartAttract(Transform player)
    {
        target = player;
        isAttracting = true;
    }

    private void Update()
    {
        if (!isAttracting || target == null) return;

        // Player'a doğru hızla yaklaş
        Vector3 dir = (target.position - transform.position).normalized;
        moveSpeed += acceleration * Time.deltaTime;
        transform.position += dir * moveSpeed * Time.deltaTime;

        // Player'a yeterince yaklaştıysa topla
        if (Vector3.Distance(transform.position, target.position) < pickupDistance)
        {
            Collect(target.GetComponent<PlayerController>());
        }
    }

    private void Collect(PlayerController player)
    {
        if (player != null)
        {
            player.AddExp(expValue, element);
        }
        else
        {
            Debug.Log("Player is null! no EXP gained.");
        }

        Destroy(gameObject);
    }

    private void ApplyElementColor()
    {
        if (sprite == null) return;

        sprite.color = element == ElementType.Neutral || element == ElementType.None
            ? neutralColor
            : ElementUtility.GetColor(element);
    }
}
