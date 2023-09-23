
# NET for Apache® Spark™ 


Integration of .NET for Apache® Spark™ (The .NET for Apache Spark project is part of the .NET Foundation.)

Must read:

- https://dotnet.microsoft.com/en-us/apps/data/spark
- https://learn.microsoft.com/da-dk/previous-versions/dotnet/spark/what-is-spark?WT.mc_id=dotnet-35129-website
- https://learn.microsoft.com/da-dk/previous-versions/dotnet/spark/what-is-apache-spark-dotnet

Requires Java and Spark-util, follow the instructions below:

- https://learn.microsoft.com/en-us/previous-versions/dotnet/spark/tutorials/get-started?source=recommendations&tabs=linux
- https://dotnet.microsoft.com/en-us/apps/data/spark
- https://spark.apache.org/		

Use docker to run the spark container

	- docker run -it --rm spark /opt/spark/bin/spark-sql
 


- https://learn.microsoft.com/da-dk/previous-versions/dotnet/spark/tutorials/get-started?tabs=windows:

setx /M HADOOP_HOME C:\tools\spark-3.5.0-bin-hadoop3\
setx /M SPARK_HOME C:\tools\spark-3.5.0-bin-hadoop3\
setx /M PATH "%PATH%;%HADOOP_HOME%;%SPARK_HOME%bin" 



#Warning: Don't run this if your path is already long as it will truncate your path to 1024 characters and potentially remove entries!




