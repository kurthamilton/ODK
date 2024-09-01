namespace ODK.Services.Chapters;

public class UpdateChapterProperty
{
    public string? DisplayName { get; set; }

    public string? HelpText { get; set; }

    public bool ApplicationOnly { get; set; }

    public string Label { get; set; } = "";

    public string Name { get; set; } = "";

    public IReadOnlyCollection<string> Options { get; set; } = [];

    public bool Required { get; set; }

    public string? Subtitle { get; set; }
}
