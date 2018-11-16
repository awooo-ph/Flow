using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

    internal class DbTable
    {
        public string Name { get; }

        private DbColumn _primaryKey;
        public DbColumn PrimaryKey => _primaryKey;

        private List<DbColumn> _columns;

        public List<DbColumn> Columns
        {
            get
            {
                if (_columns != null) return _columns;
                _columns = new List<DbColumn>();

                foreach (var member in _type.GetProperties())
                {
                    try
                    {
                        if (member.IsDefined(typeof(IgnoreAttribute), true)) continue;
                        if (!member.CanWrite) continue;
                        var pk = member.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).FirstOrDefault();
                        var def =
                            member.GetCustomAttributes(typeof(DefaultValueAttribute), true).FirstOrDefault() as
                                DefaultValueAttribute;
                        var opt =
                            member.GetCustomAttributes(typeof(NoCaseAttribute), true).FirstOrDefault() as
                                NoCaseAttribute;
                        var columnType =
                            member.GetCustomAttributes(typeof(ColumnTypeAttribute), true).FirstOrDefault() as
                                ColumnTypeAttribute;
                        
                        
                        
                        var col = new DbColumn()
                        {
                            Name = member.Name,
                            Type = member.PropertyType,
                            DbType = columnType == null ? GetDbType(member.PropertyType) : columnType.DbType,
                            IsPrimaryKey = pk != null,
                            DefaultValue = def?.DefaultValue,
                            Options = opt == null ? "" : "COLLATE NOCASE",//$" REFERENCES {opt.Table}([Id]) ON DELETE CASCADE"
                        };

                        _columns.Add(col);
                        if (col.IsPrimaryKey)
                            _primaryKey = col;
                    }
                    catch (Exception e)
                    {
                        Debug.Print($"AWOOO: {e.Message}");
                    }
                    
                }
                return _columns;
            }
        }

        private string _createSql;

        public string CreateSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_createSql)) return _createSql;
                _createSql = GetCreateSql();
                return _createSql;
            }
        }

        private string _insertSql;

        public string InsertSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_insertSql)) return _insertSql;
                _insertSql = GenerateInsertSql();
                return _insertSql;
            }
        }

        private string _updateSql;

        public string UpdateSql
        {
            get
            {
                if (!string.IsNullOrEmpty(_updateSql)) return _updateSql;
                _updateSql = GenerateUpdateSql();
                return _updateSql;
            }
        }

        private string GenerateUpdateSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine($"UPDATE [{Name}] SET ");
            DbColumn pk = null;
            foreach (var dbColumn in Columns)
            {
                if (dbColumn.IsPrimaryKey)
                    pk = dbColumn;
                else
                    sql.Append($"[{dbColumn.Name}]=@{dbColumn.Name},");
            }
            if (pk == null)
                throw new Exception("No Primary Key");
            sql.Remove(sql.ToString().LastIndexOf(","), 1);
            sql.AppendLine($" WHERE [{pk.Name}]=@{pk.Name};");
            return sql.ToString();
        }

        private string GenerateInsertSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine($"INSERT INTO [{Name}] (");
            var values = new StringBuilder();
            values.AppendLine("VALUES (");
            foreach (var dbColumn in Columns.Where(x => !x.IsPrimaryKey))
            {
                sql.Append($"[{dbColumn.Name}],");
                values.AppendLine($"@{dbColumn.Name},");
            }
            sql.Remove(sql.ToString().LastIndexOf(","), 1);
            sql.AppendLine(") ");
            sql.Append(values.Remove(values.ToString().LastIndexOf(","), 1));
            sql.Append("); SELECT last_insert_rowid();");
            return sql.ToString();
        }

        private Type _type;

        public DbTable(Type type, string name = null)
        {
            _type = type;
            if (type.IsDefined(typeof(TableAttribute), true))
            {
                var ta = (TableAttribute)type.GetCustomAttributes(typeof(TableAttribute), true)[0];
                name = ta.TableName;
            }
            if (name != null)
                Name = name;
            else
                Name = type.Name + "s";
        }

        private string GetCreateSql()
        {
            var sql = new StringBuilder();
            sql.AppendLine($"CREATE TABLE IF NOT EXISTS [{Name}] (");

            foreach (var dbColumn in Columns)
            {
                var pk = dbColumn.IsPrimaryKey ? "PRIMARY KEY NOT NULL" : "";
                var def = dbColumn.DefaultValue == null ? "" : $"DEFAULT {dbColumn.DefaultValue}";
                sql.AppendLine($"[{dbColumn.Name}] {dbColumn.DbType} {pk} {def} {dbColumn.Options},");
            }
            sql.Remove(sql.ToString().LastIndexOf(","), 1);
            sql.Append(");");
            return sql.ToString();
        }

        private static string GetDbType(Type type)
        {
            if (type == typeof(string))
                return "TEXT";
            if (type == typeof(int))
                return "INT";
            if (type == typeof(long))
                return "INTEGER";
            if (type == typeof(DateTime))
                return "DATETIME";
            if (type == typeof(double))
                return "DOUBLE";
            if (type == typeof(bool))
                return "BOOLEAN";
            if (type == typeof(Enum) || type.BaseType.Name == "Enum")
                return "INT";
            if (type == typeof(int?))
                return "INT";
            if (type == typeof(long?))
                return "INTEGER";
            if (type == typeof(DateTime?))
                return "DATETIME";
            if (type == typeof(byte[]))
                return "BLOB";

            var t = Nullable.GetUnderlyingType(type);
            if (t?.IsEnum??false)
                return "INT";
            
            throw new Exception("Unknow type");
        }
    }
