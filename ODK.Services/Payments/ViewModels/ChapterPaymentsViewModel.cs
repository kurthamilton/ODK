using ODK.Core.Chapters;

namespace ODK.Services.Payments.ViewModels;

public class ChapterPaymentsViewModel
{
    public required Chapter Chapter { get; init; }

    /// <summary>
    /// Whether the group has a payment account it can take money through. False while onboarding is
    /// unfinished as well as before it starts, since neither state can be charged against.
    /// </summary>
    public required bool PaymentAccountEnabled { get; init; }

    public required IReadOnlyCollection<ChapterPaymentItemViewModel> Payments { get; init; }

    /// <summary>
    /// Whether the member viewing the page is a site admin. Recording a refund is a site-admin act, and
    /// this is a group's own page - so the action it unlocks is on the row for the one reader who can use
    /// it, and absent for the group's own admins.
    /// </summary>
    public required bool ViewedBySiteAdmin { get; init; }
}