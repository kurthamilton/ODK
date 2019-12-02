using System;
using System.Web;

namespace ODK.Umbraco.Web.Mvc
{
    public class RequestCacheItem<T> where T : class
    {
        private readonly Func<T> _getValue;

        public RequestCacheItem(string key, Func<T> getValue)
        {
            _getValue = getValue;
            Key = key;
        }

        public string Key { get; }

        public T Value => GetValue();

        private T GetValue()
        {
            object item = HttpContext.Current.Items[Key];
            if (item == null)
            {
                HttpContext.Current.Items[Key] = _getValue();
            }

            return HttpContext.Current.Items[Key] as T;
        }
    }
}
