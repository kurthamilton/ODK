namespace ODK.E2E.Data;

/// <summary>
/// The app's <c>EnvironmentType</c> numbers, which are what its <c>EnvironmentTypeId</c> columns store.
/// Repeated here rather than referenced because these tests deliberately do not depend on the app's
/// projects; the numbers are safe to repeat because they are a persisted contract, which is why that enum
/// assigns them explicitly rather than letting them fall where they may.
/// </summary>
public static class EnvironmentTypeIds
{
    public const int Dev = 2;

    public const int E2E = 3;

    public const int None = 0;

    public const int Prod = 1;

    /// <summary>The number for an environment's name as the app's enum spells it.</summary>
    public static int FromName(string name) => name switch
    {
        nameof(Dev) => Dev,
        nameof(E2E) => E2E,
        nameof(None) => None,
        nameof(Prod) => Prod,
        _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Unknown environment")
    };
}
