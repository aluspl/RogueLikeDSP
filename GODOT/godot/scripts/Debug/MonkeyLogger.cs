using System.Collections.Concurrent;
using Godot;

namespace LifeLike.Game.Debug;

/// <summary>Zbiera błędy z logu Godota (wyjątki C# w wywołaniach zwrotnych, PushError) dla testu małpy.</summary>
public partial class MonkeyLogger : Logger
{
    public readonly ConcurrentQueue<string> Errors = new();

    public override void _LogError(string function, string file, int line, string code, string rationale, bool editorNotify,
        int errorType, Godot.Collections.Array<ScriptBacktrace> scriptBacktraces)
    {
        if (errorType == 1) return; // ostrzeżenia nie przerywają testu
        Errors.Enqueue($"{(rationale.Length > 0 ? rationale : code)} ({file}:{line} {function})");
    }
}
