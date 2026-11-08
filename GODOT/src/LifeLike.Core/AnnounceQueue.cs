namespace LifeLike.Core;

/// <summary>
/// v0.23.0: kolejka ogłoszeń „z przytupem” (awans, Markowa / Mistrzowska broń, boss pokonany, sekretne zlecenie).
/// Jedno naraz przez Duration, kolejne czekają (najwyżej MaxQueued, najstarsze czekające odpadają), bez powtórzeń.
/// Nie blokuje gry – widok tylko rysuje Current i Elapsed.
/// </summary>
public sealed class AnnounceQueue
{
    public const float Duration = 1.1f;
    /// <summary>v0.23.0: Kawka – głaskanie trwa dłużej (kilka pociągnięć dłoni).</summary>
    public const float DogDuration = 2.2f;
    public const int MaxQueued = 4;
    /// <summary>Klatka animacji zamachu w ogłoszeniu (weapons_swing: 6 klatek).</summary>
    public const float SwingStep = 0.07f;

    private readonly List<Announcement> _items = [];

    public Announcement Current => _items.Count > 0 ? _items[0] : null;

    /// <summary>Czas od pokazania bieżącego (s).</summary>
    public float Elapsed { get; private set; }

    /// <summary>Czas wyświetlania ogłoszenia a (Kawka dłużej).</summary>
    public static float DurationOf(Announcement a) => a is { Kind: AnnounceKind.Dog } ? DogDuration : Duration;

    /// <summary>Czas wyświetlania bieżącego ogłoszenia.</summary>
    public float CurrentDuration => Current is { } c ? DurationOf(c) : Duration;

    public float Progress => Math.Clamp(Elapsed / CurrentDuration, 0f, 1f);

    public int Count => _items.Count;

    public bool Busy => _items.Count > 0;

    public void Push(Announcement a)
    {
        foreach (var x in _items)
        {
            if (x == a) return;
        }
        if (_items.Count >= MaxQueued) _items.RemoveAt(1); // bieżące zostaje na ekranie
        _items.Add(a);
    }

    public void Update(float dt)
    {
        if (_items.Count == 0) return;
        Elapsed += dt;
        if (Elapsed < CurrentDuration) return;
        _items.RemoveAt(0);
        Elapsed = 0f;
    }

    /// <summary>Zrzuty ekranu: bieżące ogłoszenie zatrzymane w chwili t (najwyżej tuż przed końcem).</summary>
    public void Seek(float t) => Elapsed = Math.Clamp(t, 0f, CurrentDuration - 0.001f);

    public void Clear()
    {
        _items.Clear();
        Elapsed = 0f;
    }

    /// <summary>Zamiana z kabury (broń z ręki do kabury i odwrotnie) – to nie znalezisko, bez ogłoszenia nowej broni.</summary>
    public static bool IsHolsterSwap(int handBefore, int holsterBefore, int handAfter, int holsterAfter) =>
        holsterBefore >= 0 && handAfter == holsterBefore && holsterAfter == handBefore;

    /// <summary>Znaleziona broń zasługuje na ogłoszenie: Markowa, Mistrzowska albo broń mistrza (unikat).</summary>
    public static bool WeaponWorthy(int rarity, bool master) => master || rarity >= 2;

    public static AnnounceKind WeaponKind(int rarity, bool master) => master || rarity >= 3 ? AnnounceKind.Unique : AnnounceKind.Branded;

    /// <summary>Klatka zamachu po t sekundach (ostatnia zostaje).</summary>
    public static int SwingFrame(float t, int frames) => Math.Clamp((int)(t / SwingStep), 0, frames - 1);
}
