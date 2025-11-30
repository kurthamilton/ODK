using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Reflection;

namespace ODK.Web.Razor.Services;

/// <summary>
/// This class allows a custom attribute to act as a dependency injector in PageModel classes
/// </summary>
public class InjectingPageModelActivatorProvider<T> : IPageModelActivatorProvider
{
    private readonly IServiceProvider _rootProvider;
    private IPageModelActivatorProvider? _defaultProvider;
    private readonly object _lock = new();

    public InjectingPageModelActivatorProvider(IServiceProvider rootProvider)
    {
        _rootProvider = rootProvider;
    }

    public Func<PageContext, object> CreateActivator(CompiledPageActionDescriptor descriptor)
    {
        EnsureDefaultProviderResolved();

        var innerActivator = _defaultProvider!.CreateActivator(descriptor);
        if (innerActivator == null) return _ => new object();

        return context =>
        {
            var model = innerActivator(context);
            InjectProperties(model, context.HttpContext.RequestServices);
            return model;
        };
    }

    public Action<PageContext, object>? CreateReleaser(CompiledPageActionDescriptor descriptor)
    {
        EnsureDefaultProviderResolved();
        return _defaultProvider!.CreateReleaser(descriptor);
    }

    private void EnsureDefaultProviderResolved()
    {
        if (_defaultProvider != null)
        {
            return;
        }

        lock (_lock)
        {
            if (_defaultProvider != null)
            {
                return;
            }

            var providers = _rootProvider.GetServices<IPageModelActivatorProvider>().ToArray();

            _defaultProvider = providers
                .FirstOrDefault(p => p.GetType() != typeof(InjectingPageModelActivatorProvider<>));

            if (_defaultProvider == null)
            {
                throw new InvalidOperationException("Default IPageModelActivatorProvider not found.");
            }                
        }
    }

    private static void InjectProperties(object instance, IServiceProvider scoped)
    {
        var props = instance
            .GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p =>
                p.CanWrite &&
                p.IsDefined(typeof(T), inherit: true));

        foreach (var prop in props)
        {
            var value = scoped.GetService(prop.PropertyType);
            if (value != null)
            {
                prop.SetValue(instance, value);
            }
        }
    }
}