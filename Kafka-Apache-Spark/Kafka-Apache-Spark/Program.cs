
// Create Spark session
using Microsoft.Spark.Sql;

using static Microsoft.Spark.Sql.Functions;

SparkSession spark =
    SparkSession
        .Builder()
        .AppName("word_count_sample")
        .GetOrCreate();

// Create initial DataFrame
string filePath = args[0];
DataFrame dataFrame = spark.Read().Text(filePath);

//Count words
DataFrame words =
    dataFrame
        .Select(Split(Col("value"), " ").Alias("words"))
        .Select(Explode(Col("words")).Alias("word"))
        .GroupBy("word")
        .Count()
        .OrderBy(Col("count").Desc());

// Display results
words.Show();

// Stop Spark session
spark.Stop();