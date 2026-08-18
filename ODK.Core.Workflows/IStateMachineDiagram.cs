namespace ODK.Core.Workflows;

/// <summary>
/// The one deliberately non-generic view of a machine, so a registry can hold machines with
/// different state and context types. It exists for that reason alone and is not a second model of
/// a definition.
/// </summary>
public interface IStateMachineDiagram
{
    string Name { get; }

    string ToMermaid();
}
