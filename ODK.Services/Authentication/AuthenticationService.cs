﻿using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Notifications;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;
using ODK.Services.Members;
using ODK.Services.Notifications;

namespace ODK.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMemberEmailService _memberEmailService;
    private readonly INotificationService _notificationService;
    private readonly AuthenticationServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUrlProvider _urlProvider;

    public AuthenticationService(
        AuthenticationServiceSettings settings,
        IAuthorizationService authorizationService, 
        IUnitOfWork unitOfWork,
        IMemberEmailService memberEmailService,
        IUrlProvider urlProvider,
        INotificationService notificationService)
    {
        _authorizationService = authorizationService; 
        _memberEmailService = memberEmailService;
        _notificationService = notificationService;
        _settings = settings;
        _unitOfWork = unitOfWork;
        _urlProvider = urlProvider;
    }    

    public async Task<ServiceResult> ActivateChapterAccountAsync(Guid chapterId, string activationToken, string password)
    {
        var token = await _unitOfWork.MemberActivationTokenRepository
            .GetByToken(activationToken)
            .Run();
        if (token == null)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        var (chapter, adminMembers, notificationSettings, member, memberPassword, chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetById(chapterId),
            x => x.ChapterAdminMemberRepository.GetByChapterId(chapterId),
            x => x.MemberNotificationSettingsRepository.GetByChapterId(chapterId, NotificationType.NewMember),
            x => x.MemberRepository.GetById(token.MemberId),
            x => x.MemberPasswordRepository.GetByMemberId(token.MemberId),
            x => x.ChapterPropertyRepository.GetByChapterId(chapterId),
            x => x.MemberPropertyRepository.GetByMemberId(token.MemberId, chapterId));

        OdkAssertions.MeetsCondition(token, x => x.ChapterId == chapterId);

        memberPassword = UpdatePassword(memberPassword, password);
        member.Activated = true;

        _unitOfWork.MemberRepository.Update(member);

        if (memberPassword.MemberId == default)
        {
            memberPassword.MemberId = member.Id;
            _unitOfWork.MemberPasswordRepository.Add(memberPassword);
        }
        else
        {
            _unitOfWork.MemberPasswordRepository.Update(memberPassword);
        }

        _unitOfWork.MemberActivationTokenRepository.Delete(token);

        _notificationService.AddNewMemberNotifications(member, chapter.Id, adminMembers, notificationSettings);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendNewMemberEmailsAsync(
            chapter, 
            adminMembers, 
            member, 
            chapterProperties, 
            memberProperties);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ActivateSiteAccountAsync(string activationToken, string password)
    {
        var token = await _unitOfWork.MemberActivationTokenRepository
            .GetByToken(activationToken)
            .Run();
        if (token == null || token.ChapterId != null)
        {
            return ServiceResult.Failure("The link you followed is no longer valid");
        }

        var (member, memberPassword) = await _unitOfWork.RunAsync(
            x => x.MemberRepository.GetById(token.MemberId),
            x => x.MemberPasswordRepository.GetByMemberId(token.MemberId));

        memberPassword = UpdatePassword(memberPassword, password);
        member.Activated = true;

        _unitOfWork.MemberRepository.Update(member);

        if (memberPassword.MemberId == default)
        {
            memberPassword.MemberId = member.Id;
            _unitOfWork.MemberPasswordRepository.Add(memberPassword);
        }
        else
        {
            _unitOfWork.MemberPasswordRepository.Update(memberPassword);
        }

        _unitOfWork.MemberActivationTokenRepository.Delete(token);

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendSiteWelcomeEmail(member);

        return ServiceResult.Successful();
    }

    public async Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword)
    {
        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(memberId)
            .Run();
        var matches = CheckPassword(memberPassword, currentPassword);
        if (!matches)
        {
            return ServiceResult.Failure("Current password is incorrect");
        }

        memberPassword = UpdatePassword(memberPassword, newPassword);
        _unitOfWork.MemberPasswordRepository.Update(memberPassword);

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    public async Task<Member?> GetMemberAsync(string username, string password)
    {
        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(username)
            .Run();
        if (member == null || !member.IsCurrent())
        {
            return null;
        }

        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(member.Id)
            .Run();

        bool passwordMatches = CheckPassword(memberPassword, password);
        return passwordMatches ? member : null;
    }

    public async Task<IReadOnlyCollection<Claim>> GetClaimsAsync(Member member)
    {
        var adminMembers = await _unitOfWork.ChapterAdminMemberRepository.GetByMemberId(member.Id).Run();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
            new Claim(ClaimTypes.Role, OdkRoles.Member)
        };

        if (adminMembers.Any())
        {
            claims.Add(new Claim(ClaimTypes.Role, OdkRoles.Admin));
        }

        if (member.SuperAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, OdkRoles.SuperAdmin));
        }

        return claims;
    }
    
    public async Task<ServiceResult> RequestPasswordResetAsync(Guid chapterId, string emailAddress)
    {
        var chapter = await _unitOfWork.ChapterRepository
            .GetById(chapterId)
            .Run();

        return await RequestPasswordResetAsync(chapter, emailAddress);
    }

    public async Task<ServiceResult> RequestPasswordResetAsync(string emailAddress)
    {
        return await RequestPasswordResetAsync(null, emailAddress);
    }

    public async Task<ServiceResult> ResetPasswordAsync(string token, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return ServiceResult.Failure("Password cannot be blank");
        }

        const string message = "Link is invalid or has expired. Please request a new link using the password reset form.";

        var request = await _unitOfWork.MemberPasswordResetRequestRepository
            .GetByToken(token)
            .Run();
        if (request == null)
        {
            return ServiceResult.Failure(message);
        }

        _unitOfWork.MemberPasswordResetRequestRepository.Delete(request);

        if (request.ExpiresUtc < DateTime.UtcNow)
        {
            await _unitOfWork.SaveChangesAsync();
            return ServiceResult.Failure(message);
        }

        var memberPassword = await _unitOfWork.MemberPasswordRepository
            .GetByMemberId(request.MemberId)
            .Run();

        memberPassword = UpdatePassword(memberPassword, password);
        
        if (memberPassword.MemberId == default)
        {
            memberPassword.MemberId = request.MemberId;
            _unitOfWork.MemberPasswordRepository.Add(memberPassword);
        }
        else
        {
            _unitOfWork.MemberPasswordRepository.Update(memberPassword);
        }

        await _unitOfWork.SaveChangesAsync();

        return ServiceResult.Successful();
    }

    private static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new OdkServiceException("Password cannot be blank");
        }
    }    

    private bool CheckPassword([NotNullWhen(true)] MemberPassword? memberPassword, string password)
    {
        if (memberPassword == null)
        {
            return false;
        }

        string passwordHash = PasswordHasher.ComputeHash(password, memberPassword.Salt);
        return memberPassword.Hash == passwordHash;
    }

    private async Task<ServiceResult> RequestPasswordResetAsync(Chapter? chapter, string emailAddress)
    {
        if (!MailUtils.ValidEmailAddress(emailAddress))
        {
            return ServiceResult.Failure("Invalid email address format");
        }

        var member = await _unitOfWork.MemberRepository
            .GetByEmailAddress(emailAddress)
            .Run();
        if (member == null)
        {
            // return fake success to avoid leaking valid email addresses
            return ServiceResult.Successful();
        }

        if (!member.Activated)
        {
            var activationToken = await _unitOfWork.MemberActivationTokenRepository.GetByMemberId(member.Id).Run();
            if (activationToken == null)
            {
                activationToken = new MemberActivationToken
                {
                    ActivationToken = RandomStringGenerator.Generate(64),
                    ChapterId = chapter?.Id,
                    MemberId = member.Id
                };
                _unitOfWork.MemberActivationTokenRepository.Add(activationToken);
                await _unitOfWork.SaveChangesAsync();
            }

            await _memberEmailService.SendActivationEmail(chapter, member, activationToken.ActivationToken);
            return ServiceResult.Successful();
        }

        var created = DateTime.UtcNow;
        var expires = created.AddMinutes(_settings.PasswordResetTokenLifetimeMinutes);
        var token = RandomStringGenerator.Generate(64);

        _unitOfWork.MemberPasswordResetRequestRepository.Add(new MemberPasswordResetRequest
        {
            CreatedUtc = created,
            ExpiresUtc = expires,
            MemberId = member.Id,
            Token = token
        });

        await _unitOfWork.SaveChangesAsync();

        await _memberEmailService.SendPasswordResetEmail(chapter, member, token);

        return ServiceResult.Successful();
    }    

    private MemberPassword UpdatePassword(MemberPassword? memberPassword, string password)
    {
        ValidatePassword(password);

        (string hash, string salt) = PasswordHasher.ComputeHash(password);

        if (memberPassword == null)
        {
            memberPassword = new MemberPassword();
        }

        memberPassword.Hash = hash;
        memberPassword.Salt = salt;

        return memberPassword;
    }
}
