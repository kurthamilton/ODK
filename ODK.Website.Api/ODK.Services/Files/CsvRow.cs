using System.Collections.Generic;
using System.Linq;

namespace ODK.Services.Files
{
    public class CsvRow
    {
        private readonly List<string> _values = new List<string>();

        public IReadOnlyCollection<string> Values => _values.ToArray();

        public void AddValue(string value)
        {
            _values.Add(value);
        }

        public void AddValues(IEnumerable<string> values)
        {
            _values.AddRange(values);
        }

        public override string ToString()
        {
            return string.Join(",", _values.Select(x => x.Contains(" ") ? $"\"{x}\"" : x));
        }

        public string Value(int index)
        {
            return Values.ElementAt(index);
        }
    }
}
