using System;

/// <summary>
/// Bir slota konmuş tek bir element.
/// PURE bilgisi slot bonusu ve ileride gelecek özel etkiler (ör. tamamı PURE slot) için saklanır.
/// </summary>
[Serializable]
public struct SlotElement
{
    public ElementType element;
    public bool isPure;

    public SlotElement(ElementType element, bool isPure)
    {
        this.element = element;
        this.isPure = isPure;
    }
}
