using System.Collections.Generic;
using System.Globalization;

using Newtonsoft.Json;

namespace MarceloCTorres.Covid19Api.Core
{
  /// <summary>
  /// 
  /// </summary>
  public class CountryRegionCultureName
  {
    [JsonProperty("culture")]
    public string Culture { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }
  }

  /// <summary>
  /// 
  /// </summary>
  public class CountryRegion
  {
    [JsonProperty("threeLetterIsoCode")]
    public string ThreeLetterIsoCode { get; set; }

    [JsonProperty("twoLetterIsoCode")]
    public string TwoLetterIsoCode { get; set; }

    [JsonProperty("countryRegion")]
    public string Name { get; set; }

    [JsonProperty("latitude")]
    public double? Latitude { get; set; }

    [JsonProperty("longitude")]
    public double? Longitude { get; set; }

    [JsonProperty("population")]
    public long? Population { get; set; }

    [JsonProperty("cultureNames")]
    public List<CountryRegionCultureName> CultureNames { get; set; }

    public CountryRegion() => CultureNames = new List<CountryRegionCultureName>();

    public void SetCurrentCultureName()
    {
      if(!string.IsNullOrEmpty(TwoLetterIsoCode))
      {
        try
        {
          var region = new RegionInfo(TwoLetterIsoCode);
          CultureNames.Add(new CountryRegionCultureName()
          {
            Culture = CultureInfo.CurrentCulture.Parent.Name,
            Name = region.DisplayName
          });
        }
        catch
        {
          CultureNames.Add(new CountryRegionCultureName()
          {
            Culture = CultureInfo.CurrentCulture.Parent.Name,
            Name = string.Empty
          });
        }
      }
    }
  }
}
