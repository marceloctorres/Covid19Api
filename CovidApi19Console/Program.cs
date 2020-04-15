using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using MarceloCTorres.CovidApi19.Core;

namespace CovidApi19Console
{
  class Program
  {
    static readonly ProcessSourceInfo process = new ProcessSourceInfo();
    static readonly string basePath = @"..\..\..\";
    static readonly string configurationFileName = "configuration.json";
    static string configurationFilePath;
    static bool isCountriesUpdated = true;
    static bool isDailyReporUptated = true;
    static bool isConfirmedTimeSeriesUpdated = true;
    static bool isDeathsTimeSeriesUpdated = true;
    static bool isRecoveredTimeSeriesUpdated = true;

    static string actualPath;

    static void GetActualPath()
    {
#if DEBUG
      var processDir = Directory.GetCurrentDirectory();
      actualPath = Directory.GetParent(basePath).FullName;
#else
      actualPath = Directory.GetCurrentDirectory();
#endif
    }

    static void InitDirectories()
    {
      process.Configuration.SourceBasePath = Path.Combine(actualPath, "Sources");
      process.Configuration.TargetBasePath = Path.Combine(actualPath, "Target");

      if(!Directory.Exists(process.Configuration.SourceBasePath))
      {
        Directory.CreateDirectory(process.Configuration.SourceBasePath);
      }

      if(!Directory.Exists(process.Configuration.TargetBasePath))
      {
        Directory.CreateDirectory(process.Configuration.TargetBasePath);
      }
    }

