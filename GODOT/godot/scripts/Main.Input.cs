using System;
using Godot;
using LifeLike.Core;
using CoreGame = LifeLike.Core.Game;

namespace LifeLike.Game;

/// <summary>Wejście (klawiatura / pad / mysz) dla ekranów - wyłącznie przez nazwy akcji z GameInput.</summary>
public partial class Main
{
    private static bool Pressed(InputEvent e, string action, bool echo = false) => e.IsActionPressed(action, echo);

    private static int VDir(InputEvent e) => Pressed(e, GameInput.Up, true) ? -1 : Pressed(e, GameInput.Down, true) ? 1 : 0;

    public override void _UnhandledInput(InputEvent e)
    {
        if (_g is null) return;
        var handled = _screen switch
        {
            Screen.Title => TitleInput(e),
            Screen.ClassSelect => ClassSelectInput(e),
            Screen.Profile => ProfileInput(e),
            Screen.Playing => PlayInput(e),
            Screen.Phone => PhoneInput(e),
            Screen.StageClear or Screen.StageCard => StageClearInput(e),
            Screen.Hurtownia => HurtowniaInput(e),
            Screen.EndMessage => EndMessageInput(e),
            Screen.End => EndInput(e),
            Screen.GearOffer => OfferInput(e),
            Screen.ActionMenu => ActionMenuInput(e),
            _ => false,
        };
        if (handled) GetViewport().SetInputAsHandled();
    }

    private bool TitleInput(InputEvent e)
    {
        var v = VDir(e);
        if (v != 0)
        {
            _titleSel = (_titleSel + v + TitleScreen.Items.Length) % TitleScreen.Items.Length;
            _note = "";
            Sfx.Play("menu");
            ShowTitle();
            return true;
        }
        if (Pressed(e, GameInput.Profile))
        {
            ShowProfile(0);
            return true;
        }
        if (Pressed(e, GameInput.Shop))
        {
            ShowProfile(4);
            return true;
        }
        if (!Pressed(e, GameInput.Attack) && !Pressed(e, GameInput.Confirm)) return false;
        Sfx.Play("menu");
        switch (_titleSel)
        {
            case 0:
                ShowClassSelect();
                break;
            case 1:
                ShowProfile(0);
                break;
            default:
                ShowProfile(4);
                break;
        }
        return true;
    }

