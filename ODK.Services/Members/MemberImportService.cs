using ODK.Core.Members;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Members.Models;
using ODK.Services.Members.ViewModels;

namespace ODK.Services.Members;

public class MemberImportService : OdkAdminServiceBase, IMemberImportService
{
    private readonly IEmailValidationService _emailValidationService;
    private readonly MemberImportServiceSettings _settings;
    private readonly SiteSubscriptionCooldown _siteSubscriptionCooldown;
    private readonly IUnitOfWork _unitOfWork;

    public MemberImportService(
        IUnitOfWork unitOfWork,
        IEmailValidationService emailValidationService,
        SiteSubscriptionCooldown siteSubscriptionCooldown,
        MemberImportServiceSettings settings)
        : base(unitOfWork)
    {
        _emailValidationService = emailValidationService;
        _settings = settings;
        _siteSubscriptionCooldown = siteSubscriptionCooldown;
        _unitOfWork = unitOfWork;
    }

    public async Task<ServiceResult> ClearStagedMembers(IMemberChapterAdminServiceRequest request)
    {
        var staged = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberChapterImportRepository
                .Query(y => y.InChapter(request.Chapter.Id))
                .GetAll());

        if (staged.Count == 0)
        {
            return ServiceResult.Failure("There is nobody waiting to be invited");
        }

        _unitOfWork.MemberChapterImportRepository.DeleteMany(staged);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> DeleteStagedMember(IMemberChapterAdminServiceRequest request, Guid id)
    {
        var staged = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberChapterImportRepository.GetByIdOrDefault(id));

        // Checked rather than assumed: an id from another group's page would otherwise delete its row.
        if (staged == null || staged.ChapterId != request.Chapter.Id)
        {
            return ServiceResult.Failure("Person not found");
        }

        _unitOfWork.MemberChapterImportRepository.Delete(staged);

        await _unitOfWork.SaveChanges();