    static void ProcessCountries()
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.Countries);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);
      var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

      if(isCountriesUpdated || !File.Exists(targetPath))
      {
        Console.WriteLine(sourcePath);
        process.ProcessCountries(sourcePath);

        Console.WriteLine(targetPath);
        process.WriteRegions(targetPath);
      }
    }

    static void ReadCountries()
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.Countries);
      var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

      Console.WriteLine(targetPath);
      process.ReadRegions(targetPath);
    }

    static void ProcessDialyReports()
    {
      if(isDailyReporUptated)
      {
        var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.DialyReport);
        var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);
        var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

        if(process.Regions == null)
        {
          ReadCountries();
        }

        Console.WriteLine(sourcePath);
        process.ProcessDailyReports(sourcePath);

        Console.WriteLine(targetPath);
        process.WriteDailyReports(targetPath);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    static void ProcessConfirmedTimeSeries()
    {
      if(process.Regions == null)
      {
        ReadCountries();
      }
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesConfirmed);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      Console.WriteLine(sourcePath);
      process.ProccessTimeSeriesData(sourcePath, process.UpdateConfirmed);
    }

    /// <summary>
    /// 
    /// </summary>
    static void ProcessDeathsTimeSeries()
    {
      if(process.Regions == null)
      {
        ReadCountries();
      }
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesDeaths);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      Console.WriteLine(sourcePath);
      process.ProccessTimeSeriesData(sourcePath, process.UpdateDeaths);
    }

    /// <summary>
    /// 
    /// </summary>
    static void ProcessRecoveredTimeSeries()
    {
      if(process.Regions == null)
      {
        ReadCountries();
      }
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesRecovered);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      Console.WriteLine(sourcePath);
      process.ProccessTimeSeriesData(sourcePath, process.UpdateRecovered);
    }

    /// <summary>
    /// 
    /// </summary>
    static void ProcessTimeSeriesData()
    {
      if(isConfirmedTimeSeriesUpdated || isDeathsTimeSeriesUpdated || isRecoveredTimeSeriesUpdated)
      {
        var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesConsolidated);
        var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

        ProcessConfirmedTimeSeries();
        ProcessDeathsTimeSeries();
        ProcessRecoveredTimeSeries();
        process.ConsolidateTimeSeries();

        Console.WriteLine(targetPath);
        process.WriteTimeSeriesData(targetPath);
      }
    }

    static Configuration GetConfiguration()
    {
      GetActualPath();
      Console.WriteLine(actualPath);

      configurationFilePath = Path.Combine(actualPath, configurationFileName);
      if(File.Exists(configurationFilePath))
      {
        return Configuration.Read(configurationFilePath);
      }
      else
      {
        var configuration = new Configuration
        {
          RepoBasePath = @"C:\Users\mtorres\OneDrive - Esri NOSA\Documentos\GitHub\COVID-19",
          FilesConfiguration =
          {
            new FileTypeConfiguration
            {
              SourceType = SourceTypes.Countries,
              RepoFileName = "",
              RepoRelativeFilePath = "",
              SourceFileName = "countries.csv",
              TargetFileName = "countries.json",
            },
            new FileTypeConfiguration
            {
              SourceType = SourceTypes.DialyReport,
              RepoFileName = "",
              RepoRelativeFilePath = "",
              SourceFileName = "daily_reports.csv",
              TargetFileName = "daily_reports.csv",
            },
            new FileTypeConfiguration
            {
              SourceType = SourceTypes.TimeSeriesConfirmed,
              RepoFileName = "time_series_covid19_confirmed_global.csv",
              RepoRelativeFilePath = @"\csse_covid_19_data\csse_covid_19_time_series",
              SourceFileName = "time_series_confirmed.csv",
              TargetFileName = "time_series_full.json",
            },
            new FileTypeConfiguration
            {
              SourceType = SourceTypes.TimeSeriesDeaths,
              RepoFileName = "time_series_covid19_recovered_global.csv",
              RepoRelativeFilePath = @"\csse_covid_19_data\csse_covid_19_time_series",
              SourceFileName = "time_series_deaths.csv",
              TargetFileName = "time_series_full.json",
            },
            new FileTypeConfiguration
            {
              SourceType = SourceTypes.TimeSeriesRecovered,
              RepoFileName = "time_series_covid19_deaths_global.csv",
              RepoRelativeFilePath = @"\csse_covid_19_data\csse_covid_19_time_series",
              SourceFileName = "time_series_recovered.csv",
              TargetFileName = "time_series_full.json",
            },
            new FileTypeConfiguration
            {
              SourceType = SourceTypes.TimeSeriesConsolidated,
              SourceFileName = "time_series_full.json",
              TargetFileName = "time_series.json",
            },
          }
        };
        configuration.Write(configurationFilePath);
        return configuration;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceTypes"></param>
    /// <param name="isUpdated"></param>
    /// <param name="findLast"></param>
    static void GetRepoFiles(SourceTypes sourceTypes, ref bool isUpdated, bool findLast = false)
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(sourceTypes);
      var repoPath = findLast ?
        Path.Combine(process.Configuration.RepoBasePath, fileTypeConfiguration.RepoRelativeFilePath) :
        Path.Combine(process.Configuration.RepoBasePath, fileTypeConfiguration.RepoRelativeFilePath, fileTypeConfiguration.RepoFileName);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      if(findLast)
      {
        repoPath = Directory.GetFiles(repoPath)
                            .Where(f => f.EndsWith(".csv"))
                            .OrderBy(f => File.GetLastWriteTime(f))
                            .LastOrDefault();
      }
      isUpdated = fileTypeConfiguration.LastUpdate != null ?
        File.GetLastWriteTime(repoPath) > fileTypeConfiguration.LastUpdate :
        true;
      if(isUpdated)
      {
        fileTypeConfiguration.LastUpdate = File.GetLastWriteTime(repoPath);
        process.Configuration.Write(configurationFilePath);
        File.Copy(repoPath, sourcePath, true);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    static void GetCountryRepoFiles()
    {
      GetRepoFiles(SourceTypes.Countries, ref isCountriesUpdated);
    }

    /// <summary>
    /// 
    /// </summary>
    static void GetDialyReportRepoFiles()
    {
      GetRepoFiles(SourceTypes.DialyReport, ref isDailyReporUptated, true);
    }

    /// <summary>
    /// 
    /// </summary>
    static void GetConfirmedRepoFiles()
    {
      GetRepoFiles(SourceTypes.TimeSeriesConfirmed, ref isConfirmedTimeSeriesUpdated);
    }

    /// <summary>
    /// 
    /// </summary>
    static void GetDeathsRepoFiles()
    {
      GetRepoFiles(SourceTypes.TimeSeriesDeaths, ref isDeathsTimeSeriesUpdated);
    }

    /// <summary>
    /// 
    /// </summary>
    static void GetRecoveredRepoFiles()
    {
      GetRepoFiles(SourceTypes.TimeSeriesRecovered, ref isRecoveredTimeSeriesUpdated);
    }

    /// <summary>
    /// 
    /// </summary>
    static void GetRepoFiles()
    {
      GetCountryRepoFiles();
      GetDialyReportRepoFiles();
      GetConfirmedRepoFiles();
      GetDeathsRepoFiles();
      GetRecoveredRepoFiles();
    }

    static string CommandOutput(string command, string workingDirectory = null)
    {
      try
      {
        ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd", "/c " + command);

        procStartInfo.RedirectStandardError = procStartInfo.RedirectStandardInput = procStartInfo.RedirectStandardOutput = true;
        procStartInfo.UseShellExecute = false;
        procStartInfo.CreateNoWindow = true;
        if(null != workingDirectory)
        {
          procStartInfo.WorkingDirectory = workingDirectory;
        }

        Process proc = new Process();
        proc.StartInfo = procStartInfo;
        proc.Start();

        StringBuilder sb = new StringBuilder();
        proc.OutputDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
          sb.AppendLine(e.Data);
        };
        proc.ErrorDataReceived += delegate (object sender, DataReceivedEventArgs e)
        {
          sb.AppendLine(e.Data);
        };

        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();
        proc.WaitForExit();
        return sb.ToString();
      }
      catch(Exception objException)
      {
        return $"Error in command: {command}, {objException.Message}";
      }
    }

    /// <summary>
    /// 
    /// </summary>
    static void RefreshRepo()
    {
      var cmd = @"git pull upstream master";
      var result = CommandOutput(cmd, process.Configuration.RepoBasePath);
      Console.WriteLine(cmd);
      Console.WriteLine(result);
    }

    static void PushRepo()
    {
      var commitMessage = $"Protegido por Covid119ApiConsole en '{DateTime.Now:yyyy-MM-dd HH:mm:ss}'";
      string[] cmds = new string[]
      {
        $"git pull origin master",
        $"git add .",
        $"git commit -m \"{commitMessage}\"",
        $"git push origin master"
      };
      foreach(var cmd in cmds)
      {
        var result = CommandOutput(cmd, process.Configuration.PublishBasePath);
        Console.WriteLine(cmd);
        Console.WriteLine(result);
      }
    }

    static void PublishFiles()
    {
      var outputDir = Path.Combine(process.Configuration.PublishBasePath, "docs");
      var inputDir = process.Configuration.TargetBasePath;

      var files = Directory.GetFiles(inputDir);
      foreach(var file in files)
      {
        var filename = Path.GetFileName(file);
        var newFile = Path.Combine(outputDir, filename);
        File.Copy(file, newFile, true);
      }

    }

    static void ProcessSourceFiles()
    {
      ProcessCountries();
      ProcessDialyReports();
      ProcessTimeSeriesData();
    }


    static void Main(string[] args)
    {
      try
      {
        process.Configuration = GetConfiguration();
        InitDirectories();

        RefreshRepo();
        GetRepoFiles();
        ProcessSourceFiles();
        PublishFiles();
        PushRepo();
      }
      catch(Exception ex)
      {
        Console.WriteLine(ex.Message);
        Console.WriteLine(ex.StackTrace);
        Console.Write("Press any key...");
        Console.ReadKey();
      }

    }
  }
}
