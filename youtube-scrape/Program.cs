using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

class Program
{
    static async Task Main(string[] args)
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        
        string token = "YOUR_TOKEN";
        string[] searchParams = { "scraping+proxy", "web+scraping+api", "bypass+cloudflare+webscraping" };
        var results = new List<string[]>();
        var tasks = new List<Task>();

        using var client = new HttpClient();

        foreach (var searchParam in searchParams)
        {
            var task = Task.Run(async () =>
            {
                string continuationToken = null;
                for (int page = 0; page < 5; page++)
                {
                    var response = await FetchYoutubeData(client, token, searchParam, continuationToken);
                    var jsonData = continuationToken == null ? ParseInitialData(response) : JObject.Parse(response);
                    lock (results)
                    {
                        results.AddRange(ParseVideoData(jsonData, searchParam));
                    }
                    continuationToken = jsonData.SelectTokens("..continuationCommand.token").FirstOrDefault()?.ToString();

                    if (continuationToken == null) break;
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        WriteResultsToCsv("youtube_results.csv", results);
        
        stopwatch.Stop();
        Console.WriteLine($"Toplam çalışma süresi: {stopwatch.Elapsed.TotalMinutes} dakika");
        
    }

    static async Task<string> FetchYoutubeData(HttpClient client, string token, string searchParam, string continuationToken)
    {
        string url = continuationToken == null
            ? $"https://api.scrape.do?token={token}&url={WebUtility.UrlEncode($"https://www.youtube.com/results?search_query={searchParam}")}"
            : "https://www.youtube.com/youtubei/v1/search?prettyPrint=false";

        var request = new HttpRequestMessage(continuationToken == null ? HttpMethod.Get : HttpMethod.Post, url);

        if (continuationToken != null)
        {
            string requestBody = CreateRequestBody(continuationToken);
            request.Content = new StringContent(requestBody, null, "application/json");
        }

        var response = await client.SendAsync(request);
        return await response.Content.ReadAsStringAsync();
    }

    static JObject ParseInitialData(string content)
    {
        var match = Regex.Match(content, @"ytInitialData\s*=\s*(\{.*?\});");
        if (match.Success) return JObject.Parse(match.Groups[1].Value);

        throw new InvalidOperationException("ytInitialData bulunamadı.");
    }

    static IEnumerable<string[]> ParseVideoData(JObject jsonData, string searchParam)
    {
        return jsonData.SelectTokens("..videoRenderer").Select(vr => new string[]
        {
            searchParam,
            EscapeCsvField(vr["title"]?["runs"]?.First()?["text"]?.ToString()),
            $"https://www.youtube.com/watch?v={vr["videoId"]}"
        });
    }

    static void WriteResultsToCsv(string fileName, IEnumerable<string[]> results)
    {
        using var writer = new StreamWriter(fileName);
        writer.WriteLine("SearchParam,Title,VideoURL");

        foreach (var row in results)
        {
            writer.WriteLine(string.Join(",", row));
        }
    }

    static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field)) return string.Empty;

        if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
        {
            field = field.Replace("\"", "\"\"");
            return $"\"{field}\"";
        }

        return field;
    }
    private static string CreateRequestBody(string continuationToken)
{
    return
        "{" +
        "\"context\":{" +
        "\"client\":{" +
        "\"hl\":\"tr\"," +
        "\"gl\":\"TR\"," +
        "\"remoteHost\":\"176.40.242.236\"," +
        "\"deviceMake\":\"Apple\"," +
        "\"deviceModel\":\"\"," +
        "\"visitorData\":\"CgtONXYxR3FRY0dQMCjH9tK1BjIKCgJUUhIEGgAgHA%3D%3D\"," +
        "\"userAgent\":\"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/127.0.0.0 Safari/537.36,gzip(gfe)\"," +
        "\"clientName\":\"WEB\"," +
        "\"clientVersion\":\"2.20240807.01.00\"," +
        "\"osName\":\"Macintosh\"," +
        "\"osVersion\":\"10_15_7\"," +
        "\"originalUrl\":\"https://www.youtube.com/results?search_query=scraping+proxy\"," +
        "\"screenPixelDensity\":2," +
        "\"platform\":\"DESKTOP\"," +
        "\"clientFormFactor\":\"UNKNOWN_FORM_FACTOR\"," +
        "\"configInfo\":{" +
        "\"appInstallData\":\"CMf20rUGEP-IsQUQ1YiwBRCPlLEFEMr5sAUQ2qCxBRCNzLAFEJajsQUQ3ej-EhDT4a8FEOHssAUQ65OuBRCI468FEKi3sAUQppqwBRDrmbEFEOPRsAUQ9quwBRCPxLAFENuvrwUQ0I2wBRC6-LAFEKiTsQUQxJKxBRCSnbEFEI_GsAUQ1KGvBRCvqrEFEKaSsQUQooGwBRDhp7EFEMnXsAUQ4tSuBRCZmLEFEPSrsAUQt--vBRCSwP8SEKKdsQUQvbauBRDJ968FENfprwUQt-r-EhCa8K8FEKaTsQUQ6-j-EhCq2LAFEJSJsQUQvYqwBRCU_rAFEND6sAUQkJKxBRCd0LAFEInorgUQzdewBRCW0LAFEKiSsQUQnaawBRDHn7EFEN_1sAUQg7n_EhD8hbAFEOX0sAUQqJqwBRCIh7AFENnJrwUQ74ixBRDuoq8FEIvPsAUQ1t2wBRC9mbAFEI2UsQUQsdywBRDapbEFEJiNsQUQyeawBRDM364FEJaVsAUQ6sOvBRDvzbAFEOG8_xIQpcL-EhDGpLEFELDusAUQtrH_EiogQ0FNU0VoVUpvTDJ3RE5Ia0J2UHQ4UXVCOXdFZEJ3PT0%3D\"" +
        "}," +
        "\"userInterfaceTheme\":\"USER_INTERFACE_THEME_DARK\"," +
        "\"timeZone\":\"Europe/Istanbul\"," +
        "\"browserName\":\"Chrome\"," +
        "\"browserVersion\":\"127.0.0.0\"," +
        "\"acceptHeader\":\"text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7\"," +
        "\"deviceExperimentId\":\"ChxOelF3TURjME5qQXdNalExTXpVeU9EUTBOZz09EMf20rUGGMf20rUG\"," +
        "\"screenWidthPoints\":2056," +
        "\"screenHeightPoints\":1073," +
        "\"utcOffsetMinutes\":180," +
        "\"connectionType\":\"CONN_CELLULAR_4G\"," +
        "\"memoryTotalKbytes\":\"8000000\"," +
        "\"mainAppWebInfo\":{" +
        "\"graftUrl\":\"https://www.youtube.com/results?search_query=scraping+proxy\"," +
        "\"pwaInstallabilityStatus\":\"PWA_INSTALLABILITY_STATUS_CAN_BE_INSTALLED\"," +
        "\"webDisplayMode\":\"WEB_DISPLAY_MODE_BROWSER\"," +
        "\"isWebNativeShareAvailable\":false" +
        "}}," +
        "\"user\":{\"lockedSafetyMode\":false}," +
        "\"request\":{\"useSsl\":true,\"internalExperimentFlags\":[],\"consistencyTokenJars\":[]}," +
        "\"clickTracking\":{\"clickTrackingParams\":\"CAEQt6kLGAIiEwjSkcGTtOWHAxUaQ3oFHStnCIk=\"}," +
        "\"adSignalsInfo\":{" +
        "\"params\":[{\"key\":\"dt\",\"value\":\"1723120456077\"},{\"key\":\"flash\",\"value\":\"0\"},{\"key\":\"frm\",\"value\":\"0\"},{\"key\":\"u_tz\",\"value\":\"180\"},{\"key\":\"u_his\",\"value\":\"2\"},{\"key\":\"u_h\",\"value\":\"1329\"},{\"key\":\"u_w\",\"value\":\"2056\"},{\"key\":\"u_ah\",\"value\":\"1205\"},{\"key\":\"u_aw\",\"value\":\"2056\"},{\"key\":\"u_cd\",\"value\":\"30\"},{\"key\":\"bc\",\"value\":\"31\"},{\"key\":\"bih\",\"value\":\"1073\"},{\"key\":\"biw\",\"value\":\"2041\"},{\"key\":\"brdim\",\"value\":\"0,44,0,44,2056,44,2056,1194,2056,1073\"},{\"key\":\"vis\",\"value\":\"1\"},{\"key\":\"wgl\",\"value\":\"true\"},{\"key\":\"ca_type\",\"value\":\"image\"}]," +
        "\"bid\":\"ANyPxKruIczz4DKsjI0r6cEPncCVVU88Q3K3lGpiddk_uoGrm4omg2uBCg0_lROkBwWC14mdTwhOC1XGASQy3OM31YgYPlF46A\"}}," +
        "\"continuation\":\"" + continuationToken + "\"" +
        "}";
}
}
