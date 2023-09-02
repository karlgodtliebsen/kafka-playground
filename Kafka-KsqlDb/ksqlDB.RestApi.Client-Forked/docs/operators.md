# Operators

Supported **comparison** and **logical** operators that can be used in value expressions:

|   ksql   |           meaning           |  c#  |
|:--------:|:---------------------------:|:----:|
| =        | is equal to                 | ==   |
| != or <> | is not equal to             | !=   |
| <        | is less than                | <    |
| <=       | is less than or equal to    | <=   |
| >        | is greater than             | >    |
| >=       | is greater than or equal to | >=   |
| AND      | logical AND                 | &&   |
| OR       | logical OR                  | \|\| |
| NOT      | logical NOT                 |  !   |

### Operator LIKE - String.StartsWith, String.EndsWith, String.Contains
**v1.3.0**

The **LIKE** operator is used in value expressions to perform pattern matching on strings.
It allows you to match strings against a specified pattern using wildcard characters.
The **LIKE** operator is used in combination with the % (percent sign) wildcard characters for prefix or suffix matching.

Match a string with a specified pattern:

```C#
var query = context.CreateQueryStream<Movie>()
  .Where(c => c.Title.ToLower().Contains("hard".ToLower());
```

```SQL
SELECT *
  FROM Movies
 WHERE LCASE(Title) LIKE LCASE('%hard%')
  EMIT CHANGES;
```

```C#
var query = context.CreateQueryStream<Movie>()
  .Where(c => c.Title.StartsWith("Die");
```

```SQL
SELECT *
  FROM Movies
 WHERE Title LIKE 'Die%'
  EMIT CHANGES;
```

### WHERE IS NULL, IS NOT NULL
**v1.0.0**

The **IS NULL** and **IS NOT NULL** operators are used in the **WHERE** clause to check for the presence or absence of a `NULL` value in a column.

```C#
using var subscription = new KSqlDBContext(@"http://localhost:8088")
  .CreateQueryStream<Click>()
  .Where(c => c.IP_ADDRESS != null || c.URL == null)
  .Select(c => new { c.IP_ADDRESS, c.URL, c.TIMESTAMP });
```

Generated KSQL:
```SQL
SELECT IP_ADDRESS, URL, TIMESTAMP
  FROM Clicks
 WHERE IP_ADDRESS IS NOT NULL OR URL IS NULL
  EMIT CHANGES;
```

This query selects all rows from the stream 'Clicks' where the value in the column 'IP_ADDRESS' is not NULL or 'URL' is NULL.

### Operator IN - `IEnumerable<T>` and `IList<T>` Contains
**v1.0.0**

The **IN** operator is used in the **WHERE** clause to check if a value matches any value in a list.
It allows you to specify multiple values and compare them to a single column or expression.

Specifies multiple OR conditions.
`IList<T>`.Contains:
```C#
var orderTypes = new List<int> { 1, 2, 3 };

Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

```
Enumerable extension:
```C#
IEnumerable<int> orderTypes = Enumerable.Range(1, 3);

Expression<Func<OrderData, bool>> expression = o => orderTypes.Contains(o.OrderType);

```
For both options the following SQL is generated:
```SQL
OrderType IN (1, 2, 3)
```

### Operator (NOT) BETWEEN
**v1.0.0**

The **BETWEEN** operator provides a concise way to specify range conditions in KSQL queries, making it useful for filtering data based on a range of values.

`KSqlOperatorExtensions.Between` - constrain a value to a specified range in a WHERE clause.

```C#
using ksqlDB.RestApi.Client.KSql.Query.Operators;

IQbservable<Tweet> query = context.CreateQueryStream<Tweet>()
  .Where(c => c.Id.Between(1, 5));
```

Generated KSQL:

```SQL
SELECT *
  FROM Tweets
 WHERE Id BETWEEN 1 AND 5 EMIT CHANGES;
```

This query selects all rows from 'Tweets' where the value in column 'Id' is between 1 and 5.

### Operator Between for Time type values
**v1.5.0**

```C#
var from = new TimeSpan(11, 0, 0);
var to = new TimeSpan(15,0 , 0);

Expression<Func<MyTimeSpan, TimeSpan>> expression = t => t.Ts.Between(from, to);
```

```SQL
Ts BETWEEN '11:00:00' AND '15:00:00'
```

```C#
var from = new TimeSpan(11, 0, 0);
var to = new TimeSpan(15, 0, 0);

var query = context.CreateQueryStream<MyClass>()
  .Where(c => c.Ts.Between(from, to))
  .Select(c => new { c.Ts, to, FromTime = from, DateTime.Now, New = new TimeSpan(1, 0, 0) }
  .ToQueryString();
```

### CASE
**v1.0.0**

- Select a condition from one or more expressions.
```C#
var query = new KSqlDBContext(@"http://localhost:8088")
  .CreateQueryStream<Tweet>()
  .Select(c =>
    new
    {
      case_result =
        (c.Amount < 2.0) ? "small" :
        (c.Amount < 4.1) ? "medium" : "large"
    }
  );
```

```SQL
SELECT 
  CASE 
    WHEN Amount < 2 THEN 'small' 
    WHEN Amount < 4.1 THEN 'medium' 
    ELSE 'large' 
  END AS case_result 
 FROM Tweets EMIT CHANGES;
```

### Arithmetic operations on columns
The usual **arithmetic** operators (+,-,/,*,%) may be applied to numeric types, like INT, BIGINT, and DOUBLE:
```SQL
SELECT USERID, LEN(FIRST_NAME) + LEN(LAST_NAME) AS NAME_LENGTH
  FROM USERS
  EMIT CHANGES;
```

```C#
Expression<Func<Person, object>> expression = c => c.FirstName.Length * c.LastName.Length;
```

### Lexical precedence
You can use parentheses to change the order of evaluation:
```C#
await using var context = new KSqlDBContext(@"http://localhost:8088");

var query = context.CreateQueryStream<Location>()
  .Select(c => (c.Longitude + c.Longitude) * c.Longitude);
```

```SQL
SELECT (Longitude + Longitude) * Longitude
  FROM Locations
  EMIT CHANGES;
```

In Where clauses:
```C#
await using var context = new KSqlDBContext(@"http://localhost:8088");

var query = context.CreateQueryStream<Location>()
  .Where(c => (c.Latitude == "1" || c.Latitude != "2") && c.Latitude == "3");
```

```SQL
SELECT *
  FROM Locations
 WHERE ((Latitude = '1') OR (Latitude != '2')) AND (Latitude = '3')
  EMIT CHANGES;
```

Redundant brackets are not reduced in the current version.
