namespace ODK.Web.Razor.Models;

public class OdkComponentViewModel<T> : OdkComponentViewModel
{
    public OdkComponentViewModel(OdkComponentContext context, T value) 
        : base(context)
    {
        Value = value;
    }

    public T Value { get; }
}
