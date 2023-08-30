﻿using System.Linq.Expressions;
using System.Text;
using ksqlDB.RestApi.Client.KSql.Query.Functions;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class KSqlWindowBoundsVisitor : KSqlVisitor
{
  public KSqlWindowBoundsVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
  }

  protected override Expression VisitMember(MemberExpression memberExpression)
  {
    if (memberExpression.Type == typeof(Bounds))
    {
      Append(memberExpression.Member.Name.ToUpper());
    }

    return memberExpression;
  }
}