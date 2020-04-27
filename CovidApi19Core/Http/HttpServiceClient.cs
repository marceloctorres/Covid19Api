using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Newtonsoft.Json;
using System.Diagnostics;

namespace MarceloCTorres.Covid19Api.Core.Http
{
  public class HttpServiceClient
  {
    /// <summary>
    /// 
    /// </summary>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    private static async Task<(string result, DateTime dateTime)> SendAsync(HttpMethod method, Uri uri)
    {
      try
      {
        using var client = new HttpClient();
        var message = new HttpRequestMessage(method, uri);
        var response = await client.SendAsync(message);
        var dateTime = response.Headers.Date.Value.DateTime;
        var text = await response.Content.ReadAsStringAsync();

        return (result: text, dateTime);
      }
      catch(Exception ex)
      {
        Debug.WriteLine(ex.Message);
      }
      return (result: string.Empty, dateTime: DateTime.MinValue);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="method"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    private static async Task<T> SendAsync<T>(HttpMethod method, Uri uri)
    {
      using var client = new HttpClient();
      var message = new HttpRequestMessage(method, uri);
      var response = await client.SendAsync(message);
      var text = await response.Content.ReadAsStringAsync();
      T t = JsonConvert.DeserializeObject<T>(text);
      return t;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static async Task<(string result, DateTime dateTime)> GetAsync(Uri uri)
    {
      return await SendAsync(HttpMethod.Get, uri);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="uri"></param>
    /// <returns></returns>
    public static async Task<T> GetAsync<T>(Uri uri)
    {
      return await SendAsync<T>(HttpMethod.Get, uri);
    }
  }
}