        return ServiceResult.Successful();
    }

    public async Task<MemberImportAdminPageViewModel> GetMemberImportViewModel(
        IMemberChapterAdminServiceRequest request)
    {
        var (platform, chapter) = (request.Platform, request.Chapter);

        var (staged, memberCount, outstandingInvites, ownerSubscription) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberChapterImportRepository
                .Query(y => y.InChapter(chapter.Id))
                .GetAll(),
            x => x.MemberRepository.GetCountByChapterId(chapter.Id),
            x => x.MemberChapterInviteRepository.GetByChapterId(chapter.Id),
            x => x.MemberSiteSubscriptionRecordRepository
                .Query(y => y.Current().ForChapterOwner(chapter.Id).Active(_siteSubscriptionCooldown))
                .SiteSubscription()
                .GetSingleOrDefault());

        /* A second round-trip, because which accounts to look up is not known until the held rows are. It
           is one query for the whole page rather than one per row. */
        var existingMembers = await _unitOfWork.MemberRepository
            .Query()
            .HasEmailAddress(staged.Select(x => x.EmailAddress).ToArray())
            .GetAll()
            .Run();

        var validity = await ValidateEmailAddresses(staged.Select(x => x.EmailAddress));
        var classifier = MemberImportClassifier.Create(chapter.Id, existingMembers, outstandingInvites);
        var utcNow = DateTime.UtcNow;

        return new MemberImportAdminPageViewModel
        {
            Capacity = new MemberImportCapacity
            {
                MemberCount = memberCount,
                OutstandingInviteCount = outstandingInvites.Count,
                OwnerSubscription = ownerSubscription
            },
            Chapter = chapter,
            Platform = platform,
            RetentionDays = _settings.RetentionDays,
            Rows = staged
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .Select(x => ToRowViewModel(
                    x, classifier.Classify(x.EmailAddress, validity[x.EmailAddress]), utcNow))
                .ToArray()
        };
    }

    public async Task<IReadOnlyCollection<IReadOnlyCollection<string>>> GetMemberImportTemplate(
        IMemberChapterAdminServiceRequest request)
    {
        await AssertMemberIsChapterAdmin(request);

        return [MemberImportCsvRow.GetCsvHeaderRow()];
    }

    public async Task<int> PurgeExpiredImports()
    {
        /* Measured from when the address arrived rather than from the last upload that carried it, so
           re-uploading the same file cannot hold an address indefinitely. The invite raised from a row
           inherits the same instant, so the two purges are one retention period. */
        var createdBeforeUtc = DateTime.UtcNow.AddDays(-_settings.RetentionDays);

        var expired = await _unitOfWork.MemberChapterImportRepository
            .Query(x => x.CreatedBefore(createdBeforeUtc))
            .GetAll()
            .Run();

        if (expired.Count == 0)
        {
            return 0;
        }

        _unitOfWork.MemberChapterImportRepository.DeleteMany(expired);

        await _unitOfWork.SaveChanges();

        return expired.Count;
    }

    public async Task<StageMemberImportResult> StageMembers(
        IMemberChapterAdminServiceRequest request,
        IReadOnlyCollection<MemberImportCsvRow> rows,
        string? sourceFileName)
    {
        var chapter = request.Chapter;

        // The file's own duplicates collapse here: a group holds one row per address.
        var distinct = rows
            .Where(x => !string.IsNullOrWhiteSpace(x.EmailAddress))
            .GroupBy(x => x.EmailAddress, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.First())
            .ToArray();

        if (distinct.Length == 0)
        {
            return StageMemberImportResult.Failure(
                "The uploaded file did not contain any rows with an email address");
        }

        var emailAddresses = distinct
            .Select(x => x.EmailAddress)
            .ToArray();

        var (existingMembers, outstandingInvites, staged) = await GetChapterAdminRestrictedContent(
            request,
            x => x.MemberRepository
                .Query()
                .HasEmailAddress(emailAddresses)
                .GetAll(),
            x => x.MemberChapterInviteRepository.GetByChapterId(chapter.Id),
            x => x.MemberChapterImportRepository
                .Query(y => y.InChapter(chapter.Id))
                .GetAll());

        var validity = await ValidateEmailAddresses(emailAddresses);
        var classifier = MemberImportClassifier.Create(chapter.Id, existingMembers, outstandingInvites);

        var stagedByEmailAddress = staged
            .ToDictionary(x => x.EmailAddress, StringComparer.OrdinalIgnoreCase);

        var utcNow = DateTime.UtcNow;
        var statuses = new List<MemberImportRowStatus>(distinct.Length);
        var updated = 0;

        foreach (var row in distinct)
        {
            var status = classifier.Classify(row.EmailAddress, validity[row.EmailAddress]);
            statuses.Add(status);

            /* Nothing to hold for a member the group already has or has already asked: the row would show
               as needing no action and never clear. An unusable address is held, because correcting it is
               an action. */
            if (status is MemberImportRowStatus.ExistingInGroup or MemberImportRowStatus.AlreadyInvited)
            {
                continue;
            }

            if (stagedByEmailAddress.TryGetValue(row.EmailAddress, out var existing))
            {
                existing.FirstName = row.FirstName;
                existing.LastName = row.LastName;
                existing.SourceFileName = sourceFileName;
                existing.UploadedUtc = utcNow;

                // CreatedUtc is left alone: it is the retention clock, so a later upload cannot extend it.
                _unitOfWork.MemberChapterImportRepository.Update(existing);
                updated++;
                continue;
            }

            _unitOfWork.MemberChapterImportRepository.Add(new MemberChapterImport
            {
                ChapterId = chapter.Id,
                CreatedUtc = utcNow,
                EmailAddress = row.EmailAddress,
                FirstName = row.FirstName,
                LastName = row.LastName,
                SourceFileName = sourceFileName,
                UploadedUtc = utcNow
            });
        }

        await _unitOfWork.SaveChanges();

        return StageMemberImportResult.Recorded(statuses, updated);
    }

    private MemberImportRowViewModel ToRowViewModel(
        MemberChapterImport staged, MemberImportRowStatus status, DateTime utcNow)
    {
        var deletedUtc = staged.CreatedUtc.AddDays(_settings.RetentionDays);
        var remaining = deletedUtc - utcNow;

        return new MemberImportRowViewModel
        {
            // Rounded up, so a row with hours left reads as a day rather than as due.
            DaysRemaining = remaining > TimeSpan.Zero
                ? (int)Math.Ceiling(remaining.TotalDays)
                : 0,
            DeletedUtc = deletedUtc,
            EmailAddress = staged.EmailAddress,
            FirstName = staged.FirstName,
            Id = staged.Id,
            LastName = staged.LastName,
            SourceFileName = staged.SourceFileName,
            Status = status,
            UploadedUtc = staged.UploadedUtc
        };
    }

    /// <summary>
    /// Whether each address is a usable format, keyed by the address. Format only: a file can hold hundreds
    /// of rows, and a deliverability check per row would spend the daily quota on one upload.
    /// </summary>
    private async Task<IReadOnlyDictionary<string, bool>> ValidateEmailAddresses(
        IEnumerable<string> emailAddresses)
    {
        var validity = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        foreach (var emailAddress in emailAddresses)
        {
            if (validity.ContainsKey(emailAddress))
            {
                continue;
            }

            var result = await _emailValidationService.Validate(emailAddress, EmailValidationLevel.Soft);
            validity[emailAddress] = result.Success;
        }

        return validity;
    }
}
