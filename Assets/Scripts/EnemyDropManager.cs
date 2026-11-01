using UnityEngine;

public class EnemyDropManager : MonoBehaviour
{
    [Header("Element")]
    [Tooltip("Bu düşmanın elementi. Düşürdüğü EXP orbu bu elementten olur.")]
    [SerializeField] private ElementType element = ElementType.Neutral;

    [Header("EXP Prefabs")]
    [SerializeField] private GameObject smallExpPrefab;
    [SerializeField] private GameObject mediumExpPrefab;
    [SerializeField] private GameObject largeExpPrefab;

    [Header("Drop Chances (%)")]
    [SerializeField][Range(0, 100)] private float smallChance = 80f;
    [SerializeField][Range(0, 100)] private float mediumChance = 15f;
    [SerializeField][Range(0, 100)] private float largeChance = 5f;

    [Header("Drop Offset")]
    [SerializeField] private Vector3 dropOffset = new Vector3(0, 0.2f, 0);

    public ElementType Element => element;

    public void DropExp()
    {
        float roll = Random.Range(0f, 100f);
        GameObject prefabToDrop = null;

        if (roll <= smallChance)
            prefabToDrop = smallExpPrefab;
        else if (roll <= smallChance + mediumChance)
            prefabToDrop = mediumExpPrefab;
        else
            prefabToDrop = largeExpPrefab;

        if (prefabToDrop != null)
        {
            GameObject drop = Instantiate(prefabToDrop, transform.position + dropOffset, Quaternion.identity);

            if (drop.TryGetComponent(out ExpOrb orb))
                orb.SetElement(element);
        }
    }
}
