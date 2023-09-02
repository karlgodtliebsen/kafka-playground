﻿using System.Linq.Expressions;
using System.Reflection;
using ksqlDB.RestApi.Client.Infrastructure.Extensions;
using ksqlDb.RestApi.Client.KSql.Entities;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Linq.PullQueries;
using ksqlDB.RestApi.Client.KSql.Linq.Statements;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDb.RestApi.Client.KSql.Query.PushQueries;
using ksqlDB.RestApi.Client.KSql.Query.Visitors;
using ksqlDB.RestApi.Client.KSql.Query.Windows;
using Pluralize.NET;

namespace ksqlDB.RestApi.Client.KSql.Query;

internal class KSqlQueryGenerator : ExpressionVisitor, IKSqlQueryGenerator
{
  private readonly KSqlDBContextOptions options;
  private static readonly IPluralize EnglishPluralizationService = new Pluralizer();

  private KSqlVisitor kSqlVisitor;
  private KSqlQueryMetadata queryMetadata;

  public bool ShouldEmitChanges { get; set; } = true;

  public KSqlQueryGenerator(KSqlDBContextOptions options)
  {
    this.options = options ?? throw new ArgumentNullException(nameof(options));
  }

  public string BuildKSql(Expression expression, QueryContext queryContext)
  {
    queryMetadata = new KSqlQueryMetadata();

    kSqlVisitor = new KSqlVisitor(queryMetadata);
    whereClauses = new Queue<Expression>();
    joins = new List<(MethodInfo, IEnumerable<Expression>, LambdaExpression)>();

    Visit(expression);

    string finalFromItemName = InterceptFromItemName(queryContext.FromItemName ?? fromItemName);

    queryContext.AutoOffsetReset = autoOffsetReset;

    queryMetadata.FromItemType = fromTableType;

    if (joins.Any())
    {
      var fromItem = joins.Last();

      var lambdaExpression = StripQuotes(fromItem.Item2.ToArray()[1]) as LambdaExpression;
      var alias = lambdaExpression.Parameters[0].Name;

      var fromTable = new FromItem
      {
        Type = queryMetadata.FromItemType,
        Alias = alias
      };

      queryMetadata.Joins = GetFromItems(joins, fromTable);

      var joinsVisitor = new KSqlJoinsVisitor(kSqlVisitor.StringBuilder, options, new QueryContext { FromItemName = finalFromItemName }, queryMetadata);

      joinsVisitor.VisitJoinTable(joins);
    }
    else
    {
      kSqlVisitor.Append("SELECT ");

      if (queryMetadata.Select != null)
        kSqlVisitor.Visit(queryMetadata.Select.Body);
      else
        kSqlVisitor.Append("*");

      kSqlVisitor.Append($" FROM {finalFromItemName}");
    }

    bool isFirst = true;

    foreach (var methodCallExpression in whereClauses)
    {
      if (isFirst)
      {
        kSqlVisitor.Append(HasJoins ? string.Empty : Environment.NewLine);
        kSqlVisitor.Append("WHERE ");

        isFirst = false;
      }
      else
        kSqlVisitor.Append(" AND ");

      kSqlVisitor.Visit(methodCallExpression);
    }

    var timeWindows = TryGenerateWindowAggregation();

    if (groupBy != null)
    {
      kSqlVisitor.Append(" GROUP BY ");
      kSqlVisitor.Visit(groupBy.Body);
    }

    if (having != null)
    {
      kSqlVisitor.Append(" HAVING ");
      kSqlVisitor.Visit(having.Body);
    }

    if (partitionBy != null)
    {
      kSqlVisitor.Append(" PARTITION BY ");
      kSqlVisitor.Visit(partitionBy.Body);
    }

    if (ShouldEmitChanges)
    {
      string separator = string.Empty;

      if (!HasJoins || (HasJoins && whereClauses.Any()))
        separator = " ";

      string outputRefinement = timeWindows is {OutputRefinement: OutputRefinement.Final} ? "FINAL" : "CHANGES";

      kSqlVisitor.Append($"{separator}EMIT {outputRefinement}");
    }

    if (limit.HasValue)
      kSqlVisitor.Append($" LIMIT {limit}");

    kSqlVisitor.Append(";");

    return kSqlVisitor.BuildKSql();
  }

  private bool HasJoins => joins?.Any() ?? false;

  private TimeWindows TryGenerateWindowAggregation()
  {
    if (windowedBy == null)
      return null;

    new KSqlWindowsVisitor(kSqlVisitor.StringBuilder, queryMetadata).Visit(windowedBy);

    var constantExpression = windowedBy;

    return (TimeWindows)constantExpression.Value;
  }

  protected virtual string InterceptFromItemName(string value)
  {
    if (options.ShouldPluralizeFromItemName)
      return EnglishPluralizationService.Pluralize(value);

    return value;
  }

