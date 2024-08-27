namespace ODK.Web.Razor.Pages.My.Groups;

public class GroupModel : OdkPageModel
{
    public Guid ChapterId { get; set; }

    public void OnGet(Guid id)
    {
        ChapterId = id;
    }
}
