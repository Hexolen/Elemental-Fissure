using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("RigidBody")]
    [SerializeField] Rigidbody2D rb;

    [Header("Sprites & Animation")]
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;

    [Header("Weapon Slots")]
    [Tooltip("Seviye atlayınca gelen element bu menüyle slota yerleştirilir")]
    [SerializeField] private SlotSelectionMenu slotMenu;

    [Header("Element EXP")]
    [Tooltip("Seviye atlarken hangi elementin geleceğini belirleyen puan takibi")]
    [SerializeField] private ElementExpTracker elementExp;

    [Header("Stats")]
    [SerializeField] float maxHealth = 100;
    [SerializeField] float armor = 5f;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float siphonResidue = 0.5f;
    [SerializeField] int revival = 0;

    [Header("Level System")]
    [SerializeField] private int level = 1;
    [SerializeField] private int currentExp = 0;
    [SerializeField] private int requiredExp = 100;
    [SerializeField] private float expMultiplier = 1.5f;

    private Vector2 moveInput;
    private bool isMoving;

    //GetterMethods
    public bool IsMoving { get { return isMoving; } }
    public float MaxHealth { get { return maxHealth; } }
    public float SiphonResidue { get { return siphonResidue; } }
    public int Level => level;
    public int CurrentExp => currentExp;
    public int RequiredExp => requiredExp;

    public UnityEvent<float> OnSiphonResidueChanged = new UnityEvent<float>();

    private void Awake()
    {
        if (elementExp == null)
            elementExp = GetComponent<ElementExpTracker>();

        if (elementExp == null)
            Debug.LogError($"{name}: ElementExpTracker yok, seviye atlarken element gelmeyecek!", this);
    }

    void FixedUpdate()
    {
        Move();
        Animate();
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        isMoving = moveInput.sqrMagnitude > 0.01f;
    }

    void Move()
    {
        rb.linearVelocity = isMoving ? moveInput * moveSpeed : Vector2.zero;
    }

    void Animate()
    {

        if (animator != null)
            animator.SetBool("isMoving", isMoving);

        if (moveInput.x < 0)
            sprite.flipX = true;
        else if (moveInput.x > 0)
            sprite.flipX = false;
    }

    public void SetSiphonResidue(float newValue)
    {
        siphonResidue = newValue;
        OnSiphonResidueChanged.Invoke(siphonResidue);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, siphonResidue);
    }

    /// <summary>
    /// EXP ve orbun element puanını ekler. Seviye atlanırsa element puan oranlarına göre gelir.
    /// </summary>
    public void AddExp(int amount, ElementType orbElement)
    {
        if (amount <= 0)
        {
            Debug.Log("AddExp called with zero or negative value!");
            return;
        }

        currentExp += amount;
        if (elementExp != null)
            elementExp.AddOrb(orbElement);

        Debug.Log($"EXP Gained: {amount} ({ElementUtility.GetDisplayName(orbElement)}) | Total EXP: {currentExp}/{requiredExp}");

        bool leveledUp = false;
        while (currentExp >= requiredExp)
        {
            LevelUp();
            leveledUp = true;
        }

        // Aynı anda birden fazla seviye atlanırsa hepsi aynı puan dağılımını kullanır, sonra yeni seviye için sıfırlanır
        if (leveledUp && elementExp != null)
            elementExp.ResetPoints();
    }

    private void LevelUp()
    {
        level++;

        currentExp -= requiredExp;

        // Exponential XP hesabı
        requiredExp = Mathf.CeilToInt(requiredExp * expMultiplier);

        Debug.Log($"LEVEL UP! Yeni Level: {level} | Yeni Gerekli EXP: {requiredExp}");

        OnLevelUp();
    }

    private void OnLevelUp()
    {
        if (elementExp == null) return;

        string summary = elementExp.GetSummary();
        ElementType element = elementExp.RollElement(out bool isPure);
        string pureText = isPure ? " (PURE)" : "";
        Debug.Log($"Seviye {level} elementi: {ElementUtility.GetDisplayName(element)}{pureText} | Bu seviyedeki puanlar: {summary}");

        if (slotMenu != null)
            slotMenu.Offer(element, isPure);
        else
            Debug.LogWarning($"{name}: SlotSelectionMenu atanmadı, gelen element slota konamadı.");
    }
}
