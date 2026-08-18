namespace ODK.Core.Workflows;

/// <summary>Thrown when a definition is invalid, which is a programming error rather than a runtime one.</summary>
public sealed class StateMachineDefinitionException : Exception
{
    public StateMachineDefinitionException(string message)
        : base(message)
    {
    }
}
