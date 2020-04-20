using System;

using Newtonsoft.Json;

namespace MarceloCTorres.Covid19Api.Core
{
  public class DailyReport
  {
    [JsonProperty("region")]
    public CountryRegion CountryRegion { get; set; }

    [JsonProperty("lastUpdate")]
    public DateTime? LastUpdate { get; set; }

    [JsonProperty("dailyData")]
    public DailyData DailyData { get; set; }

  }
}
