using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Yeni bir element geldiğinde açılan menü. Ortada gelen element, + şeklinde 4 slot butonu var.
/// Oyuncu bir slota tıklayınca element oraya eklenir. Menü açıkken oyun durur.
/// Birden fazla element arka arkaya gelirse (ör. aynı anda 2 seviye) sırayla gösterilir.
/// </summary>
public class SlotSelectionMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponSlotManager slotManager;
    [SerializeField] private GameObject panel;
    [SerializeField] private Image centerImage;
    [SerializeField] private TMP_Text centerLabel;

    [Tooltip("0: Üst, 1: Sağ, 2: Alt, 3: Sol")]
    [SerializeField] private SlotButtonUI[] slotButtons = new SlotButtonUI[WeaponSlotManager.SlotCount];

    private readonly Queue<SlotElement> pending = new Queue<SlotElement>();
    private SlotElement current;
    private bool isOpen;
    private float timeScaleBeforeOpen = 1f;

    public bool IsOpen => isOpen;

    private void Awake()
    {
        if (panel != null)
            panel.SetActive(false);

        for (int i = 0; i < slotButtons.Length; i++)
        {
            int index = i;
            if (slotButtons[i] != null && slotButtons[i].Button != null)
                slotButtons[i].Button.onClick.AddListener(() => OnSlotClicked(index));
        }
    }

    /// <summary>
    /// Oyuncuya bir element ver. Menü açıksa sıraya girer.
    /// </summary>
    public void Offer(ElementType element, bool isPure)
    {
        if (!ElementUtility.IsPlayable(element)) return;

        if (slotManager == null)
        {
            Debug.LogError($"{name}: SlotSelectionMenu'ye WeaponSlotManager atanmadı!", this);
            return;
        }

        pending.Enqueue(new SlotElement(element, isPure));

        if (!isOpen)
            ShowNext();
    }

    private void ShowNext()
    {
        while (pending.Count > 0)
        {
            SlotElement next = pending.Dequeue();

            if (!slotManager.HasFreeSpace())
            {
                Debug.LogWarning($"Tüm slotlar dolu, {ElementUtility.GetDisplayName(next.element)} atlandı.");
                continue;
            }

            Open(next);
            return;
        }

        Close();
    }

    private void Open(SlotElement element)
    {
        current = element;

        if (!isOpen)
        {
            timeScaleBeforeOpen = Time.timeScale;
            Time.timeScale = 0f;
            isOpen = true;
        }

        panel.SetActive(true);
        RefreshCenter();
        RefreshSlots();
    }

    private void Close()
    {
        if (!isOpen) return;

        isOpen = false;
        panel.SetActive(false);
        Time.timeScale = timeScaleBeforeOpen;
    }

    private void RefreshCenter()
    {
        if (centerImage != null)
            centerImage.color = ElementUtility.GetColor(current.element);

        if (centerLabel != null)
        {
            string pureText = current.isPure ? "\n<size=60%><color=#FFD700>PURE</color></size>" : "";
            centerLabel.text = $"<b>{ElementUtility.GetDisplayName(current.element)}</b>{pureText}";
        }
    }

    private void RefreshSlots()
    {
        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null) continue;

            WeaponSlot slot = slotManager.GetSlot(i);
            if (slot == null) continue;

            slotButtons[i].Show(
                i,
                slot,
                slotManager.MaxElementsPerSlot,
                slotManager.CanAddElement(i),
                slotManager.GetSpellName(i),
                slotManager.PreviewSpellName(i, current.element));
        }
    }

    private void OnSlotClicked(int index)
    {
        if (!isOpen) return;

        if (slotManager.AddElement(index, current.element, current.isPure))
            ShowNext();
    }
}
