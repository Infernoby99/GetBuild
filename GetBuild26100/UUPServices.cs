using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace GetBuild26100;

public class UUPServices : IUUPServices
{
    private readonly HttpClient _http;

    public UUPServices(HttpClient http)
    {
        _http = http;
    }

    public async Task<listid.Root?> GetListidDataAsync(string query)
    {
        return await _http.GetFromJsonAsync<listid.Root>($"listid.php?search={query}");
    }

    public async Task<GetBuild.Root?> GetBuildDataAsync(string query)
    {
        return await _http.GetFromJsonAsync<GetBuild.Root>($"get.php?id={query}");
    }
}