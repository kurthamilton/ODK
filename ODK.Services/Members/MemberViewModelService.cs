using ODK.Core;
using ODK.Data.Core;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public class MemberViewModelService : IMemberViewModelService
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberViewModelService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<MemberPageViewModel> GetMemberPage(Guid currentMemberId, string chapterName, Guid memberId)
    {
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        var (currentMember, members, chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(currentMemberId),
            x => x.MemberRepository.GetByChapterId(chapter.Id, [currentMemberId, memberId]),
            x => x.ChapterPropertyRepository.GetByChapterId(chapter.Id),
            x => x.MemberPropertyRepository.GetByMemberId(memberId, chapter.Id));
        
        var member = members.FirstOrDefault(x => x.Id == memberId);

        OdkAssertions.MemberOf(currentMember, chapter.Id);
        OdkAssertions.Exists(member);

        return new MemberPageViewModel
        {
            Chapter = chapter,
            ChapterProperties = chapterProperties,
            CurrentMember = currentMember,
            Member = member,
            MemberProperties = memberProperties
        };
    }

    public async Task<MembersPageViewModel> GetMembersPage(Guid currentMemberId, string chapterName)
    {        
        var chapter = await _unitOfWork.ChapterRepository.GetByName(chapterName).RunAsync();
        OdkAssertions.Exists(chapter);

        // get current member separately as they might be hidden from the list of members
        var (members, currentMember) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetByChapterId(chapter.Id),
            x => x.MemberRepository.GetById(currentMemberId));
        
        OdkAssertions.MemberOf(currentMember, chapter.Id);

        return new MembersPageViewModel
        {
            ChapterName = chapter.Name,
            Members = members
        };
    }
}
