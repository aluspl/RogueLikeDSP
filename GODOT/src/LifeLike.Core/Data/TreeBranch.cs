namespace LifeLike.Core.Data;

/// <summary>Gałąź drzewka Szkoleń (core::tree_branch): pień = Szkolenia z maski Upgrades (bity GameData.Upgrades).</summary>
public sealed record TreeBranch(string Id, string Name, int Upgrades);
