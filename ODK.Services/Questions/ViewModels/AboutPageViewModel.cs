namespace ODK.Services.Questions.ViewModels;

public class AboutPageViewModel
{
    public required IReadOnlyCollection<AboutPageQuestionViewModel> Questions { get; init; }
}
