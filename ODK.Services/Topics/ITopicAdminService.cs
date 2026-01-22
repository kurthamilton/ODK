using ODK.Services.Topics.Models;
using ODK.Services.Topics.ViewModels;

namespace ODK.Services.Topics;

public interface ITopicAdminService
{
    Task<ServiceResult> AddTopic(Guid currentMemberId, Guid topicGroupId, string name);

    Task<ServiceResult> AddTopicGroup(Guid currentMemberId, string name);

    Task ApproveTopics(MemberServiceRequest request, ApproveTopicsModel approved, ApproveTopicsModel rejected);

    Task<TopicAdminPageViewModel> GetTopicAdminPageViewModel(Guid currentMemberId, Guid topicId);

    Task<TopicsAdminPageViewModel> GetTopicsAdminPageViewModel(Guid currentMemberId);

    Task<ServiceResult> UpdateTopic(MemberServiceRequest request, Guid topicId, UpdateTopicModel model);
}