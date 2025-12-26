using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum SpellNames
{
    AIRSPELL = 0,
    FIRESPELL = 1,
    WATERSPELL = 2,
    EARTHSPELL = 3
}

public class PlayerController : MonoBehaviour
{
    [Header("RigidBody")]
    [SerializeField] Rigidbody2D rb;

    [Header("Sprites & Animation")]
    [SerializeField] SpriteRenderer sprite;
    [SerializeField] Animator animator;

    [Header("Spells")]
    [SerializeField] AllSpellsSO allSpells;

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

    private void Start()
    {
        AirSpell.Spawn(this.transform, allSpells.Spells[(int)SpellNames.AIRSPELL], this);
        FireSpell.Spawn(this.transform, allSpells.Spells[(int)SpellNames.FIRESPELL], this);
        WaterSpell.Spawn(this.transform, allSpells.Spells[(int)SpellNames.WATERSPELL], this);
        EarthSpell.Spawn(this.transform, allSpells.Spells[(int)SpellNames.EARTHSPELL], this);
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

    public void AddExp(int amount)
    {
        if (amount > 0)
        {
            currentExp += amount;

            Debug.Log($"EXP Gained: {amount} | Total EXP: {currentExp}/{requiredExp}");

            while (currentExp >= requiredExp)
            {
                LevelUp();
            }
        }
        else
        {
            Debug.Log("AddExp called with zero or negative value!");
        }
    }

    private void LevelUp()
    {
        level++;

        currentExp -= requiredExp;

        // Exponential XP hesabý
        requiredExp = Mathf.CeilToInt(requiredExp * expMultiplier);

        Debug.Log($"LEVEL UP! Yeni Level: {level} | Yeni Gerekli EXP: {requiredExp}");

        OnLevelUp();
    }

    private void OnLevelUp()
    {
        //Doldur
    }

}