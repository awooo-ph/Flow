using System;

    class DbColumn
    {
        public string Name { get; set; }
        private Type _type;

        public Type Type
        {
            get => _type;
            set
            {
                _type = value;
                if (_type == typeof(string) && DefaultValue == null)
                    DefaultValue = "\"\"";
            }
        }

        public string DbType { get; set; }
        private string _defaultValue;

        public string DefaultValue
        {
            get { return _defaultValue; }
            set
            {
                _defaultValue = value;
                if (_defaultValue == null && _type == typeof(string))
                    _defaultValue = "\"\"";
            }
        }

        public bool IsPrimaryKey { get; set; }
        public string Options { get; set; }
        public bool IsNullableEnum { get; set; }

        public object GetValue(object o)
        {
            if (o != null) return o;
            if (Type == typeof(string)) return string.Empty;
            if (Type == typeof(double)) return 0f;
            if (Type == typeof(long)) return 0L;
            return null;
        }
    }


    [AttributeUsage(AttributeTargets.Property)]
    sealed class PrimaryKeyAttribute : Attribute
    {

    }

[AttributeUsage(AttributeTargets.Property)]
sealed class NoCaseAttribute : Attribute
{

}

[AttributeUsage(AttributeTargets.Property)]
    sealed class DefaultValueAttribute : Attribute
    {
        public DefaultValueAttribute(string defaultValue)
        {
            DefaultValue = defaultValue;
        }

        public string DefaultValue { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    sealed class ColumnTypeAttribute : Attribute
    {
        public ColumnTypeAttribute(string dbType)
        {
            DbType = dbType;
        }
        public string DbType { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property)]
    sealed class IsNullableEnumAttribute : Attribute
    {
        
        public IsNullableEnumAttribute()
        {
            
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    sealed class ForeignKeyAttribute : Attribute
    {
        public ForeignKeyAttribute(string table)
        {
            Table = table;
        }

        public string Table { get; set; }
    }


    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreAttribute : Attribute { }
