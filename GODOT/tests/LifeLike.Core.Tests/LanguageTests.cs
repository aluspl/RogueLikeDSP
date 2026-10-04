using System.Text.Json;
using System.Text.RegularExpressions;

namespace LifeLike.Core.Tests;

/// <summary>
/// v0.21.53 cz. 2 (#40): język angielski – każdy tekst z danych i interfejsu ma tłumaczenie, szablony mają te same
/// {0}, {1}..., dane po angielsku wczytują się tak samo jak po polsku (te same liczby, odwołania po nazwie działają).
/// Kopie golden/game.json i golden/en.json z GBA/data (jak przy przebiegach złotych).
/// </summary>
public class LanguageTests
{
    private static string Game => File.ReadAllText(TestData.GoldenPath("game.json"));
    private static string En => File.ReadAllText(TestData.GoldenPath("en.json"));

    private static readonly object Gate = new();

    [Fact]
    public void EveryDataTextHasEnglish()
    {
        lock (Gate)
        {
            LangOverlay.Apply(Game, En);
            Assert.Empty(LangOverlay.Missing);
        }
    }

    [Fact]
    public void EveryUiKeyHasEnglishWithSamePlaceholders()
    {
        using var g = JsonDocument.Parse(Game);
        using var e = JsonDocument.Parse(En);
        foreach (var section in new[] { "ui", "uiGodot" })
        {
            var en = e.RootElement.GetProperty(section);
            foreach (var p in g.RootElement.GetProperty(section).EnumerateObject())
            {
                Assert.True(en.TryGetProperty(p.Name, out var t), $"{section}.{p.Name}: brak angielskiego tekstu");
                var pl = p.Value.GetString() ?? "";
                var tx = t.GetString() ?? "";
                Assert.Equal(Holes(pl), Holes(tx));
                var args = Enumerable.Range(0, 8).Select(i => (object)i).ToArray();
                _ = string.Format(pl, args); // poprawny szablon w obu językach
                _ = string.Format(tx, args);
            }
        }
    }

    private static string Holes(string s) => string.Join(",", Regex.Matches(s, @"\{\d+[^}]*\}").Select(m => m.Value).OrderBy(x => x));

    [Fact]
    public void EnglishDataLoadsWithSameNumbers()
    {
        lock (Gate)
        {
            var pl = GameData.Parse(Game, En, false);
            var en = GameData.Parse(Game, En, true);
            Loc.English = false;
            Assert.Equal(pl.Enemies.Length, en.Enemies.Length);
            Assert.Equal(pl.Enemies.Select(x => x.MaxHealth), en.Enemies.Select(x => x.MaxHealth));
            Assert.Equal(pl.Rewards.Select(x => x.Index), en.Rewards.Select(x => x.Index)); // nagroda „sprzęt” po nazwie slotu
            Assert.Equal(pl.ChoiceEvents.SelectMany(x => x.Choices).SelectMany(c => c.Outs).Select(o => o.Arg),
                en.ChoiceEvents.SelectMany(x => x.Choices).SelectMany(c => c.Outs).Select(o => o.Arg));
            Assert.Equal("Przeciek", pl.Enemies[pl.EnemyIndex("przeciek")].Name);
            Assert.Equal("Leak", en.Enemies[en.EnemyIndex("przeciek")].Name);
            Assert.Equal("The Unmissable Deadline", en.Enemies[en.EnemyIndex("termin")].Name);
            Assert.Equal("Semi", en.Career.First(c => c.Id == "blizniak").Short); // wyjątek z „context”
        }
    }

    [Fact]
    public void UiTextFollowsLanguage()
    {
        lock (Gate)
        {
            GameData.Parse(Game, En, false);
            Assert.Equal("Zapisz i wyjdź", Loc.T("zapisz_i_wyjdz"));
            Loc.English = true;
            Assert.Equal("Save and quit", Loc.T("zapisz_i_wyjdz"));
            Assert.Equal("Stage 3/12", Loc.F("etap_2", 3, 12));
            Loc.English = false;
            Assert.Equal("Etap 3/12", Loc.F("etap_2", 3, 12));
            Assert.Equal("nieznany_klucz", Loc.T("nieznany_klucz"));
        }
    }
}
