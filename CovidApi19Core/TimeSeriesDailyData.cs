namespace MarceloCTorres.Covid19Api.Core
{
  public class TimeSeriesDailyData : DailyData
  {
    public long Delta_Confirmed { get; set; }

    public long Delta_Deaths { get; set; }

    public long Delta_Recovered { get; set; }

    public long Delta_Active { get; set; }
  }
}
