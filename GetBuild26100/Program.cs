using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace GetBuild26100;

using System.Net.Http.Json;

class Program
{
    private static readonly LatestBuild _latestBuild = new();
    
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://api.uupdump.net/")
    };

    static async Task Main(string[] args)
    {
        try
        {
            const string listUrl = "listid.php?search=26100";
            const string getUrl = "get.php?id=";
            listid.Root? listBuilds = await ApiRequestList(listUrl);
            

            string latestUuid = await GetLatestVersionId(listBuilds);
            
            GetBuild.Root? getBuilds = await ApiRequestGet(getUrl+ latestUuid);
            string chosenId = await Getfilename(getBuilds);
            

            GetBuild.Root? getBuildLink = await ApiRequestGet(getUrl + latestUuid, true);
            await GetLink(getBuildLink, chosenId);
            
            Console.WriteLine("\n============== LATEST BUILD INFO ==============");
            Console.WriteLine($"Windows Version:\t{_latestBuild.WinVers}");
            Console.WriteLine($"Architecture:\t\t{_latestBuild.Arch}64");
            Console.WriteLine("\n============== BUILD PROPETIES   ==============");
            Console.WriteLine($"Build Number:\t\t{_latestBuild.BuildNum}");
            Console.WriteLine($"UUID (Build):\t\t{_latestBuild.BuildUuid}");
            Console.WriteLine($"Release Date:\t\t{_latestBuild.RelDate}");
            Console.WriteLine("\n==============  FILE PROPETIES   ==============");
            Console.WriteLine($"Filename:\t\t{_latestBuild.Filename}");
            Console.WriteLine($"UUID (File):\t\t{_latestBuild.FileUuid}");
            Console.WriteLine($"SHA1:\t\t\t{_latestBuild.Hash}");
            Console.WriteLine($"Download URL:\t\t{_latestBuild.Url}");
            Console.WriteLine("===============================================");
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
        return _http.GetFromJsonAsync<listid.Root>(query);
    }

    private static Task<GetBuild.Root?> ApiRequestGet(string query, bool noLink = false)
    {
        string reqLink = !noLink ? "&&noLink=1" : "&&nolink=0";
        return _http.GetFromJsonAsync<GetBuild.Root>(query + reqLink) ;
    }

    private static async Task<string> GetLatestVersionId(listid.Root builds)
    {
        string chosenArch;
        do
        {
            Console.Write("Chose your Architecture ( amd | arm ) : ");
            chosenArch = Console.ReadLine();
            if(chosenArch == "amd" | chosenArch == "arm") continue;
            chosenArch = "";

        } while (chosenArch == "");
        Regex rx = new(@"^26100\.(\d+)$");
        string? bestUuid = null;
        int maxPatch = -1;
        
        foreach (var data in builds.response.builds)
        {
            string build = data.Value.build;

            Match match = rx.Match(build);
            if (chosenArch != null && match.Success && data.Value.arch.Contains(chosenArch))
            {
                int patch = int.Parse(match.Groups[1].Value);

                if (patch > maxPatch)
                {
                    maxPatch = patch;
                    bestUuid = data.Value.uuid;
                    
                    Console.WriteLine($"Latest Update Version\t\t| {data.Value.title}");

                    _latestBuild.WinVers = data.Value.title;
                    _latestBuild.BuildNum = data.Value.build;
                    _latestBuild.Arch = chosenArch;
                    string relDate = DateTimeOffset.FromUnixTimeSeconds(data.Value.created).DateTime.ToString();
                    _latestBuild.RelDate = relDate;
                    _latestBuild.BuildUuid = data.Value.uuid;
                }
                
            }
        }
        Console.WriteLine($"Neueste Patch-Version\t\t| 26100.{maxPatch}");
        Console.WriteLine($"BuildUuid from latest version\t| {bestUuid}");
        return bestUuid;
    }

    private static async Task<string> Getfilename(GetBuild.Root build)
    {
        Regex rx = new(".msu");
        List<string> filesId = new();
        List<string> filename = new();
        
        int i = 0;
        
        Console.WriteLine("***************************************************************************************");
        foreach (var file in build.response.Files)
        {
            Match match = rx.Match(file.Key);

            if (match.Success)
            {
                Console.WriteLine($"Index[{i++}] Filename: {file.Key} | {file.Value.uuid}");
                filesId.Add(file.Value.uuid);
                filename.Add(file.Key);
            }
        }
        Console.WriteLine("***************************************************************************************");
        
        do
        {
            string chosenId = String.Empty;
            Console.Write("\nType Index to get File Link: ");
            string? input = Console.ReadLine();
            
            if (int.TryParse(input, out int index) && index >= 0 && index < filesId.Count)
            {
                chosenId = filesId[index];
                Console.WriteLine("\nChosen File: " + filename[index]);
                Console.WriteLine("Chosen UUID: " + chosenId);
                bool check = await CheckChoice(filename[index], chosenId);
                
                if (check) return chosenId;
            }
            else
            {
                Console.WriteLine("\n!!!!! The typed Index is out of Bounds or not and Integer !!!!!\n");
            }
            
        } while (true);

        
    }

    private static async Task GetLink(GetBuild.Root builds, string uuid)
    {
        foreach (var build in builds.response.Files)
        {
            if (build.Value.uuid == uuid)
            {
                _latestBuild.Url = build.Value.url;
                _latestBuild.Hash = build.Value.sha1;
            }
        }
    }

    private static async Task<bool> CheckChoice(string filename, string fileId)
    {
        Console.Write("\nChosen right File? ( press 'y'), " +
                      "else press Enter:");
        string answer = Console.ReadLine();
        if (answer == "Y" || answer == "y")
        {
            _latestBuild.Filename = filename;
            _latestBuild.FileUuid = fileId;
            return true;
        }

        return false;
    }
    
    
}