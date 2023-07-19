using System;
using System.Collections.Generic;
using System.Linq;

namespace ODK.Services.Files
{
    public class CsvFile
    {
        private readonly List<CsvRow> _rows = new List<CsvRow>();

        public CsvRow Header { get; } = new CsvRow();

        public IReadOnlyCollection<CsvRow> Rows => _rows.ToArray();

        public CsvRow AddRow()
        {
            CsvRow row = new CsvRow();
            _rows.Add(row);
            return row;
        }

        public void AddValue(string value)
        {
            _rows.Last().AddValue(value);
        }

        public void AddValues(IEnumerable<string> values)
        {
            _rows.Last().AddValues(values);
        }

        public IDictionary<string, int> GetColumnIndexes()
        {
            IDictionary<string, int> columnIndexes = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            for (int i = 0; i < Header.Values.Count; i++)
            {
                columnIndexes.Add(Header.Values.ElementAt(i), i);
            }

            return columnIndexes;
        }

        public override string ToString()
        {
            IEnumerable<CsvRow> rows = new[] { Header }.Union(Rows);
            return string.Join(Environment.NewLine, rows);
        }
    }
}
