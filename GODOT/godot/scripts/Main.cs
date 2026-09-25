using System;
using Godot;
using LifeLike.Core;
using LifeLike.Core.Data;
using LifeLike.Game.Debug;
using LifeLike.Game.Gfx;
using LifeLike.Game.Input;
using LifeLike.Game.Session;

namespace LifeLike.Game;

/// <summary>
/// Korzeń sceny (scenes/Main.tscn): wczytuje dane (game.json wspólny z GBA) i profil, składa App (sesja, węzły,
/// ekrany) i przekazuje ekranowi bieżącemu wejście (jako akcje gry) i czas. Cała logika gry siedzi
/// w LifeLike.Core (port 1:1 z GBA), przejścia ekranów w Screens/, zrzuty i test dymny w Debug/.
/// </summary>
public partial class Main : Node2D
{
	[Export] public uint Seed { get; set; } // 0 = losowy

	private App _app;

	public override void _Ready()
	{
		GameInput.Register();
		RenderingServer.SetDefaultClearColor(Pal.Void);
		Layout.Track(GetTree().Root);
		GameData d;
		try
		{
			d = GodotDataSource.LoadGameData();
		}
		catch (Exception ex)
		{
			GD.PushError($"Dane gry: {ex.Message}");
			GetTree().Quit(1);
			return;
		}
		var opts = LaunchOptions.Parse(OS.GetCmdlineUserArgs(), Seed);
		var profile = opts.Harness ? Meta.NewProfile(d) : GodotDataSource.LoadProfile(d);
		_app = new App(this, d, profile, !opts.Harness, !opts.Harness, opts.Seed);
		GameInput.Injected += OnInjected;
		if (DebugRunner.TryStart(_app, opts)) return;
		_app.Flow.Title.Open();
	}

	public override void _UnhandledInput(InputEvent e)
	{
		if (Dispatch(GameInput.Translate(e))) GetViewport().SetInputAsHandled();
	}

	/// <summary>Akcja gracza (klawiatura, pad, mysz albo wirtualny kontroler) do ekranu bieżącego.</summary>
	private bool Dispatch(InputCmd cmd) => _app?.Flow.Current is not null && _app.Flow.Current.HandleInput(cmd);

	private void OnInjected(InputCmd cmd) => Dispatch(cmd);

	public override void _Process(double delta) => _app?.Flow.Current?.Process(delta);

	public override void _ExitTree()
	{
		GameInput.Injected -= OnInjected;
		Assets.ClearCache();
		Ui.ClearCache();
		PixelFont.Release();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
}
