namespace LifeLike.Game;

/// <summary>Wybór na przewijanej liście (sel / top z oknem widocznych wierszy), jak listy w run_shop na GBA.</summary>
public sealed class ListState
{
    public int Sel;
    public int Top;

    public void Reset()
    {
        Sel = 0;
        Top = 0;
    }

    public void Move(int d, int count, int window)
    {
        if (count <= 0 || d == 0) return;
        Sel = (Sel + d + count) % count;
        Clamp(count, window);
    }

    public void Clamp(int count, int window)
    {
        if (Sel >= count) Sel = count - 1;
        if (Sel < 0) Sel = 0;
        if (Sel < Top) Top = Sel;
        if (Sel >= Top + window) Top = Sel - window + 1;
        if (Top < 0) Top = 0;
    }
}
