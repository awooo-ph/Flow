using System;


    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class TableAttribute : Attribute
    {
        public string TableName { get; }
        public string CreateSql { get; set; }

        public TableAttribute(string name)
        {
            TableName = name;
        }
    }

