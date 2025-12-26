using UnityEngine;
using UnityEngine.Events;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float currentHealth;
    [SerializeField] private UnityEngine.UI.Slider healthBar;

    [Header("Damage Text")]
    [SerializeField] private GameObject damageTextPrefab;

    [Header("Death Settings")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private EnemyDropManager dropManager;

    [Header("Events")]
    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;

    private void Awake()
    {
        // Player için
        if (playerController != null)
        {
            maxHealth = playerController.MaxHealth;
        }
        else
        {
            // Enemy için
            if (maxHealth <= 0)
                Debug.LogError($"{name}: PlayerController yok ve MaxHealth deðeri 0 veya atanmadý!");
        }

        currentHealth = maxHealth;

        if (healthBar == null)
            Debug.LogError($"{name}: HealthBar atanmadý!");
        else
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }

    /// <summary>
    /// Maksimum can deðeri (readonly)
    /// </summary>
    public float MaxHealth => maxHealth;

    /// <summary>
    /// Þu anki can deðeri (readonly)
    /// </summary>
    public float CurrentHealth => currentHealth;

    /// <summary>
    /// Hasar alma
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (damage <= 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;
        else
            Debug.LogError($"{name}: HealthBar atanmadý!");

        //Create Health Text
        if (damageTextPrefab == null)
            Debug.LogError($"{name}: DamageTextPrefab atanmadý!");
        else if (gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            GameObject textInstance = Instantiate(
                damageTextPrefab,
                transform.position + Vector3.up * 0.2f,
                Quaternion.identity
            );

            DamageText floatingText = textInstance.GetComponent<DamageText>();
            if (floatingText == null)
                Debug.LogError($"{name}: DamageText scripti prefabda eksik!");
            else
                floatingText.SetText(damage);
        }

        onHealthChanged.Invoke(currentHealth);

        if (currentHealth <= 0)
            Die();
    }

    /// <summary>
    /// Can iyileþtirme
    /// </summary>
    public void Heal(float amount)
    {
        if (amount <= 0) return;

        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthBar != null)
            healthBar.value = currentHealth;
        else
            Debug.LogError($"{name}: HealthBar atanmadý!");

        onHealthChanged.Invoke(currentHealth);
    }

    /// <summary>
    /// Ölüm iþlemi
    /// </summary>
    private void Die()
    {
        if (dropManager == null)
            Debug.LogError($"{name}: DropManager atanmadý!");
        else
            dropManager.DropExp();

        onDeath.Invoke();

        if (destroyOnDeath)
            Destroy(gameObject);
    }


    /// <summary>
    /// Maksimum caný runtime’da deðiþtirmek için
    /// </summary>
    public void SetMaxHealth(float newMax, bool resetCurrent = true)
    {
        maxHealth = Mathf.Max(1, newMax);
        if (resetCurrent)
            currentHealth = maxHealth;

        if (healthBar != null)
            healthBar.value = currentHealth;
        else
            Debug.LogError($"{name}: HealthBar atanmadý!");

        onHealthChanged.Invoke(currentHealth);
    }
}