# Functions
In `ksqlDB`, there are various **built-in functions** available for processing and transforming data within streams and tables.
These functions can be used in queries to perform calculations, aggregations, string operations, date and time manipulations, and more. Here are some commonly used function categories in `ksqlDB`:
- **scalar** functions
  - **string** functions
  - **mathematical** functions
  - **date and time** functions
  - **invocation** functions
- **aggregation** functions
- **table** functions

## String functions

There are several **string** functions available in `ksqlDB` that allow you to **manipulate** and **transform** string data within streams and tables. These functions can be used in KSQL queries to perform operations such as concatenation, substring extraction, case conversion, and more.

### Concat

The **CONCAT** function is used to concatenate two or more strings together.

```C#
Expression<Func<Tweet, string>> expression = c => K.Functions.Concat(c.Message, "_Value");
```

### LPad, RPad

The **LPAD** function pads a string from the left side, and the **RPAD** function pads a string from the right side. The padding is performed by adding characters to the specified side until the desired length is reached.

```C#
using ksqlDB.RestApi.Client.KSql.Query.Functions;

Expression<Func<Tweet, string>> expression1 = c => KSql.Functions.LPad(c.Message, 8, "x");
Expression<Func<Tweet, string>> expression2 = c => KSql.Functions.RPad(c.Message, 8, "x");
```

KSQL
```KSQL
LPAD(Message, 8, 'x')
RPAD(Message, 8, 'x')
```

### Trim, Substring

The **TRIM** function is used to remove leading and trailing spaces from a string. It helps in cleaning up string values.

The **SUBSTRING** function is used to extract a substring from a given string. It allows you to retrieve a portion of a string based on specified starting position and length.

```C#
using ksqlDB.RestApi.Client.KSql.Query.Functions;

Expression<Func<Tweet, string>> expression3 = c => KSql.Functions.Trim(c.Message);
Expression<Func<Tweet, string>> expression4 = c => K.Functions.Substring(c.Message, 2, 3);
```

KSQL
```KSQL
TRIM(Message)
Substring(Message, 2, 3)
```

### UCase, LCase

The **UCASE** and **LCASE** functions are used to convert strings to **uppercase** and **lowercase**, respectively. These functions take a string expression as an argument and return the converted string.

```C#
l => l.Message.ToLower() != "hi";
l => l.Message.ToUpper() != "HI";
```
```KSQL
LCASE(Latitude) != 'hi'
UCASE(Latitude) != 'HI'
```

### Length (LEN)

The **LENGTH** function is used to determine the length of a string. It returns the number of characters in the given string.

```C#
Expression<Func<Tweet, int>> lengthExpression = c => c.Message.Length;
```
KSQL
```KSQL
LEN(Message)
```

### Like

The **LIKE** operator is used in combination with the % (percent sign) wildcard characters for prefix or suffix matching.

