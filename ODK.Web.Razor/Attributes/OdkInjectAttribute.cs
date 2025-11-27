namespace ODK.Web.Razor.Attributes;

/// <summary>
/// Use in PageModel classes, where the Razor [Inject] doesn't work
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public class OdkInjectAttribute : Attribute
{
}
