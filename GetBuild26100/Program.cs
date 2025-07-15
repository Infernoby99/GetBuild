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
            listid.Root? build = await apiRequest();
            
            foreach (var data in build.response.builds)
            {
                string[] sub = data.Value.build.Split('.');
                if (sub[0] == "26100" && Convert.ToInt32(sub[1]) > 4000 && data.Value.title.Contains("Cumulative"))
                {
                    Console.WriteLine("***********************");
                    Console.WriteLine("Titel: \t\t\t" + data.Value.title);
                    Console.WriteLine("Architekur: \t\t" + data.Value.arch);
                    Console.WriteLine("Build Nummer: \t\t" + data.Value.build);
                    var createdDate = DateTimeOffset.FromUnixTimeSeconds(data.Value.created).DateTime;
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

    private static Task<listid.Root?> apiRequest()
    {
        const string listUrl = "listid.php?search=26100&&sortByDate=1";
        return http.GetFromJsonAsync<listid.Root>(listUrl);
    }
    
}