namespace ODK.Web.Common.Components;

public class MenuItem
{
    public bool Active { get; set; }

    public bool ActiveIsExactMatch { get; set; }

    public string? ExternalLink { get; set; }

    public string? Icon { get; set; }

    public string? Link { get; set; }
    
    public string? Text { get; set; }
}
