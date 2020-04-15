using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CsvHelper;
using Newtonsoft.Json;

namespace MarceloCTorres.CovidApi19.Core
{
  /// <summary>
  /// 
  /// </summary>
  public class ProcessSourceInfo
  {
    private readonly NumberFormatInfo numberFormat = CultureInfo.InvariantCulture.NumberFormat;
    private readonly DateTimeFormatInfo dateFormat = CultureInfo.InvariantCulture.DateTimeFormat;

    /// <summary>
    /// 
    /// </summary>
    public List<CountryRegion> Regions { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public List<DailyReport> DailyReports { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public List<CountryRegionTimeSeries> TimeSeries { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public Configuration Configuration { get; set; }

    public ProcessSourceInfo()
    {

    }

    public FileTypeConfiguration FindFileTypeConfiguration(SourceTypes type)
    {
      return Configuration.FilesConfiguration.Where(f => f.SourceType == type).FirstOrDefault();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private CountryRegion FindCountry(string name)
    {
      if(Regions != null)
      {
        return Regions.Where(r => r.Name == name).FirstOrDefault();
      }
      return null;
    }

    private CountryRegionTimeSeries FindCountryTimeSeries(string name, string province)
    {
      if(TimeSeries != null)
      {
        return TimeSeries.Where(t => t.CountryRegion.Name == name && t.ProvinceState == province).FirstOrDefault();
      }
      return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputFile"></param>
    public void WriteRegions(string outputFile)
    {
      var json = JsonConvert.SerializeObject(Regions, Formatting.Indented);
      File.WriteAllText(outputFile, json);
    }

    public void ReadRegions(string inputFile)
    {
      var json = File.ReadAllText(inputFile);
      Regions = JsonConvert.DeserializeObject<List<CountryRegion>>(json);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outpuFile"></param>
    public void WriteDailyReports(string outputFile)
    {
      var json = JsonConvert.SerializeObject(DailyReports, Formatting.Indented);
      File.WriteAllText(outputFile, json);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputFile"></param>
    public void WriteTimeSeriesData(string outputFile)
    {
      var json = JsonConvert.SerializeObject(TimeSeries, Formatting.Indented);
      File.WriteAllText(outputFile, json);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public void ProcessCountries(string filePath)
    {
      var regions = new List<CountryRegion>();
      using var reader = new StreamReader(filePath);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
      csv.Read();
      csv.ReadHeader();
      while(csv.Read())
      {
        var province = csv.GetField(6);
        if(string.IsNullOrEmpty(province))
        {
          var name = csv.GetField(7);
          var iso2 = csv.GetField(1);
          var iso3 = name == "Diamond Princess" ? "DPS" :
            name == "MS Zaandam" ? "MSZ" : csv.GetField(2);

          double? lat = csv.GetField<double>(8);
          double? lon = csv.GetField<double>(9);
          long? pop = null;
          if(!string.IsNullOrEmpty(csv.GetField(11)))
          {
            pop = csv.GetField<long>(11);
          }

          var country = new CountryRegion()
          {
            TwoLetterIsoCode = iso2,
            ThreeLetterIsoCode = iso3,
            Name = name,
            Latitude = lat,
            Longitude = lon,
            Population = pop
          };
          country.SetCurrentCultureName();
          regions.Add(country);
        }
      }
      Regions = regions.OrderBy(r => r.ThreeLetterIsoCode).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public void ProcessDailyReports(string filePath)
    {
      var dailyReports = new List<DailyReport>();
      using var reader = new StreamReader(filePath);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
      csv.Read();
      csv.ReadHeader();
      while(csv.Read())
      {
        DateTime lastUpdate = csv.GetField<DateTime>(4);
        long confirmed = csv.GetField<long>(7);
        long deaths = csv.GetField<long>(8);
        long recovered = csv.GetField<long>(9);
        long active = csv.GetField<long>(10);
        var country = csv.GetField(3);

        var dialyReport = new DailyReport
        {
          LastUpdate = lastUpdate,
          CountryRegion = FindCountry(country),
          DailyData = new DailyData()
          {
            Date = lastUpdate,
            Confirmed = confirmed,
            Deaths = deaths,
            Recovered = recovered,
            Active = active,
            ActualActive = confirmed - deaths - recovered
          }
        };
        dailyReports.Add(dialyReport);
      }

      var group = dailyReports
                  .GroupBy(
                    d => d.CountryRegion,
                    d => d.DailyData,
                    (k, g) => new DailyReport
                    {
                      CountryRegion = k,
                      LastUpdate = g.Select(d => d.Date).First(),
                      DailyData = new DailyData
                      {
                        Date = g.Select(d => d.Date).First().Date,
                        Confirmed = g.Sum(d => d.Confirmed),
                        Recovered = g.Sum(d => d.Recovered),
                        Deaths = g.Sum(d => d.Deaths),
                        Active = g.Sum(d => d.Active),
                        ActualActive = g.Sum(d => d.ActualActive)
                      }
                    });
      DailyReports = group.OrderBy(d => d.CountryRegion.ThreeLetterIsoCode).ToList();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public void UpdateConfirmed(DailyData data, long value)
    {
      data.Confirmed = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public void UpdateDeaths(DailyData data, long value)
    {
      data.Deaths = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="value"></param>
    public void UpdateRecovered(DailyData data, long value)
    {
      data.Recovered = value;
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    public void ProccessTimeSeriesData(string filePath, Action<DailyData, long> updateAction)
    {
      if(TimeSeries == null)
      {
        TimeSeries = new List<CountryRegionTimeSeries>();
      }

      using var reader = new StreamReader(filePath);
      using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
      csv.Read();
      csv.ReadHeader();

      var records = csv.GetRecords<dynamic>();
      foreach(var record in records)
      {
        var dict = (IDictionary<string, object>)record;
        var countryRegion = FindCountry((string)dict["Country/Region"]);
        var provinceState = (string)dict["Province/State"];
        var noDateFields = new string[] {"Province/State", "Country/Region", "Lat", "Long" };

        var timeSeriesItem = FindCountryTimeSeries(countryRegion.Name, provinceState);
        if(timeSeriesItem == null)
        {
          timeSeriesItem = new CountryRegionTimeSeries
          {
            CountryRegion = countryRegion,
            ProvinceState = provinceState
          };
          TimeSeries.Add(timeSeriesItem);
        }

        foreach(var item in dict)
        {
          if(!noDateFields.Contains(item.Key))
          {
            var date = Convert.ToDateTime(item.Key, dateFormat);
            var value = Convert.ToInt64(item.Value, numberFormat);
            var dailyData = timeSeriesItem.TimeSeries.Where(t => t.Date == date).FirstOrDefault();
            if(dailyData == null)
            {
              dailyData = new DailyData()
              {
                Date = Convert.ToDateTime(item.Key, dateFormat),
              };
              timeSeriesItem.TimeSeries.Add(dailyData);
            }
            updateAction?.Invoke(dailyData, value);
          }
        }
      }
    }

    public void ConsolidateTimeSeries()
    {
      TimeSeries = TimeSeries
                    .GroupBy(
                      t => t.CountryRegion,
                      t => t.TimeSeries, 
                      (k, g) =>
                        {
                          var timeSeries = g.SelectMany(d => d, (d, daily) => daily)
                                            .GroupBy(
                                              d => d.Date,
                                              d => d,
                                              (kd, gd) => new DailyData
                                              {
                                                Date = kd,
                                                Count = gd.Count(),
                                                Confirmed = gd.Sum(d => d.Confirmed),
                                                Deaths = gd.Sum(d => d.Deaths),
                                                Recovered = gd.Sum(d => d.Recovered),
                                                Active = gd.Sum(d => d.Active),
                                              })
                                            .ToList();
                          var c = timeSeries.Count();

                          return new CountryRegionTimeSeries
                          {
                            CountryRegion = k,
                            TimeSeries = timeSeries
                          };
                        })
                    .OrderBy(t => t.CountryRegion.ThreeLetterIsoCode)
                    .ToList();
    }
    
  }
}
