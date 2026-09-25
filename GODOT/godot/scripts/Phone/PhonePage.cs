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

    /// <summary>Podpowiedź sterowania na dole ekranu aplikacji (klawiatura / pad).</summary>
    public virtual string Hint => "";

    /// <summary>Przyciski na dole strony przy sterowaniu dotykiem (zamiast Hint).</summary>
    public virtual PageAction[] Actions => [];

    /// <summary>Krzyżyk w nagłówku przy dotyku (zamyka stronę akcją Cancel); zakładki mają go zawsze.</summary>
    public virtual bool Closable => false;

    /// <summary>Dotknięcie wiersza listy zarejestrowanego przez PhonePainter.HitRow; true = obsłużone.</summary>
    public virtual bool TapRow(int index) => false;

    public abstract void Draw(PhonePainter p);

    /// <summary>Wejście dla strony; true = obsłużone (PhoneView nie przełącza wtedy zakładek).</summary>
    public virtual bool Input(InputCmd e) => false;

    /// <summary>Wywoływane przy wejściu na stronę (np. wyzerowanie wyboru).</summary>
    public virtual void Enter()
    {
    }

    protected void Redraw() => Owner?.QueueRedraw();
}
