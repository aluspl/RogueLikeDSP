namespace LifeLike.Core.Data;

/// <summary>
/// Węzeł drzewka Szkoleń (v0.21.52 cz. c,, core::tree_node): otwiera się po Depth poziomach pnia gałęzi (Branch),
/// pierwszy wybór 1 z 2 opcji kosztuje Cost dośw., zmiana – GameData.TreeRespecCost.
/// </summary>
public sealed record TreeNode(int Branch, int Depth, int Cost, TreeOption[] Options);
