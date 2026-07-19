namespace ODK.Services.Members.Tasks;

public interface IMemberTaskService
{
    Task<IReadOnlyCollection<MemberTask>> GetOutstandingTasks(IMemberServiceRequest request);
}