See also [String.StartsWith, String.EndsWith, String.Contains](https://github.com/tomasfabian/ksqlDB.RestApi.Client-DotNet/blob/main/doc/operators.md#operator-like---stringstartswith-stringendswith-stringcontains)

```C#
using ksqlDB.RestApi.Client.KSql.Query.Functions;

Expression<Func<Tweet, bool>> likeExpression = c => KSql.Functions.Like(c.Message, "%santa%");

Expression<Func<Tweet, bool>> likeLCaseExpression = c => KSql.Functions.Like(c.Message.ToLower(), "%santa%".ToLower());
```
Here's the generated KSQL syntax:
```KSQL
"LCASE(Message) LIKE LCASE('%santa%')"
```

## Numeric functions

There are several **numeric** functions available in `ksqlDB` that allow you to perform **calculations** and **manipulations** on numeric data within streams and tables.
These functions can be used in KSQL queries to perform operations such as absolute value calculation, rounding, and more.

### Abs, Ceil, Floor, Random, Sign, Round
```C#
Expression<Func<Tweet, double>> expression1 = c => K.Functions.Abs(c.Amount);
Expression<Func<Tweet, double>> expression2 = c => K.Functions.Ceil(c.Amount);
Expression<Func<Tweet, double>> expression3 = c => K.Functions.Floor(c.Amount);
Expression<Func<Tweet, double>> expression4 = c => K.Functions.Random();
Expression<Func<Tweet, double>> expression5 = c => K.Functions.Sign(c.Amount);

int scale = 3;
Expression<Func<Tweet, double>> expression6 = c => K.Functions.Round(c.Amount, scale);
```

Generated KSQL:
```KSQL
ABS(Amount)
CEIL(AccountBalance)
FLOOR(AccountBalance)
RANDOM()
SIGN(Amount)

ROUND(Amount, 3)
```

## Date and time functions

There are several **date and time** functions available in `ksqlDB` that allow you to perform **operations** and **manipulations** on date and time values within streams and tables.
These functions can be used in KSQL queries to extract specific components, format timestamps, and more.

#### DATETOSTRING
```C#
int epochDays = 18672;
string format = "yyyy-MM-dd";

Expression<Func<Tweet, string>> expression = _ => KSqlFunctions.Instance.DateToString(epochDays, format);
```
Generated KSQL:
```KSQL
DATETOSTRING(18672, 'yyyy-MM-dd')
```

#### TIMESTAMPTOSTRING
```C#
new KSqlDBContext(ksqlDbUrl).CreateQueryStream<Movie>()
  .Select(c => K.Functions.TimestampToString(c.RowTime, "yyyy-MM-dd''T''HH:mm:ssX"))
```

Generated KSQL:
```KSQL
SELECT DATETOSTRING(1613503749145, 'yyyy-MM-dd''T''HH:mm:ssX')
FROM tweets EMIT CHANGES;
```

#### Date and time scalar functions
[Date and time](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/scalar-functions/#date-and-time)

### Entries
```C#
bool sorted = true;
      
var subscription = new KSqlDBContext(@"http://localhost:8088")
  .CreateQueryStream<Movie>()
  .Select(c => new
  {
    Entries = KSqlFunctions.Instance.Entries(new Dictionary<string, string>()
    {
      {"a", "value"}
    }, sorted)
  })
  .Subscribe(c =>
  {
    foreach (var entry in c.Entries)
    {
      var key = entry.K;

      var value = entry.V;
    }
  }, error => {});
```

Generated KSQL:
```KSQL
SELECT ENTRIES(MAP('a' := 'value'), True) Entries 
FROM movies_test EMIT CHANGES;
```

### CAST - ToString
Converts any type to its string representation.

```C#
var query = context.CreateQueryStream<Movie>()
  .GroupBy(c => c.Title)
  .Select(c => new { Title = c.Key, Concatenated = K.Functions.Concat(c.Count().ToString(), "_Hello") });
```

```KSQL
SELECT Title, CONCAT(CAST(COUNT(*) AS VARCHAR), '_Hello') Concatenated FROM Movies GROUP BY Title EMIT CHANGES;
```

### CAST - convert string to numeric types

```C#
using System;
using ksqlDB.RestApi.Client.KSql.Query.Functions;

Expression<Func<Tweet, int>> stringToInt = c => KSQLConvert.ToInt32(c.Message);
Expression<Func<Tweet, long>> stringToLong = c => KSQLConvert.ToInt64(c.Message);
Expression<Func<Tweet, decimal>> stringToDecimal = c => KSQLConvert.ToDecimal(c.Message, 10, 2);
Expression<Func<Tweet, double>> stringToDouble = c => KSQLConvert.ToDouble(c.Message);
```

```KSQL
CAST(Message AS INT)
CAST(Message AS BIGINT)
CAST(Message AS DECIMAL(10, 2))
CAST(Message AS DOUBLE)
```

## Lambda functions (Invocation functions)
**v1.0.0**

- requirements: ksqldb 0.17.0
- This version covers ARRAY type. MAP types are not included in this release.

Lambda functions allow you to compose new expressions from existing ones. Lambda functions must be used inside the following invocation functions:
- **Transform**
- **Reduce**
- **Filter**

See also [Use lambda functions](https://docs.ksqldb.io/en/latest/how-to-guides/use-lambda-functions/) and [Invocation functions](https://docs.ksqldb.io/en/latest/developer-guide/ksqldb-reference/scalar-functions/#invocation-functions)

The following example shows you how to take advantage of invocation functions with ksqlDB.RestApi.Client:

Add namespaces:
```C#
using System;
using System.Threading.Tasks;
using ksqlDB.RestApi.Client.KSql.Linq;
using ksqlDB.RestApi.Client.KSql.Query.Context;
using ksqlDB.RestApi.Client.KSql.Query.Functions;
using ksqlDB.RestApi.Client.KSql.Query.Options;
using ksqlDB.RestApi.Client.KSql.RestApi;
using ksqlDB.RestApi.Client.KSql.RestApi.Statements;
using ksqlDB.RestApi.Client.Sample.Models.InvocationFunctions;
```
Prepare the model:
```C#
record Lambda
{
  public int Id { get; set; }
  public int[] Lambda_Arr { get; set; }
}
```
Create the stream and insert a value:
```C#
public async Task PrepareAsync(IKSqlDbRestApiClient restApiClient)
{
  var statement =
    new KSqlDbStatement(
      @"CREATE STREAM stream2 (id INT, lambda_arr ARRAY<INTEGER>) WITH (kafka_topic = 'stream2', partitions = 1, value_format = 'json');");

  var createStreamResponse = await restApiClient.ExecuteStatementAsync(statement);

  var insertResponse = await restApiClient.ExecuteStatementAsync(
    new KSqlDbStatement("insert into stream2 (id, lambda_arr) values (1, ARRAY [1,2,3]);"));
}
```

Subscribe to the unbounded stream of events:
```C#
public IDisposable Invoke(IKSqlDBContext ksqlDbContext)
{
  var subscription = ksqlDbContext.CreateQuery<Lambda>(fromItemName: "stream2")
    .WithOffsetResetPolicy(AutoOffsetReset.Earliest)
    .Select(c => new
    {
      Transformed = KSqlFunctions.Instance.Transform(c.Lambda_Arr, x => x + 1),
      Filtered = KSqlFunctions.Instance.Filter(c.Lambda_Arr, x => x > 1),
      Acc = K.Functions.Reduce(c.Lambda_Arr, 0, (x, y) => x + y)
    }).Subscribe(c =>
    {
      Console.WriteLine($"Transformed array: {c.Transformed}");
      Console.WriteLine($"Filtered array: {c.Filtered}");
      Console.WriteLine($"Reduced array: {c.Acc}");
    }, error => { Console.WriteLine(error.Message); });

  return subscription;
}
```

The above query is equivalent to:
```KSQL
set 'auto.offset.reset' = 'earliest';

SELECT TRANSFORM(Lambda_Arr, (x) => x + 1) Transformed, FILTER(Lambda_Arr, (x) => x > 1) Filtered, REDUCE(Lambda_Arr, 0, (x, y) => x + y) Acc 
FROM stream2 
EMIT CHANGES;
```

Output:
```
+--------------------------------------+--------------------------------------+--------------------------------------+
|TRANSFORMED                           |FILTERED                              |ACC                                   |
+--------------------------------------+--------------------------------------+--------------------------------------+
|[2, 3, 4]                             |[2, 3]                                |6                                     |
```
 
### Transform arrays
**v1.0.0**

- Transform a collection by using a lambda function.
- If the collection is an array, the lambda function must have one input argument.

```C#
record Tweets
{
  public string[] Messages { get; set; }
  public int[] Values { get; set; }
}
```

```C#
Expression<Func<Tweets, string[]>> expression = c => K.Functions.Transform(c.Messages, x => x.ToUpper());
```

```SQL
TRANSFORM(Messages, (x) => UCASE(x))
```

### Reduce arrays
**v1.0.0**
 
- Reduce a collection starting from an initial state.
- If the collection is an array, the lambda function must have two input arguments.
```C#
Expression<Func<Tweets, int>> expression = c => K.Functions.Reduce(c.Values, 0, (x,y) => x + y);
```

```SQL
REDUCE(Values, 0, (x, y) => x + y)
```

### Filter arrays
**v1.0.0**

- Filter a collection with a lambda function.
- If the collection is an array, the lambda function must have one input argument.
```C#
Expression<Func<Tweets, string[]>> expression = c => K.Functions.Filter(c.Messages, x => x == "E.T.");
```

```SQL
FILTER(Messages, (x) => x = 'E.T.')
```

## Lambda functions (Invocation functions) - Maps
**v1.0.0**

Model:
```C#
record Lambda
{
  public IDictionary<string, int[]> DictionaryArrayValues { get; set; }
  public IDictionary<string, int> DictionaryInValues { get; set; }
}
```

### Transform maps
**v1.0.0**

Transform a collection by using a lambda function.
If the collection is a map, two lambda functions must be provided, and both lambdas must have two arguments: a map entry key and a map entry value.

```C#
Expression<Func<Lambda, IDictionary<string, int[]>>> expression = 
    c => K.Functions.Transform(c.Dictionary, (k, v) => K.Functions.Concat(k, "_new"), (k, v) => K.Functions.Transform(v, x => x * x));
```

Equivalent KSQL:
```SQL
TRANSFORM(DictionaryArrayValues, (k, v) => CONCAT(k, '_new'), (k, v) => TRANSFORM(v, (x) => x * x))
```

### Filter maps
**v1.0.0**

Filter a collection with a lambda function.
If the collection is a map, the lambda function must have two input arguments.

```C#
Expression<Func<Lambda, IDictionary<string, int>>> expression = 
    c => K.Functions.Filter(c.Dictionary2, (k, v) => k != "E.T" && v > 0);
```

Equivalent KSQL:
```SQL
FILTER(DictionaryInValues, (k, v) => (k != 'E.T') AND (v > 0))
```

### Reduce maps
**v1.0.0**

Reduce a collection starting from an initial state.
If the collection is a map, the lambda function must have three input arguments.
If the state is null, the result is null.

```C#
Expression<Func<Lambda, int>> expression = 
    c => K.Functions.Reduce(c.Dictionary2, 2, (s, k, v) => K.Functions.Ceil(s / v));
```

Equivalent KSQL:
```SQL
REDUCE(DictionaryInValues, 2, (s, k, v) => CEIL(s / v))
```

### Improved invocation function extensions
**v1.5.0**

```C#
var ksql = ksqlDbContext.CreateQueryStream<Lambda>()
  .Select(c => new
  {
    Transformed = c.Lambda_Arr.Transform(x => x + 1),
    Filtered = c.Lambda_Arr.Filter(x => x > 1),
    Acc = c.Lambda_Arr.Reduce(0, (x, y) => x + y)
  })
  .ToQueryString();
```

```C#
record Lambda
{
  public int Id { get; set; }
  public int[] Lambda_Arr { get; set; }

  public IDictionary<string, int[]> DictionaryArrayValues { get; set; }
  public IDictionary<string, int> DictionaryInValues { get; set; }
}
```

### Dynamic - calling not supported ksqldb functions

Certain functions in `ksqlDB` are currently unavailable in the .NET client library, but you can work around this limitation by utilizing `K.Functions.Dynamic`.
By constructing the appropriate function call and providing the necessary parameters, you can achieve the desired functionality. When dealing with column values, you can specify their types using the **as operator** for **explicit type casting** in C#.

```C#
using ksqlDB.RestApi.Client.KSql.Query.Functions;

context.CreateQueryStream<Tweet>()
  .Select(c => new { Col = KSql.Functions.Dynamic("IFNULL(Message, 'n/a')") as string, c.Id, c.Amount, c.Message });
```
The interesting part from the above query is:
```C#
K.Functions.Dynamic("IFNULL(Message, 'n/a')") as string
```
Generated KSQL:
```KSQL
SELECT IFNULL(Message, 'n/a') Col, Id, Amount, Message FROM Tweets EMIT CHANGES;
```
Result:
```
+----------------------------+----------------------------+----------------------------+----------------------------+
|COL                         |ID                          |AMOUNT                      |MESSAGE                     |
+----------------------------+----------------------------+----------------------------+----------------------------+
|Hello world                 |1                           |0.0031                      |Hello world                 |
|n/a                         |1                           |0.1                         |null                        |
```

You can achieve a dynamic function call in C# that returns an array by using the appropriate syntax. Here's an example:
```C#
using K = ksqlDB.RestApi.Client.KSql.Query.Functions.KSql;

context.CreateQueryStream<Tweet>()
  .Select(c => K.F.Dynamic("ARRAY_DISTINCT(ARRAY[1, 1, 2, 3, 1, 2])") as int[])
  .Subscribe(
    message => Console.WriteLine($"{message[0]} - {message[^1]}"), 
    error => Console.WriteLine($"Exception: {error.Message}"));
```

### Scalar collection functions
- `ArrayIntersect`
- `ArrayJoin`
