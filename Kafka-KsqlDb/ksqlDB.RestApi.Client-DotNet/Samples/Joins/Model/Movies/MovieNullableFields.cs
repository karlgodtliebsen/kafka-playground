﻿using ksqlDB.RestApi.Client.KSql.RestApi.Statements.Annotations;

namespace Joins.Model.Movies;

public class MovieNullableFields
{
  public string Title { get; set; } = null!;
  [Key]
  public int? Id { get; set; }
  public int? Release_Year { get; set; }
}