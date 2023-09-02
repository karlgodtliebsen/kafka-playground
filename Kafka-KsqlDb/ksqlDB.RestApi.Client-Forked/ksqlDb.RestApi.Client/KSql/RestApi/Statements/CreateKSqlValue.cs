﻿using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Formats;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Properties;

namespace ksqlDB.RestApi.Client.KSql.RestApi.Statements;

internal sealed class CreateKSqlValue : CreateEntityStatement
{
  public object ExtractValue<T>(T inputValue, IValueFormatters valueFormatters, MemberInfo memberInfo, Type type)
  {
    Type valueType = inputValue.GetType();

    object value = inputValue;

    if (memberInfo?.MemberType == MemberTypes.Property && ((PropertyInfo) memberInfo).GetAccessors().Any())
      value = valueType.GetProperty(memberInfo.Name)?.GetValue(inputValue);
    else if (memberInfo?.MemberType == MemberTypes.Field)
      value = valueType.GetField(memberInfo.Name)?.GetValue(inputValue);

    if (value == null)
      return "NULL";

    if (type == typeof(decimal) && valueFormatters.FormatDecimalValue != null)
    {
      Debug.Assert(value != null, nameof(value) + " != null");

      value = valueFormatters.FormatDecimalValue((decimal)value);
    }
    else if (type == typeof(TimeSpan))
    {
      TimeSpan timeSpan = (TimeSpan)value;

      value = timeSpan.ToString(ValueFormats.TimeFormat, CultureInfo.InvariantCulture);
      value = $"'{value}'";
    }
    else if (type == typeof(DateTime))
    {
      DateTime date = (DateTime)value;

      value = date.ToString(ValueFormats.DateFormat, CultureInfo.InvariantCulture);
      value = $"'{value}'";
    }
    else if (type == typeof(Guid))
    {
      var guid = ((Guid)value).ToString();

      value = $"'{guid}'";
    }
    else if (type == typeof(DateTimeOffset))
    {
      var dateTimeOffset = (DateTimeOffset)value;
        
      value = dateTimeOffset.ToString(ValueFormats.DateTimeOffsetFormat, CultureInfo.InvariantCulture);

      value = $"'{value}'";
    }
    else if (type == typeof(double))
    {
      Debug.Assert(value != null, nameof(value) + " != null");

      if(valueFormatters.FormatDoubleValue != null)
        value = valueFormatters.FormatDoubleValue((double)value);
    }
    else if (type == typeof(string))
      value = $"'{value}'";
    else if (type.IsPrimitive)
      value = value.ToString();
    else if (type.IsDictionary())
      GenerateMap(valueFormatters, type, ref value);
    else if (type.IsArray)
    {
      var source = ((IEnumerable)value).Cast<object>();
      var array = source.Select(c => ExtractValue(c, valueFormatters, null, type.GetElementType())).ToArray();
      value = PrintArray(array);
    }
    else if (!type.IsGenericType && (type.IsClass || type.IsStruct()))
    {
      GenerateStruct<T>(valueFormatters, type, ref value);
    }
    else
    {
      value = GenerateEnumerableValue(type, value, valueFormatters);
    }

    return value;
  }

  private void GenerateMap(IValueFormatters valueFormatters, Type type, ref object value)
  {
    if (value is not IDictionary dict)
      return;

    var sb = new StringBuilder();

    sb.Append("MAP(");

    bool isFirst = true;

    foreach (DictionaryEntry dictionaryEntry in dict)
    {
      if (isFirst)
        isFirst = false;
      else
        sb.Append(", ");

      var key = ExtractValue(dictionaryEntry.Key, valueFormatters, type, dictionaryEntry.Key.GetType());

      sb.Append(key);

      sb.Append(" := ");

      var dictValue = ExtractValue(dictionaryEntry.Value, valueFormatters, type, dictionaryEntry.Value.GetType());

      sb.Append(dictValue);
    }

    sb.Append(")");

    value = sb.ToString();
  }

  private void GenerateStruct<T>(IValueFormatters valueFormatters, Type type, ref object value)
  {
    var sb = new StringBuilder();

    sb.Append("STRUCT(");

    bool isFirst = true;

    foreach (var memberInfo2 in Members(type))
    {
      if (isFirst)
        isFirst = false;
      else
        sb.Append(", ");

      type = GetMemberType(memberInfo2);

      var innerValue = ExtractValue(value, valueFormatters, memberInfo2, type);
      sb.Append($"{memberInfo2.Name} := {innerValue}");
    }

    sb.Append(")");

    value = sb.ToString();
  }

  private object GenerateEnumerableValue(Type type, object value, IValueFormatters valueFormatters)
  {
    if (value == null)
      return "NULL";

    var enumerableType = type.GetEnumerableTypeDefinition();

    if (enumerableType == null || !enumerableType.Any())
      return value;

    type = enumerableType.First();
    type = type.GetGenericArguments()[0];

    var source = ((IEnumerable)value).Cast<object>();
    var array = source.Select(c => ExtractValue(c, valueFormatters, null, type)).ToArray();

    if (!array.Any())
      return "ARRAY_REMOVE(ARRAY[0], 0)";

    return PrintArray(array);
  }

  private static object PrintArray(object[] array)
  {
    var sb = new StringBuilder();
    sb.Append("ARRAY[");
#if NETSTANDARD
      var value = string.Join(", ", array);
#else
    var value = string.Join(", ", array);
#endif
    sb.Append(value);
    sb.Append("]");

    value = sb.ToString();

    return value;
  }
}