  public override Expression Visit(Expression expression)
  {
    if (expression == null)
      return null;

    switch (expression.NodeType)
    {
      case ExpressionType.Constant:
        VisitConstant((ConstantExpression)expression);
        break;
      case ExpressionType.Call:
        VisitMethodCall((MethodCallExpression)expression);
        break;
    }

    return expression;
  }

  private string fromItemName;

  private Type fromTableType;

  protected override Expression VisitConstant(ConstantExpression constantExpression)
  {
    if (constantExpression == null) throw new ArgumentNullException(nameof(constantExpression));

    var type = constantExpression.Type;

    var kStreamSetType = type.TryFindProviderAncestor();

    if (kStreamSetType != null)
    {
      fromTableType = ((KSet)constantExpression.Value)?.ElementType;

      fromItemName = fromTableType.ExtractTypeName();
    }

    return constantExpression;
  }

  protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
  {
    var methodInfo = methodCallExpression.Method;

    if(methodInfo.DeclaringType.IsNotOneOfFollowing(typeof(QbservableExtensions), typeof(CreateStatementExtensions), typeof(PullQueryExtensions)))
      return methodCallExpression;

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Select), nameof(CreateStatementExtensions.Select)))
    {
      LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

      if (queryMetadata.Select == null)
        queryMetadata.Select = lambda;

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name == nameof(CreateStatementExtensions.PartitionBy))
    {
      LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

      if (partitionBy == null)
        partitionBy = lambda;

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Where), nameof(CreateStatementExtensions.Where), nameof(PullQueryExtensions.Where)))
    {
      VisitChained(methodCallExpression);

      LambdaExpression lambda = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);
      whereClauses.Enqueue(lambda.Body);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Take), nameof(CreateStatementExtensions.Take)))
    {
      var arg = (ConstantExpression)methodCallExpression.Arguments[1];
      limit = (int)arg.Value;

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.WithOffsetResetPolicy)))
    {
      var arg = (ConstantExpression)methodCallExpression.Arguments[1];
      autoOffsetReset = (AutoOffsetReset)arg.Value;

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.WindowedBy), nameof(CreateStatementExtensions.WindowedBy)))
    {
      windowedBy = (ConstantExpression)StripQuotes(methodCallExpression.Arguments[1]);

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.GroupBy), nameof(CreateStatementExtensions.GroupBy)))
    {
      groupBy = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.SelectMany)))
    {
      selectManyGroupJoin = (LambdaExpression)StripQuotes(methodCallExpression.Arguments.Last());

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.GroupJoin)))
    {
      var joinTable = methodCallExpression.Arguments.Skip(1);

      joins.Add((methodInfo, joinTable, selectManyGroupJoin));

      selectManyGroupJoin = null;

      VisitChained(methodCallExpression);
    }

    if (methodInfo.Name.IsOneOfFollowing(nameof(QbservableExtensions.Having), nameof(CreateStatementExtensions.Having)))
    {
      having = (LambdaExpression)StripQuotes(methodCallExpression.Arguments[1]);

      VisitChained(methodCallExpression);
    }

    switch (methodInfo.Name)
    {
      case nameof(QbservableExtensions.Join):
      case nameof(QbservableExtensions.LeftJoin):
      case nameof(QbservableExtensions.RightJoin):
      case nameof(QbservableExtensions.FullOuterJoin):
        var joinTable = methodCallExpression.Arguments.Skip(1);

        joins.Add((methodInfo, joinTable, null));

        VisitChained(methodCallExpression);
        break;
    }

    return methodCallExpression;
  }

  private void VisitChained(MethodCallExpression methodCallExpression)
  {
    var firstPart = methodCallExpression.Arguments[0];

    if (firstPart.NodeType == ExpressionType.Call || firstPart.NodeType == ExpressionType.Constant)
      Visit(firstPart);
  }

  private Queue<Expression> whereClauses;
  private LambdaExpression partitionBy;
  private AutoOffsetReset? autoOffsetReset;
  private int? limit;
  private ConstantExpression windowedBy;
  private LambdaExpression groupBy;
  private LambdaExpression selectManyGroupJoin;
  private LambdaExpression having;
  private List<(MethodInfo, IEnumerable<Expression>, LambdaExpression)> joins;

  private static Expression StripQuotes(Expression expression)
  {
    while (expression.NodeType == ExpressionType.Quote)
    {
      expression = ((UnaryExpression)expression).Operand;
    }

    return expression;
  }

  private static FromItem[] GetFromItems(List<(MethodInfo, IEnumerable<Expression>, LambdaExpression)> joins, FromItem fromItem)
  {
    return joins.Select(c =>
      {
        var items = c.Item2.ToArray();
        var lambdaExpression = StripQuotes(items[2]) as LambdaExpression;
        var alias = lambdaExpression.Parameters[0].Name;

        var type = items[0].Type.GenericTypeArguments[0];

        return new FromItem {Type = type, Alias = alias};
      }).Append(fromItem)
      .ToArray();
  }
}
