using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slot seçim menüsündeki tek bir slot butonu. Slotun içeriğini ve eklenirse çıkacak büyüyü gösterir.
/// </summary>
public class SlotButtonUI : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private TMP_Text label;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.16f, 0.17f, 0.22f, 0.95f);
    [SerializeField] private Color fullColor = new Color(0.08f, 0.08f, 0.08f, 0.7f);

    public Button Button => button;

    public void Show(int index, WeaponSlot slot, int maxElements, bool canAdd, string currentSpell, string previewSpell)
    {
        button.interactable = canAdd;
        if (background != null)
            background.color = canAdd ? normalColor : fullColor;

        StringBuilder text = new StringBuilder();
        text.AppendLine($"<b>Slot {index + 1}</b>  <size=75%>({slot.Count}/{maxElements})</size>");

        if (slot.IsEmpty)
        {
            text.AppendLine("<color=#888888>Boş</color>");
        }
        else
        {
            text.AppendLine(FormatElements(slot));
            text.AppendLine($"<size=80%>Şu an: {currentSpell}</size>");
        }

        if (canAdd)
            text.Append($"<size=80%><color=#FFE08A>Eklenirse: {previewSpell}</color></size>");
        else
            text.Append("<size=80%><color=#FF6666>DOLU</color></size>");

        label.text = text.ToString();
    }

    private static string FormatElements(WeaponSlot slot)
    {
        List<string> parts = new List<string>();

        foreach (ElementType element in ElementUtility.AllElements)
        {
            int count = slot.CountOf(element);
            if (count == 0) continue;

            int pure = slot.PureCountOf(element);
            string pureText = pure > 0 ? $" <size=70%>({pure} PURE)</size>" : "";
            parts.Add(ElementUtility.ColorText(element, $"{ElementUtility.GetDisplayName(element)} x{count}") + pureText);
        }

        return string.Join(", ", parts);
    }
}
