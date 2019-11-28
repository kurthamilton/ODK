using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ODK.Umbraco
{
    public class ServiceResult
    {
        public ServiceResult(string propertyName, string errorMessage)
        {
            Errors = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
            {
                { propertyName, errorMessage }
            });
        }

        public ServiceResult(IDictionary<string, string> errors)
        {
            Errors = new ReadOnlyDictionary<string, string>(errors);
        }

        public ServiceResult(bool success, string errorMessage = null)
        {
            ErrorMessage = errorMessage;
            Success = success;
        }

        public string ErrorMessage { get; }

        public IReadOnlyDictionary<string, string> Errors { get; private set; } = new Dictionary<string, string>();

        public bool Success { get; }
    }
}
