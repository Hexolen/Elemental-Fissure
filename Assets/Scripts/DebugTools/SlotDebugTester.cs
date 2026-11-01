using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Slot ve EXP sistemlerini test etmek için kısayollar.
/// Element ver (seviye atlamadan menüyü açar): 1 Hava, 2 Ateş, 3 Su, 4 Toprak, 5 Rastgele. Shift = PURE.
/// EXP orbu topla (gerçek EXP + element puanı): F1 Nötr, F2 Hava, F3 Ateş, F4 Su, F5 Toprak.
/// </summary>
public class SlotDebugTester : MonoBehaviour
{
    [SerializeField] private SlotSelectionMenu slotMenu;
    [SerializeField] private bool debugKeysEnabled = true;

    [Header("EXP Test")]
    [Tooltip("F1-F5 ile toplanan sahte orbun EXP değeri")]
    [SerializeField] private int debugExpAmount = 10;

    private PlayerController player;

    private void Awake()
    {
        player = GetComponent<PlayerController>();
    }

    private void Update()
    {
        if (!debugKeysEnabled) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        HandleElementKeys(keyboard);
        HandleExpKeys(keyboard);
    }

    private void HandleElementKeys(Keyboard keyboard)
    {
        if (slotMenu == null) return;

        bool isPure = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        if (keyboard.digit1Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Air, isPure);
        else if (keyboard.digit2Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Fire, isPure);
        else if (keyboard.digit3Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Water, isPure);
        else if (keyboard.digit4Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Earth, isPure);
        else if (keyboard.digit5Key.wasPressedThisFrame) slotMenu.Offer(ElementUtility.GetRandom(), isPure);
    }

    private void HandleExpKeys(Keyboard keyboard)
    {
        if (player == null) return;

        // Menü açıkken EXP verme: oyun durmuşken orb toplanmaz
        if (slotMenu != null && slotMenu.IsOpen) return;

        if (keyboard.f1Key.wasPressedThisFrame) player.AddExp(debugExpAmount, ElementType.Neutral);
        else if (keyboard.f2Key.wasPressedThisFrame) player.AddExp(debugExpAmount, ElementType.Air);
        else if (keyboard.f3Key.wasPressedThisFrame) player.AddExp(debugExpAmount, ElementType.Fire);
        else if (keyboard.f4Key.wasPressedThisFrame) player.AddExp(debugExpAmount, ElementType.Water);
        else if (keyboard.f5Key.wasPressedThisFrame) player.AddExp(debugExpAmount, ElementType.Earth);
    }
}
