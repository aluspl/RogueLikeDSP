# LifeLike – Godot 4 + C#

Port gry LifeLike (Daj Się Poznać 2017, oryginał w Unity) na Godot 4 .NET.
Pełna dokumentacja (research, architektura, format danych, roadmapa, ADR) zostanie dodana w `docs/`.
Kierunek rozwoju: [`docs/KONCEPCJA.md`](docs/KONCEPCJA.md) – roguelike budowlany z telefonem (aplikacja PlanBudowlany) jako interfejsem.

## Wymagania
- Godot 4.7 w wersji .NET
- .NET 8 SDK

## Uruchomienie
1. W Godot wybierz *Import* i wskaż `GODOT/godot/project.godot`.
2. Uruchom scenę główną (`scenes/Main.tscn`).

Test dymny bez okna: `godot --headless --path GODOT/godot -- --smoke`

## Testy
```bash
dotnet test GODOT/LifeLike.sln
```

## Struktura
```
src/LifeLike.Core/          logika bez zależności od Godota (TileGrid, komendy, tury, FSM, generator lochu)
tests/LifeLike.Core.Tests/  testy xUnit
godot/                      projekt Godota (Main.cs, input, render 2D, HUD)
godot/data/                 dane gry w JSON (klasy, bronie)
```
