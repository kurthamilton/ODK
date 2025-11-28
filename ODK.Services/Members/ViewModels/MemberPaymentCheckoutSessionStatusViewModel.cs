namespace ODK.Services.Members.ViewModels;

public class MemberPaymentCheckoutSessionStatusViewModel
{
    public required bool Complete { get; init; }

    public required bool Expired { get; init; }

    public required bool PaymentReceived { get; init; }
}
