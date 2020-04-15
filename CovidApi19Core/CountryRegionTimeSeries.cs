using System.Collections.Generic;

using Newtonsoft.Json;

namespace MarceloCTorres.CovidApi19.Core
{
  public class CountryRegionTimeSeries
  {
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("provinceState")]
    public string ProvinceState { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("countryRegion")]
    public CountryRegion CountryRegion { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("timeSeries")]
    public List<DailyData> TimeSeries { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public CountryRegionTimeSeries() => TimeSeries = new List<DailyData>();
  }
}
