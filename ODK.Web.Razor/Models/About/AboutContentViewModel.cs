using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.About
{
    public class AboutContentViewModel
    {
        public AboutContentViewModel(Chapter chapter)
        {
            Chapter = chapter;
        }

        public Chapter Chapter { get; }
    }
}
