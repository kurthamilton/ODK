namespace ODK.Services.Members.Tasks;

/// <summary>
/// Produces outstanding <see cref="MemberTask"/>s for a member from the pre-loaded
/// <see cref="MemberTaskContext"/>. Add a task type by adding a provider (and registering it in DI).
/// </summary>
public interface IMemberTaskProvider
{
    IReadOnlyCollection<MemberTask> GetTasks(MemberTaskContext context);
}
