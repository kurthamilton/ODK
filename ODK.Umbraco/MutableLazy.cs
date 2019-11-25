using System;

namespace ODK.Umbraco
{
    public class MutableLazy<T>
    {
        private Lazy<T> _lazy;

        private T _value;

        public MutableLazy(Func<T> valueFactory)
        {
            _lazy = valueFactory != null ? new Lazy<T>(valueFactory) : null;
        }

        public T Value
        {
            get
            {
                if (_lazy != null)
                {
                    _value = _lazy.Value;
                }

                return _value;
            }

            set
            {
                _lazy = null;
                _value = value;
            }
        }
    }
}
