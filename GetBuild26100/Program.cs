using System.Runtime.CompilerServices;

namespace GetBuild26100;

using System.Net.Http.Json;

class Program
{
    private static HttpClient http = new()
    {
        BaseAddress = new Uri("https://api.uupdump.net/")
    };

    static async Task Main(string[] args)
    {
        try
        {
            //const string FetchUrl = "fetchupd.php?build=26100";
            //FetchLatest.Root? Latest = await ApiRequestLatest(FetchUrl);
            
            const string listUrl = "listid.php?search=26100";
            listid.Root? build = await ApiRequestList(listUrl);
            int i = 0;

            var Latest = build.response.builds;
            
            
            foreach (var data in build.response.builds)
            {
                if (!data.Value.title.Contains("Insider") 
                    && data.Value.build.StartsWith("26100") 
                    && data.Value.title.Contains("Windows 11"))
                {
                    Console.WriteLine($"List Build [{++i}]");
                    Console.WriteLine("Titel: \t\t\t" + data.Value.title);
                    Console.WriteLine("Architekur: \t\t" + data.Value.arch);
                    var createdDate = DateTimeOffset.FromUnixTimeSeconds(data.Value.created).Date;
                    Console.WriteLine("Erstellt: \t\t" + createdDate);
                    Console.WriteLine("UUID: \t\t\t" + data.Value.uuid);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Message: {e.Message}");
            Console.WriteLine($"Stacktrace: {e.StackTrace}");
            Console.WriteLine($"Data: {e.Data}");
        }
    }

    private static Task<listid.Root?> ApiRequestList(string query)
    {
        return http.GetFromJsonAsync<listid.Root>(query);
    }

    private static Task<FetchLatest.Root?> ApiRequestLatest(string query)
    {
        return http.GetFromJsonAsync<FetchLatest.Root>(query);
    }
    
}