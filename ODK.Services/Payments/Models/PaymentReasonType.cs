namespace ODK.Services.Payments.Models;

public enum PaymentReasonType
{
    None = 0,
    SiteSubscription = 1,
    ChapterSubscription = 2,
    EventTicket = 3,
    EventTicketDeposit = 4,
    EventTicketRemainder = 5
}