using System.Text;

namespace ODK.Core.Workflows;

/// <summary>
/// Renders a definition as a Mermaid state diagram. It walks the definition rather than anything the
/// runner does, so the diagram cannot describe a machine other than the one that executes.
/// </summary>
public static class MermaidExporter
{
    public static string ToStateDiagram<TState, TTrigger, TContext>(
        StateMachineDefinition<TState, TTrigger, TContext> definition)
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        // Newlines are written explicitly: the output is compared against a committed file, which would
        // otherwise differ between a developer's machine and the build.
        var builder = new StringBuilder();

        builder.Append("stateDiagram-v2").Append('\n');
        builder.Append("    [*] --> ").Append(definition.InitialState).Append('\n');

        foreach (var transition in definition.Transitions)
        {
            builder
                .Append("    ")
                .Append(transition.From)
                .Append(" --> ")
                .Append(transition.To)
                .Append(": ")
                .Append(transition.Label())
                .Append('\n');
        }

        return builder.ToString();
    }
}
