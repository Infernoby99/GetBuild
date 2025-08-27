using System.Text.RegularExpressions;

namespace GetBuild26100;

class Program
{
    private static readonly LatestBuild _latestBuild = new();
    private static IUUPServices _uupServices;

    static async Task Main(string[] args)
    {
        var http = new HttpClient { BaseAddress = new Uri("https://api.uupdump.net/") };
        _uupServices = new UUPServices(http);
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
        listid.Root? list = await _uupServices.GetListidDataAsync("26100");
        foreach (var build in list.response.builds)
        {
            var created = DateTimeOffset.FromUnixTimeSeconds(build.Value.created).DateTime;
            Console.WriteLine($"Key: {build.Key}");
            Console.WriteLine($"[{i++}] | {build.Value.title} | {build.Value.arch} | {created}");
            Console.WriteLine($"UUID ==> {build.Value.uuid}");
            Console.WriteLine("______________________________________________________________________________________");
            BuildUuid.Add(build.Value.uuid);
        }

        do
        {
            Console.Write("Do you want to create aria2c download file from this list, type (\"yes\" | \"no\"): ");
            string answer = Console.ReadLine();


            if (answer.ToLower() == "yes")
            {
                Console.Write("Type [index] to get all the files in Download file: ");
                int index = Convert.ToInt32(Console.ReadLine());
                await CreateDownloadList(BuildUuid[index]);
            }
            else if (answer.ToLower() == "no") return;
            else Console.WriteLine("Undefined input try again!");
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
        listid.Root? listBuilds = await _uupServices.GetListidDataAsync("26100");

        string latestUuid = await GetLatestVersionId(listBuilds);

        GetBuild.Root? getBuilds = await _uupServices.GetBuildDataAsync(latestUuid);

        Console.WriteLine("\n============== LATEST BUILD INFO ==============");
        Console.WriteLine($"Windows Version:\t{_latestBuild.WinVers}");
        Console.WriteLine($"Architecture:\t\t{_latestBuild.Arch}64");
        Console.WriteLine("\n============== BUILD PROPETIES   ==============");
        Console.WriteLine($"Build Number:\t\t{_latestBuild.BuildNum}");
        Console.WriteLine($"UUID (Build):\t\t{_latestBuild.BuildUuid}");
        Console.WriteLine($"Release Date:\t\t{_latestBuild.RelDate}");
        Console.WriteLine("\n==============  FILE PROPETIES   ==============");
        Regex rx = new(".msu");
        foreach (var file in getBuilds.response.Files)
        {
            Match match = rx.Match(file.Key);
            if (match.Success)
            {
                Console.WriteLine($"Filename:\t\t{file.Key}");
                Console.WriteLine($"UUID (File):\t\t{file.Value.uuid}");
                Console.WriteLine($"SHA1:\t\t\t{file.Value.sha1}");
                Console.WriteLine($"Download URL:\t\t\n{file.Value.url}\n");
            }
        }
        Console.WriteLine("===============================================");
        do
        {
            Console.Write("Do you want to create aria2c download file from Latest Build, type (\"yes\" | \"no\"): ");
            string answer = Console.ReadLine();


            if (answer.ToLower() == "yes")
            {
                await CreateDownloadList(_latestBuild.BuildUuid);
                return;
            }
            if (answer.ToLower() == "no") return;
            Console.WriteLine("Undefined input try again!");
        } while (true);
    }

    private static async Task FilterList()
    {
        var list = await _uupServices.GetListidDataAsync("26100");

        if (list?.response?.builds == null)
        {
            Console.WriteLine("Keine Builds gefunden!");
            return;
        }

        var builds = list.response.builds.Values.ToList();

        string? buildFilter = null;
        string? archFilter = null;
        string? nameFilter = null;

        Console.WriteLine("********** FILTER OPTIONS **********");
        Console.WriteLine("[1] | builds");
        Console.WriteLine("[2] | arch");
        Console.WriteLine("[3] | Update name");
        Console.WriteLine("[4] | exit");
        Console.WriteLine("************************************");

        Console.Write("Chose Options bei typing number [x]: ");
        string chosenFilter = Console.ReadLine();

        switch (chosenFilter)
        {
            case "1":
                buildFilter = await FitlerOptionBuild();
                break;
            case "2":
                archFilter = await FilterOptionArch();
                break;
            case "3":
                Console.Write("Type in string pattern to search for: ");
                nameFilter = Console.ReadLine();
                break;
            case "4":
                return;
        }

        var filtered = builds.AsEnumerable();

        if (!string.IsNullOrEmpty(buildFilter))
        {
            if (buildFilter.Contains("|"))
            {
                var parts = buildFilter.Split("|");
                int high = int.Parse(parts[0]);
                int low = int.Parse(parts[1]);

                filtered = filtered.Where(b =>
                {
                    var patchStr = b.build.Split('.')[1];
                    if (int.TryParse(patchStr, out int patch))
                        return patch <= high && patch >= low;
                    return false;
                });
            }
            else
            {
                filtered = filtered.Where(b => b.build.EndsWith(buildFilter));
            }
        }

        if (!string.IsNullOrEmpty(archFilter))
        {
            filtered = filtered.Where(b => b.arch.Equals(archFilter, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrEmpty(nameFilter))
        {
            filtered = filtered.Where(b => b.title.Contains(nameFilter, StringComparison.OrdinalIgnoreCase));
        }
        var filteredList = filtered.ToList();

        int i = 0;
        foreach (var build in filtered)
        {
            Console.WriteLine($"[{++i}] | {build.title} | {build.arch} | {build.build} | {build.uuid}");
        }

        if (i == 0)
        {
            Console.WriteLine("Keine Ergebnisse für deine Filter gefunden!");
            return;
        }
        
        do
        {
            Console.Write("Do you want to create aria2c download file from this filtered list? (yes/no): ");
            string answer = Console.ReadLine().ToLower();

            if (answer == "yes")
            {
                Console.Write("Type [index] of build to create Download file: ");
                if (int.TryParse(Console.ReadLine(), out int index) &&
                    index >= 0 && index < filteredList.Count)
                {
                    await CreateDownloadList(filteredList[index].uuid);
                    Console.WriteLine("Download list created: aria2c.txt");
                    return;
                }
                Console.WriteLine("Invalid index, try again!");
                
            }
            else if (answer == "no")
            {
                return;
            }
            else
            {
                Console.WriteLine("Undefined input, try again!");
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
            Console.Write("Type [x] for which Build Filter: ");
            string? inputBuild = Console.ReadLine();

            if (int.TryParse(inputBuild, out int Index))
            {
                switch (Index)
                {
                    case 1:
                        Console.Write("\nType in Specific Build version => 26100.");
                        inputBuild = Console.ReadLine();
                        break;
                    case 2:
                        Console.WriteLine("\nType in Range of Scope to filter through.");
                        Console.Write("Highest Version: 26100.");
                        inputBuild = Console.ReadLine();
                        Console.Write("Lowest Verstion: 26100.");
                        inputBuild += '|' + Console.ReadLine();
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
                      && int.TryParse(ranges[1], out int low) && low >= 2)
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

            if (inputArch == "amd" || inputArch == "arm") return inputArch + "64";
            Console.WriteLine("Wrong input Try again!");
        } while (true);
    }

    private static async Task<string> GetLatestVersionId(listid.Root builds)
    {
        string chosenArch;
        do
        {
            Console.Write("Chose your Architecture ( amd | arm ) : ");
            chosenArch = Console.ReadLine();
            if (chosenArch == "amd" || chosenArch == "arm") continue;
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

    private static async Task CreateDownloadList(string uuid)
    {
        if (uuid != null)
        {
            await using var filestream = new FileStream("aria2c.txt", FileMode.Create);
            await using var streamwriter = new StreamWriter(filestream);
            GetBuild.Root? builds = await _uupServices.GetBuildDataAsync(uuid);
            foreach (var file in builds.response.Files)
            {
                await streamwriter.WriteLineAsync(file.Value.url);
                await streamwriter.WriteLineAsync($"  out={file.Key}");
                await streamwriter.WriteLineAsync();
            }
        }
    }
}