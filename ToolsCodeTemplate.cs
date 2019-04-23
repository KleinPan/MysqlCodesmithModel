using System;
using SchemaExplorer;
using System.Data;
using CodeSmith.Engine;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;

public class ToolsCodeTemplate : CodeTemplate
{
    public void PrintHeader()
    {
        Response.WriteLine("//============================================================");
        Response.WriteLine("//创建时间:"+DateTime.Now);
        Response.WriteLine("//创建人:PeterPanwb");
        Response.WriteLine("//项目开源地址:https://github.com/PeterPanwb/MysqlCodesmithModel");
        Response.WriteLine("//============================================================");
        Response.WriteLine();
    }
    /// <summary> 获取列描述的内容 </summary>
    /// <param name="column">  </param>
    /// <returns>  </returns>
    public string GetColumnComment(ColumnSchema column)
    {
        return column.Description;
    }

    /// <summary> 获取表名称 </summary>
    /// <param name="table">  </param>
    /// <returns>  </returns>
    public string GetModelClassName(TableSchema table)
    {
        string result;
        if (table.ExtendedProperties.Contains("ModelName"))
        {
            result = ((string)table.ExtendedProperties["ModelName"].Value);
            //return MakePascal(result);
        }
        if (table.Name.EndsWith("s"))
        {
            result = MakeSingle(table.Name);
        }
        else
        {
            result = table.Name;
        }
        return ConvertToPascal(result);
    }
    //获取属性对应的C#类型
    public string GetPropertyType(ColumnSchema column)
    {
        return GetCSharpTypeFromDBFieldType(column);
    }

    public string GetCSharpTypeFromDBFieldType(ColumnSchema column)
    {
        if (column.Name.EndsWith("TypeCode")) return column.Name;
        string type;
        switch (column.DataType)
        {
            case DbType.AnsiString: type = "string"; break;
            case DbType.AnsiStringFixedLength: type = "string"; break;
            case DbType.Binary: type = "byte[]"; break;
            case DbType.Boolean: type = "bool"; break;
            case DbType.Byte: type = "byte"; break;
            case DbType.Currency: type = "decimal"; break;
            case DbType.Date: type = "DateTime"; break;
            case DbType.DateTime: type = "DateTime"; break;
            case DbType.Decimal: type = "decimal"; break;
            case DbType.Double: type = "double"; break;
            case DbType.Guid: type = "Guid"; break;
            case DbType.Int16: type = "short"; break;
            case DbType.Int32: type = "int"; break;
            case DbType.Int64: type = "long"; break;
            case DbType.Object: type = "object"; break;
            case DbType.SByte: type = "sbyte"; break;
            case DbType.Single: type = "float"; break;
            case DbType.String: type = "string"; break;
            case DbType.StringFixedLength: type = "string"; break;
            case DbType.Time: type = "TimeSpan"; break;
            case DbType.UInt16: type = "ushort"; break;
            case DbType.UInt32: type = "uint"; break;
            case DbType.UInt64: type = "ulong"; break;
            case DbType.VarNumeric: type = "decimal"; break;
            default:
                {
                    type = "__UNKNOWN__" + column.NativeType;
                    break;
                }
        }
        if (column.AllowDBNull &&
            column.SystemType.IsValueType)
        {
            type = type + "?";
        }
        return type;
    }

    public string GetPropertyName(ColumnSchema column)
    {
        //return MakePascal(GetNameFromDBFieldName(column));
        return GetNameFromDBFieldName(column);
    }

    //获取列名称
    public string GetNameFromDBFieldName(ColumnSchema column)
    {
        return column.Name;
    }

    //变为单数
    public string MakeSingle(string name)
    {
        Regex plural1 = new Regex("(?<keep>[^aeiou])ies$");
        Regex plural2 = new Regex("(?<keep>[aeiou]y)s$");
        Regex plural3 = new Regex("(?<keep>[sxzh])es$");
        Regex plural4 = new Regex("(?<keep>[^sxzhyu])s$");

        if (plural1.IsMatch(name))
            return plural1.Replace(name, "${keep}y");
        else if (plural2.IsMatch(name))
            return plural2.Replace(name, "${keep}");
        else if (plural3.IsMatch(name))
            return plural3.Replace(name, "${keep}");
        else if (plural4.IsMatch(name))
            return plural4.Replace(name, "${keep}");

        return name;
    }

    /// <summary> 根据表对象获得主键的名称 </summary>
    /// <param name="TargetTable">  </param>
    /// <returns>  </returns>
    public string GetPKName(TableSchema TargetTable)
    {
        if (TargetTable.PrimaryKey != null)
        {
            if (TargetTable.PrimaryKey.MemberColumns.Count == 1)
            {
                return TargetTable.PrimaryKey.MemberColumns[0].Name;
            }
            else
            {
                throw new Exception("此模板只支持单个列的主键!");
            }
        }
        else
        {
            throw new Exception("此模板需要有主键的表!");
        }
    }

    // 根据表对象获得主键的类型
    public string GetPKType(TableSchema TargetTable)
    {
        if (TargetTable.PrimaryKey != null)
        {
            if (TargetTable.PrimaryKey.MemberColumns.Count == 1)
            {
                return GetCSharpTypeFromDBFieldType(TargetTable.PrimaryKey.MemberColumns[0]);
            }
            else
            {
                throw new ApplicationException("此模板只支持单个列的主键!");
            }
        }
        else
        {
            throw new ApplicationException("此模板需要有主键的表!");
        }
    }
    //转化为驼峰命名法
    public string ConvertToCamel(string value)
    {
        return value.Substring(0, 1).ToLower() + value.Substring(1);
    }
    //转化为帕斯卡命名法
    public string ConvertToPascal(string str)
    {
        return str.Substring(0, 1).ToUpper() + str.Substring(1);
    }
    //变为复数
    public string MakePlural(string name)
    {
        Regex plural1 = new Regex("(?<keep>[^aeiou])y$");
        Regex plural2 = new Regex("(?<keep>[aeiou]y)$");
        Regex plural3 = new Regex("(?<keep>[sxzh])$");
        Regex plural4 = new Regex("(?<keep>[^sxzhy])$");

        if (plural1.IsMatch(name))
            return plural1.Replace(name, "${keep}ies");
        else if (plural2.IsMatch(name))
            return plural2.Replace(name, "${keep}s");
        else if (plural3.IsMatch(name))
            return plural3.Replace(name, "${keep}es");
        else if (plural4.IsMatch(name))
            return plural4.Replace(name, "${keep}s");

        return name;
    }
}