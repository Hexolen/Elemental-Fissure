using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Vector3 offset = new Vector3(0, 1f, 0);
    [SerializeField] private TextMeshPro textMesh;

    private Color startColor;

    private void Awake()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshPro>();

        startColor = textMesh.color;
        Destroy(gameObject, duration);
    }

    private void Update()
    {
        transform.position += Vector3.up * floatSpeed * Time.deltaTime;

        float alpha = Mathf.Lerp(startColor.a, 0, (1 - (duration - Time.timeSinceLevelLoad % duration) / duration));
        textMesh.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
    }

    public void SetText(float damage)
    {
        textMesh.text = "-" + Mathf.RoundToInt(damage).ToString();
    }
}