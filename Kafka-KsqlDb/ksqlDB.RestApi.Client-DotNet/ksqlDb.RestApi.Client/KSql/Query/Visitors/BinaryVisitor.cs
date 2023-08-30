using System.Linq.Expressions;
using System.Text;

namespace ksqlDB.RestApi.Client.KSql.Query.Visitors;

internal class BinaryVisitor : KSqlVisitor
{
  private readonly KSqlQueryMetadata queryMetadata;

  public BinaryVisitor(StringBuilder stringBuilder, KSqlQueryMetadata queryMetadata)
    : base(stringBuilder, queryMetadata)
  {
    this.queryMetadata = queryMetadata;
  }

  protected override Expression VisitParameter(ParameterExpression node)
  {
    Append(node.Name);

    return node;
  }

  public override Expression Visit(Expression expression)
  {
    if (expression == null)
      return null;

    switch (expression.NodeType)
    {
      //arithmetic
      case ExpressionType.Add:
      case ExpressionType.Subtract:
      case ExpressionType.Divide:
      case ExpressionType.Multiply:
      case ExpressionType.Modulo:
      //conditionals
      case ExpressionType.AndAlso:
      case ExpressionType.OrElse:
      case ExpressionType.NotEqual:
      case ExpressionType.Equal:
      case ExpressionType.GreaterThan:
      case ExpressionType.GreaterThanOrEqual:
      case ExpressionType.LessThan:
      case ExpressionType.LessThanOrEqual:
      //arrays
      case ExpressionType.ArrayIndex:
        VisitBinary((BinaryExpression)expression);
        break;
      default:
        base.Visit(expression);
        break;
    }

    return expression;
  }

  private const string OperatorAnd = "AND";

  private static readonly ISet<ExpressionType> SupportedBinaryOperators = new HashSet<ExpressionType>
  {
    ExpressionType.Add,
    ExpressionType.Subtract,
    ExpressionType.Divide,
    ExpressionType.Multiply,
    ExpressionType.Modulo,
    ExpressionType.AndAlso,
    ExpressionType.OrElse,
    ExpressionType.NotEqual,
    ExpressionType.Equal,
    ExpressionType.GreaterThan,
    ExpressionType.GreaterThanOrEqual,
    ExpressionType.LessThan,
    ExpressionType.LessThanOrEqual,
  };

  protected override Expression VisitBinary(BinaryExpression binaryExpression)
  {
    if (binaryExpression == null) throw new ArgumentNullException(nameof(binaryExpression));

    bool IsBinaryOperation(ExpressionType expressionType) => SupportedBinaryOperators.Contains(expressionType);

    bool shouldAddParentheses = IsBinaryOperation(binaryExpression.Left.NodeType);

    if (shouldAddParentheses)
      Append("(");

    Visit(binaryExpression.Left);

    if (shouldAddParentheses)
      Append(")");

    if (binaryExpression.NodeType == ExpressionType.ArrayIndex)
    {
      Append("[");
      Visit(binaryExpression.Right);
      Append("]");
    
      return binaryExpression;
    }

    //https://docs.ksqldb.io/en/latest/reference/sql/appendix/
    string @operator = binaryExpression.NodeType switch
    {
      //arithmetic
      ExpressionType.Add => "+",
      ExpressionType.Subtract => "-",
      ExpressionType.Divide => "/",
      ExpressionType.Multiply => "*",
      ExpressionType.Modulo => "%",
      //conditionals
      ExpressionType.AndAlso => OperatorAnd,
      ExpressionType.OrElse => "OR",
      ExpressionType.Equal when binaryExpression.Right is ConstantExpression ce && ce.Value == null => "IS",
      ExpressionType.Equal => "=",
      ExpressionType.NotEqual when binaryExpression.Right is ConstantExpression ce && ce.Value == null => "IS NOT",
      ExpressionType.NotEqual => "!=",
      ExpressionType.LessThan => "<",
      ExpressionType.LessThanOrEqual => "<=",
      ExpressionType.GreaterThan => ">",
      ExpressionType.GreaterThanOrEqual => ">=",
      _ => throw new ArgumentOutOfRangeException(nameof(binaryExpression.NodeType))
    };

    @operator = $" {@operator} ";

    Append(@operator);

    shouldAddParentheses = IsBinaryOperation(binaryExpression.Right.NodeType);

    if (shouldAddParentheses)
      Append("(");

    Visit(binaryExpression.Right);

    if (shouldAddParentheses)
      Append(")");

    return binaryExpression;
  }

  protected override Expression VisitMember(MemberExpression memberExpression)
  {
    if (queryMetadata.IsInNestedFunctionScope)
      return new LambdaVisitor(StringBuilder, queryMetadata).Visit(memberExpression) ?? memberExpression;

    return base.VisitMember(memberExpression);
  }
}
