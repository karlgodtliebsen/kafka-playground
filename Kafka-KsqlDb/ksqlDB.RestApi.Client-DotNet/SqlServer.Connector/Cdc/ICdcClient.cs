namespace SqlServer.Connector.Cdc
{
  public interface ICdcClient
  {    
    Task CdcEnableDbAsync();
    Task CdcDisableDbAsync();
  }
}
