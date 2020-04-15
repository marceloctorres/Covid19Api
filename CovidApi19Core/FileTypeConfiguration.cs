using System;

using Newtonsoft.Json;

namespace MarceloCTorres.CovidApi19.Core
{
  public class FileTypeConfiguration
  {
    [JsonProperty("sourceType")]
    public SourceTypes SourceType { get; set; }

    [JsonProperty("repoFileName")]
    public string RepoFileName { get; set; }

    [JsonProperty("repoRelativeFilePath")]
    public string RepoRelativeFilePath { get; set; }

    [JsonProperty("sourceFileName")]
    public string SourceFileName { get; set; }

    [JsonProperty("targetFileName")]
    public string TargetFileName { get; set; }

    [JsonProperty("lastUpdate")]
    public DateTime? LastUpdate { get; set; }
  }
}
