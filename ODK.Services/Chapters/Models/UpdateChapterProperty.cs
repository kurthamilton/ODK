namespace ODK.Services.Chapters.Models;

public class UpdateChapterProperty
{
    public string? DisplayName { get; set; }

    public string? HelpText { get; set; }

    public bool ApplicationOnly { get; set; }

    public string Label { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<string>? Options { get; set; }

    public bool Required { get; set; }

    public string? Subtitle { get; set; }
}