    private bool ClassSelectInput(InputEvent e)
    {
        if (Pressed(e, GameInput.Left, true) || Pressed(e, GameInput.Right, true))
        {
            _classSelect.Move(Pressed(e, GameInput.Left, true) ? -1 : 1);
            _cls = _classSelect.Selected;
            _classSelect.Note = "";
            return true;
        }
        var v = VDir(e);
        if (v != 0)
        {
            var n = _d.Difficulties.Length;
            do _diff = (_diff + v + n) % n;
            while (!Meta.DifficultyUnlocked(_d, _profile, _diff));
            _classSelect.Difficulty = _diff;
            Sfx.Play("menu");
            return true;
        }
        if (Pressed(e, GameInput.KeepPrev) || Pressed(e, GameInput.KeepNext))
        {
            Meta.CycleKeepsake(_d, _profile, Pressed(e, GameInput.KeepPrev) ? -1 : 1);
            SaveProfile();
            Sfx.Play("menu");
            return true;
        }
        if (Pressed(e, GameInput.Shop))
        {
            ShowProfile(4);
            return true;
        }
        if (Pressed(e, GameInput.Profile))
        {
            ShowProfile(0);
            return true;
        }
        if (Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel))
        {
            ShowTitle();
            return true;
        }
        if (!Pressed(e, GameInput.Attack) && !Pressed(e, GameInput.Confirm)) return false;
        _cls = _classSelect.Selected;
        if (!Meta.ClassUnlocked(_profile, _cls))
        {
            _classSelect.Note = $"Ten zawód odblokujesz w Szkoleniach (K) za {_d.ClassCost} dośw.";
            Sfx.Play("hurt", 0.5f);
            return true;
        }
        Sfx.Play("stage");
        StartRun();
        return true;
    }

    private bool ProfileInput(InputEvent e)
    {
        if (_phone.HandleInput(e)) return true;
        if (!Pressed(e, GameInput.Profile) && !Pressed(e, GameInput.Wait) && !Pressed(e, GameInput.Cancel)
            && !Pressed(e, GameInput.Phone) && !Pressed(e, GameInput.Shop) && !Pressed(e, GameInput.Confirm)) return false;
        _note = "";
        _phone.Close();
        ShowTitle();
        return true;
    }

    private bool PhoneInput(InputEvent e)
    {
        if (_phone.HandleInput(e)) return true;
        if (!Pressed(e, GameInput.Phone) && !Pressed(e, GameInput.Cancel) && !Pressed(e, GameInput.Wait) && !Pressed(e, GameInput.Confirm)) return false;
        ClosePhone();
        return true;
    }

    private bool PlayInput(InputEvent e)
    {
        if (_view.OverviewOn)
        {
            if (e is not InputEventKey { Pressed: true } && e is not InputEventJoypadButton { Pressed: true }) return false;
            _view.ToggleOverview();
            _hud.ShowHint(null);
            return true;
        }
        if (Pressed(e, GameInput.Phone))
        {
            ShowPhone();
            return true;
        }
        if (Pressed(e, GameInput.Map))
        {
            _view.ToggleOverview();
            _hud.ShowHint("Podgląd mapy etapu", "Dowolny klawisz: wróć");
            return true;
        }
        if (Pressed(e, GameInput.Confirm))
        {
            OpenMenu();
            return true;
        }
        int dx = 0, dy = 0;
        if (Pressed(e, GameInput.Up, true)) dy = -1;
        else if (Pressed(e, GameInput.Down, true)) dy = 1;
        else if (Pressed(e, GameInput.Left, true)) dx = -1;
        else if (Pressed(e, GameInput.Right, true)) dx = 1;
        bool acted;
        if (dx != 0 || dy != 0) acted = _g.PlayerMove(dx, dy);
        else if (Pressed(e, GameInput.Attack))
        {
            acted = AttackNearest();
            if (!acted) _view.FlashRange();
        }
        else if (Pressed(e, GameInput.Wait)) acted = _g.PlayerWait();
        else if (Pressed(e, GameInput.Ability)) acted = UseAbility();
        else if (e is InputEventMouseButton { Pressed: true } mb) acted = MouseAction(mb);
        else return false;
        AfterAction(acted);
        return true;
    }

    /// <summary>A: atak najbliższego widocznego celu w zasięgu (jak celowanie na GBA bez przełączania celu).</summary>
    private bool AttackNearest()
    {
        Span<sbyte> t = stackalloc sbyte[CoreGame.MaxEnemies];
        return _g.TargetsInRange(t) > 0 ? _g.PlayerAttack(t[0]) : _g.PlayerAttackNearest();
    }

    /// <summary>Mysz: lewy klik na wroga w zasięgu = atak, gdzie indziej = krok w tę stronę; prawy klik = czekaj.</summary>
    private bool MouseAction(InputEventMouseButton mb)
    {
        if (mb.ButtonIndex == MouseButton.Right) return _g.PlayerWait();
        if (mb.ButtonIndex != MouseButton.Left) return false;
        var cell = _view.ScreenToGrid(_view.GetGlobalMousePosition());
        var ei = _g.EnemyAt(cell.X, cell.Y);
        if (ei >= 0 && _g.Visible(cell.X, cell.Y) && CoreGame.Cheb(_g.Hero.X, _g.Hero.Y, cell.X, cell.Y) <= _g.Weapon.Range)
            return _g.PlayerAttack(ei);
        int dx = cell.X - _g.Hero.X, dy = cell.Y - _g.Hero.Y;
        if (dx == 0 && dy == 0) return _g.PlayerWait();
        return Math.Abs(dx) >= Math.Abs(dy) ? _g.PlayerMove(Math.Sign(dx), 0) : _g.PlayerMove(0, Math.Sign(dy));
    }

    private bool ActionMenuInput(InputEvent e)
    {
        for (var k = 0; k < 4; k++)
        {
            var action = k switch { 0 => GameInput.Up, 1 => GameInput.Right, 2 => GameInput.Down, _ => GameInput.Left };
            if (!Pressed(e, action)) continue;
            MenuPick(k);
            return true;
        }
        if (Pressed(e, GameInput.Attack) && _view.MenuSel >= 0) MenuExecute();
        else if (Pressed(e, GameInput.Confirm) || Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel))
        {
            CloseMenu();
            Refresh();
        }
        else return false;
        return true;
    }

    private bool OfferInput(InputEvent e)
    {
        if (Pressed(e, GameInput.Attack)) CloseOffer(true);
        else if (Pressed(e, GameInput.Wait) || Pressed(e, GameInput.Cancel)) CloseOffer(false);
        else return false;
        return true;
    }

    private bool StageClearInput(InputEvent e)
    {
        if (!Pressed(e, GameInput.Attack) && !Pressed(e, GameInput.Confirm)) return false;
        Advance();
        return true;
    }

    private bool HurtowniaInput(InputEvent e)
    {
        if (_phone.HandleInput(e))
        {
            _hud.ShowGame(_g);
            return true;
        }
        if (!Pressed(e, GameInput.Confirm) && !Pressed(e, GameInput.Wait) && !Pressed(e, GameInput.Cancel)) return false;
        Advance();
        return true;
    }

    private bool EndMessageInput(InputEvent e)
    {
        if (!Pressed(e, GameInput.Attack) && !Pressed(e, GameInput.Confirm)) return false;
        ShowEnd();
        return true;
    }

    private bool EndInput(InputEvent e)
    {
        if (_g.St == GameStatus.Won && Pressed(e, GameInput.Attack))
        {
            _g.NewGamePlus();
            _note = "";
            _events.Reset(_g);
            Refresh();
            ShowStageCard();
            return true;
        }
        if (!Pressed(e, GameInput.Confirm) && !Pressed(e, GameInput.Attack)) return false;
        _note = "";
        ShowTitle();
        return true;
    }
}
