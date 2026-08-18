using System.Text;

namespace ODK.Core.Workflows;

/// <summary>
/// Renders a definition as a markdown page: the diagram, plus the table of transitions the diagram
/// cannot show - the ordered steps behind each edge and what each one does.
/// </summary>
public static class MarkdownExporter
{
    public static string ToDocument<TState, TTrigger, TContext>(
        StateMachineDefinition<TState, TTrigger, TContext> definition)
        where TState : struct, Enum
        where TTrigger : struct, Enum
    {
        var builder = new StringBuilder();

        builder.Append("# ").Append(definition.Name).Append('\n').Append('\n');
        builder
            .Append("Generated from the state machine definition. Do not edit by hand - run the tests to ")
            .Append("regenerate it.")
            .Append('\n').Append('\n');

        builder.Append("```mermaid").Append('\n');
        builder.Append(MermaidExporter.ToStateDiagram(definition));
        builder.Append("```").Append('\n').Append('\n');

        builder.Append("## Transitions").Append('\n').Append('\n');
        builder.Append("| From | Trigger | To | When | Steps |").Append('\n');
        builder.Append("| --- | --- | --- | --- | --- |").Append('\n');

        foreach (var transition in definition.Transitions)
        {
            builder
                .Append("| ").Append(Cell(transition.From.ToString()))
                .Append(" | ").Append(Cell(transition.Trigger.ToString()))
                .Append(" | ").Append(Cell(transition.To.ToString()))
                .Append(" | ").Append(Cell(Guards(transition)))
                .Append(" | ").Append(Cell(Steps(transition)))
                .Append(" |").Append('\n');
        }

        return builder.ToString();
    }

    private static string Cell(string? value) => string.IsNullOrWhiteSpace(value)
        ? "-"
        : value.Replace("|", @"\|");

    private static string Guards<TState, TTrigger, TContext>(
        Transition<TState, TTrigger, TContext> transition)
        where TState : struct, Enum
        where TTrigger : struct, Enum
        => string.Join(", ", transition.Guards.Select(x => x.Description));

    private static string Steps<TState, TTrigger, TContext>(
        Transition<TState, TTrigger, TContext> transition)
        where TState : struct, Enum
        where TTrigger : struct, Enum
        => string.Join("<br>", transition.Steps.Select((x, i) => $"{i + 1}. {x.Description} ({x.Kind})"));
}
