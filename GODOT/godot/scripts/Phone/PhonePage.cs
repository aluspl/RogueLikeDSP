using Godot;

namespace LifeLike.Game;

/// <summary>
/// Ekran aplikacji w telefonie (zakładka albo pojedyncza strona: wiadomość, paczka sprzętu, Hurtownia...).
/// Strona rysuje treść przez PhonePainter; nagłówek, pasek statusu i pasek zakładek rysuje Phone.
/// </summary>
public abstract class PhonePage
{
    public Phone Owner { get; set; }

    public abstract string Title { get; }

    /// <summary>Podtytuł po prawej stronie nagłówka (np. „Etap 3/8, 1234 pkt”).</summary>
    public virtual string Sub => "";

    /// <summary>Podpowiedź sterowania na dole ekranu aplikacji.</summary>
    public virtual string Hint => "";

    public abstract void Draw(PhonePainter p);

    /// <summary>Wejście dla strony; true = obsłużone (Phone nie przełącza wtedy zakładek).</summary>
    public virtual bool Input(InputEvent e) => false;

    /// <summary>Wywoływane przy wejściu na stronę (np. wyzerowanie wyboru).</summary>
    public virtual void Enter()
    {
    }

    protected void Redraw() => Owner?.QueueRedraw();

    protected static bool Pressed(InputEvent e, string action, bool echo = false) => e.IsActionPressed(action, echo);

    /// <summary>Strzałka góra/dół z powtarzaniem: -1 / 1 / 0.</summary>
    protected static int VDir(InputEvent e) => Pressed(e, GameInput.Up, true) ? -1 : Pressed(e, GameInput.Down, true) ? 1 : 0;
}
