namespace ODK.Core.SocialMedia;

public class InstagramPost
{
    public InstagramPost(Guid id, Guid chapterId, string externalId, DateTime date, 
        string caption, string url)
    {
        Caption = caption;
        ChapterId = chapterId;
        Date = date;
        ExternalId = externalId;
        Id = id;
        Url = url;
    }

    public string Caption { get; }

    public Guid ChapterId { get; }

    public DateTime Date { get; }

    public string ExternalId { get; }

    public Guid Id { get; }

    public string Url { get; }
}
