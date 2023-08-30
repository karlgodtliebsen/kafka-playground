namespace ksqlDB.Api.Client.IntegrationTests.Models.Movies;

public record Movie : Record
{
  public string Title { get; set; } = null!;
  public int Id { get; set; }
  public int Release_Year { get; set; }
}
