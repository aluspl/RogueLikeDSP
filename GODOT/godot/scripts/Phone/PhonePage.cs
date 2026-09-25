using LifeLike.Game.Input;

namespace LifeLike.Game.Phone;

/// <summary>
/// Ekran aplikacji w telefonie (zakładka albo pojedyncza strona: wiadomość, paczka sprzętu, Hurtownia...).
/// Strona rysuje treść przez PhonePainter; nagłówek, pasek statusu i pasek zakładek rysuje PhoneView.
/// </summary>
public abstract class PhonePage
{
    public PhoneView Owner { get; set; }

    public abstract string Title { get; }

    /// <summary>Podtytuł po prawej stronie nagłówka (np. „Etap 3/8, 1234 pkt”).</summary>
    public virtual string Sub => "";

    /// <summary>Podpowiedź sterowania na dole ekranu aplikacji.</summary>
    public virtual string Hint => "";

    public abstract void Draw(PhonePainter p);

    /// <summary>Wejście dla strony; true = obsłużone (PhoneView nie przełącza wtedy zakładek).</summary>
    public virtual bool Input(InputCmd e) => false;

    /// <summary>Wywoływane przy wejściu na stronę (np. wyzerowanie wyboru).</summary>
    public virtual void Enter()
    {
    }

    protected void Redraw() => Owner?.QueueRedraw();
}
