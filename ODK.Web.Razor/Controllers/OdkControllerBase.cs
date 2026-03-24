using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Utils;
using ODK.Services;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Extensions;
using ODK.Web.Razor.Models.Feedback;

namespace ODK.Web.Razor.Controllers;

public abstract class OdkControllerBase : Controller
{
    protected OdkControllerBase(
        IRequestStore requestStore,
        IOdkRoutes odkRoutes)
    {
        RequestStore = requestStore;
        OdkRoutes = odkRoutes;
    }

    protected Chapter Chapter => RequestStore.Chapter;

    protected IChapterServiceRequest ChapterServiceRequest => RequestStore.ChapterServiceRequest;

    protected Member CurrentMember => RequestStore.CurrentMember;

    protected IMemberChapterServiceRequest MemberChapterServiceRequest => RequestStore.MemberChapterServiceRequest;

    protected IMemberServiceRequest MemberServiceRequest => RequestStore.MemberServiceRequest;

    protected IOdkRoutes OdkRoutes { get; }

    protected PlatformType Platform => RequestStore.Platform;

    protected IRequestStore RequestStore { get; }

    protected IServiceRequest ServiceRequest => RequestStore.ServiceRequest;

    protected void AddFeedback(string message, FeedbackType type)
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

    protected void AddFeedback(FeedbackViewModel viewModel)
    {
        TempData!.AddFeedback(viewModel);
    }

    protected IActionResult CacheableFile(byte[] data, string mimeType, int? version)
    {
        // Do not set cache control if no version was given for the image
        if (version != null)
        {
            Response.Headers.CacheControl = "public, max-age=31536000, immutable";
        }

        return File(data, mimeType);
    }

    protected IActionResult DownloadCsv(IReadOnlyCollection<IReadOnlyCollection<string>> data, string fileName)
    {
        var csv = StringUtils.ToCsv(data);
        return File(Encoding.UTF8.GetBytes(csv), "text/csv", fileName);
    }

    protected string? GetHeader(string name)
        => Request.Headers
            .GetCommaSeparatedValues(name)
            .FirstOrDefault();

    protected async Task<string> ReadBodyText()
    {
        using var reader = new StreamReader(Request.Body);
        var text = await reader.ReadToEndAsync();
        return text;
    }

    protected ServiceResult<IReadOnlyCollection<T>> ReadCsv<T>(IFormFile file)
    {
        if (!ValidateCsvFile(file, out var error))
        {
            return ServiceResult<IReadOnlyCollection<T>>.Failure(error);
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            MissingFieldFound = null,
            HeaderValidated = null
        };

        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        using var csv = new CsvReader(reader, config);

        var records = csv.GetRecords<T>().ToList();
        return ServiceResult<IReadOnlyCollection<T>>.Successful(records);
    }

    protected IActionResult RedirectToReferrer(string? fallback = null)
    {
        var url = Request.Headers["Referer"].ToString();
        if (string.IsNullOrEmpty(url))
        {
            url = !string.IsNullOrEmpty(fallback)
                ? fallback
                : Request.Path;
        }

        return Redirect(url);
    }

    private bool ValidateCsvFile(IFormFile? file, out string error)
    {
        error = string.Empty;

        if (file is null || file.Length == 0)
        {
            error = "No file uploaded";
            return false;
        }

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".csv")
        {
            error = "Only .csv files are allowed.";
            return false;
        }

        var allowedMimeTypes = new[] { "text/csv", "application/vnd.ms-excel", "text/plain" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
        {
            error = $"Invalid content type: {file.ContentType}";
            return false;
        }

        return true;
    }
}