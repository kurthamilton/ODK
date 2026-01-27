namespace ODK.Data.Core.Deferred;

public class DefaultDeferredQueryAny : IDeferredQuery<bool>
{
    private readonly bool _value;

    public DefaultDeferredQueryAny(bool value)
    {
        _value = value;
    }

    public async Task<bool> Run() => await Task.FromResult(_value);
}