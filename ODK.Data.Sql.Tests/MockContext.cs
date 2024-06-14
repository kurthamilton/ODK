using ODK.Data.Sql.Mapping;

namespace ODK.Data.Sql.Tests;

public class MockContext : SqlContext
{
    public MockContext()
        : base("")
    {
    }

    public void AddMockMap<T>(SqlMap<T> map)
    {
        AddMap(map);
    }
}
