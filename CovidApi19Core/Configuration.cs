using System.Collections.Generic;
using System.IO;

using Newtonsoft.Json;

namespace MarceloCTorres.Covid19Api.Core
{
  /// <summary>
  /// 
  /// </summary>
  public class Configuration
  {
    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("repoBaseUrl")]
    public string RepoBaseUrl { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("pythonScriptPath")]
    public string PythonScriptPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("pythonExecutionEnvPath")]
    public string PythonExecutionEnvPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("repoBasePath")]
    public string RepoBasePath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("publishBasePath")]
    public string PublishBasePath { get; set; }


    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public string SourceBasePath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonIgnore]
    public string TargetBasePath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonProperty("filesConfiguration")]
    public List<FileTypeConfiguration> FilesConfiguration { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public Configuration() => FilesConfiguration = new List<FileTypeConfiguration>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="outputFile"></param>
    public void Write(string outputFile)
    {
      var json = JsonConvert.SerializeObject(this, Formatting.Indented);
      File.WriteAllText(outputFile, json);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputFile"></param>
    /// <returns></returns>
    public static Configuration Read(string inputFile)
    {
      if(File.Exists(inputFile))
      {
        var json = File.ReadAllText(inputFile);
        return JsonConvert.DeserializeObject<Configuration>(json);
      }
      return null;
    }
  }
}
