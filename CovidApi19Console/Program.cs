using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

using MarceloCTorres.Covid19Api.Core;
using MarceloCTorres.Covid19Api.Core.Http;

namespace Covid19ApiConsole
{
  internal class Program
  {
    private static readonly ProcessSourceInfo process = new ProcessSourceInfo();
    private static readonly string basePath = @"..\..\..\";
    private static readonly string configurationFileName = "configuration.json";
    private static string configurationFilePath;
    private static bool isCountriesUpdated = true;
    private static bool isDailyReporUptated = true;
    private static bool isConfirmedTimeSeriesUpdated = true;
    private static bool isDeathsTimeSeriesUpdated = true;
    private static bool isRecoveredTimeSeriesUpdated = true;
    private static string actualPath;

    private static void GetActualPath()
    {
#if DEBUG
      var processDir = Directory.GetCurrentDirectory();
      actualPath = Directory.GetParent(basePath).FullName;
#else
      actualPath = Directory.GetCurrentDirectory();
#endif
    }

    private static void InitDirectories()
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

    private static void ProcessCountries()
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.Countries);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);
      var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

      if(isCountriesUpdated || !File.Exists(targetPath))
      {
        Trace.TraceInformation(sourcePath);
        process.ProcessCountries(sourcePath);

        Trace.TraceInformation(targetPath);
        process.WriteRegions(targetPath);
      }
    }

    private static void ReadCountries()
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.Countries);
      var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

      Trace.TraceInformation(targetPath);
      process.ReadRegions(targetPath);
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ProcessDialyReports()
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

        Trace.TraceInformation(sourcePath);
        process.ProcessDailyReports(sourcePath);

        Trace.TraceInformation(targetPath);
        process.WriteDailyReports(targetPath);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ProcessConfirmedTimeSeries()
    {
      if(process.Regions == null)
      {
        ReadCountries();
      }
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesConfirmed);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      Trace.TraceInformation(sourcePath);
      process.ProccessTimeSeriesData(sourcePath, process.UpdateConfirmed);
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ProcessDeathsTimeSeries()
    {
      if(process.Regions == null)
      {
        ReadCountries();
      }
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesDeaths);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      Trace.TraceInformation(sourcePath);
      process.ProccessTimeSeriesData(sourcePath, process.UpdateDeaths);
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ProcessRecoveredTimeSeries()
    {
      if(process.Regions == null)
      {
        ReadCountries();
      }
      var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesRecovered);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      Trace.TraceInformation(sourcePath);
      process.ProccessTimeSeriesData(sourcePath, process.UpdateRecovered);
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ProcessTimeSeriesData()
    {
      if(isConfirmedTimeSeriesUpdated || isDeathsTimeSeriesUpdated || isRecoveredTimeSeriesUpdated)
      {
        var fileTypeConfiguration = process.FindFileTypeConfiguration(SourceTypes.TimeSeriesConsolidated);
        var targetPath = Path.Combine(process.Configuration.TargetBasePath, fileTypeConfiguration.TargetFileName);

        ProcessConfirmedTimeSeries();
        ProcessDeathsTimeSeries();
        ProcessRecoveredTimeSeries();
        process.ConsolidateTimeSeries();

        Trace.TraceInformation(targetPath);
        process.WriteTimeSeriesData(targetPath);
      }
    }

    private static Configuration GetConfiguration()
    {
      GetActualPath();
      Trace.TraceInformation(actualPath);

      configurationFilePath = Path.Combine(actualPath, configurationFileName);
      if(File.Exists(configurationFilePath))
      {
        return Configuration.Read(configurationFilePath);
      }
      throw new ApplicationException("Archivo de configuración no encontrado");
    }

    private async static void GetHttpRepoFiles(SourceTypes sourceTypes, bool isUpdated, bool findLast = false)
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(sourceTypes);
      var repoPath = findLast ?
        Path.Combine(process.Configuration.RepoBaseUrl, fileTypeConfiguration.RepoRelativeFilePath) :
        Path.Combine(process.Configuration.RepoBaseUrl, fileTypeConfiguration.RepoRelativeFilePath, fileTypeConfiguration.RepoFileName);

      var (result, dateTime) = await HttpServiceClient.GetAsync(new Uri(repoPath));

      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      if(findLast)
      {
        var fileNameToSearch = $"{DateTime.Today:MM-dd-yyyy}.csv";
        Trace.TraceInformation(fileNameToSearch);
        repoPath = Directory.GetFiles(repoPath)
                            .Where(f => f.EndsWith(".csv"))
                            .OrderBy(f => f)
                            .LastOrDefault();
      }
      Trace.TraceInformation(repoPath);
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
    /// <param name="sourceTypes"></param>
    /// <param name="isUpdated"></param>
    /// <param name="findLast"></param>
    private static void GetRepoFiles(SourceTypes sourceTypes, ref bool isUpdated, bool findLast = false)
    {
      var fileTypeConfiguration = process.FindFileTypeConfiguration(sourceTypes);
      var repoPath = findLast ?
        Path.Combine(process.Configuration.RepoBasePath, fileTypeConfiguration.RepoRelativeFilePath) :
        Path.Combine(process.Configuration.RepoBasePath, fileTypeConfiguration.RepoRelativeFilePath, fileTypeConfiguration.RepoFileName);
      var sourcePath = Path.Combine(process.Configuration.SourceBasePath, fileTypeConfiguration.SourceFileName);

      if(findLast)
      {
        var fileNameToSearch = $"{DateTime.Today:MM-dd-yyyy}.csv";
        Trace.TraceInformation(fileNameToSearch);
        repoPath = Directory.GetFiles(repoPath)
                            .Where(f => f.EndsWith(".csv"))
                            .OrderBy(f => f)
                            .LastOrDefault();
      }
      Trace.TraceInformation(repoPath);
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
    private static void GetHttpCountryRepoFiles() => GetHttpRepoFiles(SourceTypes.Countries, isCountriesUpdated);

    /// <summary>
    /// 
    /// </summary>
    private static void GetCountryRepoFiles() => GetRepoFiles(SourceTypes.Countries, ref isCountriesUpdated);

    /// <summary>
    /// 
    /// </summary>
    private static void GetDialyReportRepoFiles() => GetRepoFiles(SourceTypes.DialyReport, ref isDailyReporUptated, true);

    /// <summary>
    /// 
    /// </summary>
    private static void GetConfirmedRepoFiles() => GetRepoFiles(SourceTypes.TimeSeriesConfirmed, ref isConfirmedTimeSeriesUpdated);

    /// <summary>
    /// 
    /// </summary>
    private static void GetDeathsRepoFiles() => GetRepoFiles(SourceTypes.TimeSeriesDeaths, ref isDeathsTimeSeriesUpdated);

    /// <summary>
    /// 
    /// </summary>
    private static void GetRecoveredRepoFiles() => GetRepoFiles(SourceTypes.TimeSeriesRecovered, ref isRecoveredTimeSeriesUpdated);

    /// <summary>
    /// 
    /// </summary>
    private static void GetRepoFiles()
    {
      GetCountryRepoFiles();
      GetDialyReportRepoFiles();
      GetConfirmedRepoFiles();
      GetDeathsRepoFiles();
      GetRecoveredRepoFiles();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="command"></param>
    /// <param name="workingDirectory"></param>
    /// <returns></returns>
    private static string CommandOutput(string command, string workingDirectory = null)
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

        Process proc = new Process
        {
          StartInfo = procStartInfo
        };
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
    private static void PullCSSERepo()
    {
      var commitMessage = $"Protegido por Covid19ApiConsole en '{DateTime.Now:yyyy-MM-dd HH:mm:ss}'";
      Trace.TraceInformation($"WorkingDirectory: {process.Configuration.RepoBasePath}");

      string[] cmds = new string[]
      {
        "git pull origin master",
        "git pull upstream master",
        "git add .",
        $"git commit -m \"{commitMessage}\"",
        "git push origin master"
      };
      RunCommandLine(cmds, process.Configuration.RepoBasePath);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cmds"></param>
    static void RunCommandLine(string[] cmds, string workingDirectory = null)
    {
      foreach(var cmd in cmds)
      {
        Trace.TraceInformation(cmd);
        var result = CommandOutput(cmd, workingDirectory);
        Trace.TraceInformation($"\n{result}");
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void PushCovid19ApiRepo()
    {
      var commitMessage = $"Protegido por Covid19ApiConsole en '{DateTime.Now:yyyy-MM-dd HH:mm:ss}'";
      Trace.TraceInformation($"WorkingDirectory: '{process.Configuration.PublishBasePath}'");

      string[] cmds = new string[]
      {
        $"git pull origin master",
        $"git add .",
        $"git commit -m \"{commitMessage}\"",
        $"git push origin master"
      };
      RunCommandLine(cmds, process.Configuration.PublishBasePath);

    }

    static void UpdateArcGIS()
    {
      Trace.TraceInformation($"{process.Configuration.PythonExecutionEnvPath} \"{process.Configuration.PythonScriptPath}\"");
      if(isDailyReporUptated || isConfirmedTimeSeriesUpdated ||  isDeathsTimeSeriesUpdated || isRecoveredTimeSeriesUpdated)
      {
        Trace.TraceInformation("Se actualizará ArcGIS.");
        RunPythonScript();
      }
      else
      {
        Trace.TraceInformation("No se actualizará ArcGIS.");
      }
    }

    static void RunPythonScript()
    {
      //C:\Progra~1\ArcGIS\Pro\bin\Python\scripts\propy.bat "C:\Users\mtorres\OneDrive - Esri NOSA\Documentos\ArcGIS\Projects\MyProject\covid19.py"
      string[] cmds = new string[]
      {
        //  $"C:\\Progra~1\\ArcGIS\\Pro\\bin\\Python\\scripts\\propy.bat \"C:\\Users\\mtorres\\OneDrive - Esri NOSA\\Documentos\\ArcGIS\\Projects\\MyProject\\covid19.py\""
        $"{process.Configuration.PythonExecutionEnvPath} \"{process.Configuration.PythonScriptPath}\""
      };
      RunCommandLine(cmds);
    }

  /// <summary>
  /// 
  /// </summary>
  private static void PublishFiles()
    {
      var outputDir = Path.Combine(process.Configuration.PublishBasePath, "docs");
      var inputDir = process.Configuration.TargetBasePath;

      Trace.TraceInformation($"Copy files from {inputDir} to {outputDir}");
      var files = Directory.GetFiles(inputDir);
      foreach(var file in files)
      {
        var filename = Path.GetFileName(file);
        var newFile = Path.Combine(outputDir, filename);
        File.Copy(file, newFile, true);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void ProcessSourceFiles()
    {
      ProcessCountries();
      ProcessDialyReports();
      ProcessTimeSeriesData();
    }

    /// <summary>
    /// /
    /// </summary>
    private static void InitTracing()
    {
      var textListener = new TextWriterTraceListener("covid19apiconsole.log", "text")
      {
        TraceOutputOptions = TraceOptions.None
      };

      var consoleListener = new ConsoleTraceListener(true)
      {
        Name = "console",
        TraceOutputOptions = TraceOptions.None
      };
      Trace.Listeners.Add(textListener);
      Trace.Listeners.Add(consoleListener);
      Trace.AutoFlush = true;
    }

    private static void Main(string[] args)
    {
      try
      {
        InitTracing();

        Trace.TraceInformation("********************************************************");
        Trace.TraceInformation($"Iniciando ejecución: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Trace.Flush();

        process.Configuration = GetConfiguration();

        InitDirectories();
        PullCSSERepo();
        GetRepoFiles();
        ProcessSourceFiles();
        PublishFiles();
        PushCovid19ApiRepo();
        UpdateArcGIS();
      }
      catch(Exception ex)
      {
        Trace.TraceError(ex.Message);
        Trace.TraceError(ex.StackTrace);
      }
      finally 
      { 
        Trace.TraceInformation($"Terminando ejecución: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Trace.TraceInformation("^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^");
        Trace.Flush();
      }
    }
  }
}
