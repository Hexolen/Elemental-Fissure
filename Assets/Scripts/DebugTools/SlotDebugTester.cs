using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Seviye atlama sistemi gelene kadar slot sistemini test etmek için.
/// 1: Hava, 2: Ateş, 3: Su, 4: Toprak, 5: Rastgele. Shift basılıysa element PURE gelir.
/// </summary>
public class SlotDebugTester : MonoBehaviour
{
    [SerializeField] private SlotSelectionMenu slotMenu;
    [SerializeField] private bool debugKeysEnabled = true;

    private void Update()
    {
        if (!debugKeysEnabled || slotMenu == null) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        bool isPure = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

        if (keyboard.digit1Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Air, isPure);
        else if (keyboard.digit2Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Fire, isPure);
        else if (keyboard.digit3Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Water, isPure);
        else if (keyboard.digit4Key.wasPressedThisFrame) slotMenu.Offer(ElementType.Earth, isPure);
        else if (keyboard.digit5Key.wasPressedThisFrame) slotMenu.Offer(ElementUtility.GetRandom(), isPure);
    }
}
