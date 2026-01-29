namespace ODK.Web.Razor.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public sealed class SkipRequestStoreMiddlewareAttribute : Attribute 
{ 
}
