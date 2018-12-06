using System;

    public class ExcludeAttribute : Attribute
    {
        
    }

public class RequiredAttribute : Attribute { }
public class UniqueAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FieldAttribute : Attribute {
        public FieldAttribute(string fieldName)
        {
            InitAttribute(fieldName, null, false);
        }

        public FieldAttribute(string fieldName = null, bool exclude = false)
        {
            InitAttribute(fieldName, null, exclude);
        }

        public FieldAttribute(string fieldName, object defaultValue, bool exclude)
        {
            InitAttribute(fieldName,defaultValue,exclude);
        }

        private void InitAttribute(string fieldName, object defaultValue, bool exclude)
        {
            if (fieldName == null) fieldName = "";
            FieldName =  fieldName.Replace(" ",string.Empty);
            DefaultValue = defaultValue;
            Exclude = exclude;
        }
        
        public string FieldName { get; private set; }
        public object DefaultValue { get; set; }
        public bool Exclude { get; set; }
        public Type DataType { get; set; }
        public object DefaultDbValue { get; set; }
        public bool CanBeNull { get; set; }
        public bool IgnoreChanged { get; set; }
        
    }
