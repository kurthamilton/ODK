using System;

namespace ODK.Core.DataTypes
{
    public class DataType
    {
        public DataType(Guid id, string name)
        {
            Id = id;
            Name = name;
        }        

        public Guid Id { get; }

        public string Name { get; }
    }
}
