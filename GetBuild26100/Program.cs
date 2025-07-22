using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

namespace GetBuild26100;

using System.Net.Http.Json;

class Program
{
    private static readonly LatestBuild _latestBuild = new();
    const string ListUrl = "listid.php?search=26100";
    const string GetUrl = "get.php?id=";
    
    private static readonly HttpClient _http = new()
    {
        BaseAddress = new Uri("https://api.uupdump.net/")
    };
    

    static async Task Main(string[] args)
    {
        do
        {
            try
            {
                int option = await MenuOptions();

                switch (option)
                {
                    case 1:
                        await GetLatestBuild();
                        break;
                    case 2:
                        await GetallBuilds();
                        break;
                    case 3:
                        await FilterList();
                        break;
                    case 4:
                        Environment.Exit(1);
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Stacktrace: {e.StackTrace}");
                Console.WriteLine($"Data: {e.Data}");
            }
        } while (true);
    }

    private static async Task GetallBuilds()
    {
        int i = 0;
        List<string> BuildUuid = new();
        listid.Root list = await _http.GetFromJsonAsync<listid.Root>("listid.php?search=26100");
        foreach (var build in list.response.builds)
        {
            
            var created = DateTimeOffset.FromUnixTimeSeconds(build.Value.created).DateTime;
            Console.WriteLine($"[{i++}] | {build.Value.title} | {build.Value.arch} | {created}");
            Console.WriteLine($"UUID ==> {build.Value.uuid}");
            Console.WriteLine("_______________________________________________________________________________________");
            BuildUuid.Add(build.Value.uuid);
        }

        do
        {
            Console.Write("Type Index to get all files listed in :");
            int index = Convert.ToInt32(Console.ReadLine());

            if (index >= 0 && index < BuildUuid.Count)
            {
                int x = 0;
                GetBuild.Root builds = await _http.GetFromJsonAsync<GetBuild.Root>($"get.php?id={BuildUuid[index]}");
                foreach (var file in builds.response.Files)
                {
                    using (var fileStream = new FileStream("example.txt", FileMode.Create))
                    {
                        using (var streamWriter = new StreamWriter(fileStream))
                        {
                            streamWriter.WriteLine($"Index[{x++}] | {file.Key} | {file.Value.uuid} | {file.Value.url}");
                            streamWriter.WriteLine("______________________________________________________________________________________________________________________________________________________________");
                        }
                    }   
                }
                return;
            }
            
        } while (true);
    }
    

    private static async Task<int> MenuOptions()
    {
        string chosenOpt;

        Console.WriteLine("************** UUP DUMP RPTU ****************");
        Console.WriteLine("[1] | Get Latest Build");
        Console.WriteLine("[2] | Get All Builds");
        Console.WriteLine("[3] | Filter through Build List");
        Console.WriteLine("[4] | Exit");
        Console.WriteLine("*********************************************\n");
        do
        {
            
            Console.Write("Chose Options bei typing number [x]: ");
            chosenOpt = Console.ReadLine();


            if (int.TryParse(chosenOpt, out int option) && option > 0 && option < 5)
            {
                return option;
            }

            Console.WriteLine("Your input does not correspond to our options, Please Try again");
        } while (true);
    }

    private static async Task GetLatestBuild()
    {
        
        listid.Root? listBuilds = await ApiRequestList(ListUrl);
            

        string latestUuid = await GetLatestVersionId(listBuilds);
            
        GetBuild.Root? getBuilds = await ApiRequestGet(GetUrl+ latestUuid);
        string chosenId = await GetFilename(getBuilds);
            

        GetBuild.Root? getBuildLink = await ApiRequestGet(GetUrl + latestUuid, true);
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
        Console.WriteLine("\n");
        Console.ReadLine();
    }

    private static async Task FilterList()
    {
        string query = $"listid.php?search=26100";
        bool scope = false;
        string[] FilterOptions = new string[4];
        do
        {
            string chosenFilter = string.Empty;
            string? buildFilter = string.Empty;
            string? archFilter = string.Empty;
            string? stringFilter = string.Empty;
            
            Console.WriteLine("********** FILTER OPTIONS **********");
            Console.WriteLine("[1] | builds");
            Console.WriteLine("[2] | arch");
            Console.WriteLine("[3] | Update name");
            Console.WriteLine("[4] | exit");
            Console.WriteLine("************************************");
            
            Console.Write("\nChose your Filter Options ");
            chosenFilter = Console.ReadLine();
            
            if (int.TryParse(chosenFilter, out int filterNum))
            {
                switch (filterNum)
                {
                    case 1:
                        if (string.IsNullOrEmpty(buildFilter))buildFilter = await FitlerOptionBuild();
                        break;
                    case 2:
                        if (string.IsNullOrEmpty(archFilter)) archFilter = await FilterOptionArch();
                        break;
                    case 3:
                        if (string.IsNullOrEmpty(stringFilter)) { 
                            Console.Write("Type in string pattern to search for: ");
                            stringFilter = Console.ReadLine();
                        }
                        break;
                    case 4: return;
                }
            }

            
            if (!string.IsNullOrEmpty(buildFilter))
            {
                if (buildFilter.Contains("|"))
                {
                    Console.WriteLine("F");
                    string[] range = buildFilter.Split("|");
                    for (int i = 0; i < 2; i++) FilterOptions[i] = range[i];
                    scope = true;
                }
                else
                {
                    FilterOptions[0] = buildFilter;
                    query += $"&search={FilterOptions[0]}";
                    scope = false;
                }
            }

            if (!string.IsNullOrEmpty(archFilter))
            {
                FilterOptions[2] = archFilter;
                query += $"&search={FilterOptions[2]}";
                
            }

            if (!string.IsNullOrEmpty(stringFilter))
            {
                FilterOptions[3] = stringFilter;
                query += $"&search={FilterOptions[3]}";
            }
            
            Console.WriteLine("\nCurrent Filter:");
            if(scope) Console.WriteLine($"Build Filter Scope ==> \t{FilterOptions[0]} to {FilterOptions[1]}");
            if(!string.IsNullOrEmpty(buildFilter))Console.WriteLine($"Build Filter ==> \t{FilterOptions[0]}");
            if(!string.IsNullOrEmpty(archFilter)) Console.WriteLine($"Arch Filter ==> \t{FilterOptions[2]}");
            if(!string.IsNullOrEmpty(stringFilter)) Console.WriteLine($"String Filter ==> \t{FilterOptions[3]}");


            Console.Write("Want to add more Filter? press 'y' for yes, else press Enter to Start Filter :");
            string startFilter = Console.ReadLine();

            if (startFilter == "y" || startFilter == "Y")
            {
                int i = 0;
                try
                {
                    listid.Root? FilterdBuilds = await _http.GetFromJsonAsync<listid.Root>(query);
                    foreach (var build in FilterdBuilds.response.builds)
                    {
                        if (scope)
                        {
                            int.TryParse(FilterOptions[0], out int scaleHigh);
                            int.TryParse(FilterOptions[1], out int scaleLow);
                            string[] version = build.Value.build.Split(".");

                            if (int.TryParse(version[1], out int vers) && vers < scaleHigh && vers > scaleLow)
                            {
                                Console.WriteLine($"[{++i}] | {build.Value.title} | {build.Value.arch} | {build.Value.uuid}");
                            }
                        }
                        Console.WriteLine($"[{++i}] | {build.Value.title} | {build.Value.arch} | {build.Value.uuid}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        } while (true);
    }
    
    private static async Task<string?> FitlerOptionBuild()
    {
        do
        {
            bool Scope = false;
            Console.WriteLine("********** BUILD Filter OPTIONS **********");
            Console.WriteLine("[1] | Filter for Specific Build version");
            Console.WriteLine("[2] | Filter Scope of Builds");
            Console.WriteLine("[3] | exit");
            Console.WriteLine("******************************************");
            string? inputBuild = Console.ReadLine();

            if (int.TryParse(inputBuild, out int Index))
            {
                switch (Index)
                {
                    case 1: 
                        Console.WriteLine("\nType in Specific Build version => 26100.");
                        inputBuild = Console.ReadLine();
                        break;
                    case 2:
                        Console.WriteLine("\nType in Range of Scope to filter through.");
                        Console.WriteLine($"Highest Version: 26100.{inputBuild =  Console.ReadLine()}");
                        Console.WriteLine($"Lowest Verstion: 26100.{inputBuild += '|' + Console.ReadLine()}");
                        Scope = true;
                        break;
                    case 3:
                        return string.Empty;
                }
            }

            if (!Scope && int.TryParse(inputBuild, out int build))
            {
                return build.ToString();
            }
            
            string[] ranges = Scope ? inputBuild.Split("|") : null;
            if (Scope && ranges != null
                      && int.TryParse(ranges[0], out int high) && high >= 2
                      && int.TryParse(ranges[1], out int low)  && low >= 2)
            {
                return high + "|" + low;
            }
            Console.WriteLine("One of your Inputs ist either out of Range or wrong input! Try again.");
        } while (true);
        
    } 

    private static async Task<string?> FilterOptionArch()
    {
        do
        {
            Console.WriteLine("**********  ARCH FILTER OPTIONS **********");
            Console.Write("Type in arch ( amd | arm ): ");
            string inputArch = Console.ReadLine();

            if (inputArch == "amd" || inputArch == "arm") return inputArch;
            Console.WriteLine("Wrong input Try again!");
        } while (true);
    }
    
    private static Task<listid.Root?> ApiRequestList(string query)
    {
        return _http.GetFromJsonAsync<listid.Root>(query);
    }

    private static Task<GetBuild.Root?> ApiRequestGet(string query, bool noLink = false)
    {
        string reqLink = !noLink ? "&noLink=0" : "&noLink=1";
        return _http.GetFromJsonAsync<GetBuild.Root>(query + reqLink) ;
    }

    private static async Task<string> GetLatestVersionId(listid.Root builds)
    {
        string chosenArch;
        do
        {
            Console.Write("Chose your Architecture ( amd | arm ) : ");
            chosenArch = Console.ReadLine();
            if(chosenArch == "amd" || chosenArch == "arm") continue;
            chosenArch = "";

        } while (chosenArch == "");
        string? bestUuid = null;
        int maxPatch = -1;
        
        foreach (var data in builds.response.builds)
        {
            string build = data.Value.build;

            
            if (data.Value.build.StartsWith("26100.") && data.Value.arch.Contains(chosenArch))
            {
                if (int.TryParse(data.Value.build.Split('.')[1], out int patch))
                {
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
        }
        Console.WriteLine($"Neueste Patch-Version\t\t| 26100.{maxPatch}");
        Console.WriteLine($"BuildUuid from latest version\t| {bestUuid}");
        return bestUuid;
    }

    private static async Task<string> GetFilename(GetBuild.Root build)
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
                Console.WriteLine($"Index[{i++}] | Filename: {file.Key} | {file.Value.uuid}");
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