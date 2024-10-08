﻿using Microsoft.AspNetCore.Mvc.RazorPages;
using ODK.Core.Countries;
using ODK.Services;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Feedback;

namespace ODK.Web.Razor.Pages;

public abstract class OdkPageModel : PageModel
{    
    public Guid CurrentMemberId => User.MemberId();

    public Guid? CurrentMemberIdOrDefault => User.MemberIdOrDefault();

    public string? Description
    {
        get => ViewData["Description"] as string;
        set => ViewData["Description"] = value;
    }

    public ILocation? Location
    {
        get => ViewData["Location"] as ILocation;
        set => ViewData["Location"] = value;
    }

    public IReadOnlyCollection<string>? Keywords
    {
        get => ViewData["Keywords"] as IReadOnlyCollection<string>;
        set => ViewData["Keywords"] = value;
    }

    public string? Path
    {
        get => ViewData["Path"] as string;
        set => ViewData["Path"] = value;
    }

    public string? Title
    {
        get => ViewData["Title"] as string;
        set => ViewData["Title"] = value;
    }

    protected void AddFeedback(FeedbackViewModel viewModel) => TempData!.AddFeedback(viewModel);

    protected void AddFeedback(string message, FeedbackType type = FeedbackType.Success)
        => AddFeedback(new FeedbackViewModel(message, type));

    protected void AddFeedback(ServiceResult result) 
        => AddFeedback(new FeedbackViewModel(result));

    protected void AddFeedback(ServiceResult result, string successMessage)
    {
        if (result.Success)
        {
            var message = !string.IsNullOrEmpty(result.Message) ? result.Message : successMessage;
            AddFeedback(message, FeedbackType.Success);
        }
        else
        {
            AddFeedback(result);
        }
    }
}
