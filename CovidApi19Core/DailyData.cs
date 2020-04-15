using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MarceloCTorres.CovidApi19.Core
{
  public class DailyData
  {
    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("confirmed")]
    public long Confirmed { get; set; }

    [JsonProperty("deaths")]
    public long Deaths { get; set; }

    private long _recovered;

    [JsonProperty("recovered")]
    public long Recovered
    {
      get => _recovered;
      set
      {
        _recovered = value;
        ActualActive = Confirmed - Deaths - _recovered;
      }
    }

    [JsonIgnore]
    public long Active { get; set; }

    [JsonProperty("active")]
    public long ActualActive { get; set; }

    [JsonIgnore]

    public int Count { get; set; }
  }
}
