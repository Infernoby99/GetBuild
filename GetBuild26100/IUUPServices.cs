namespace GetBuild26100;

public interface IUUPServices
{
    Task<listid.Root?> GetListidDataAsync(string query);
    Task<GetBuild.Root?> GetBuildDataAsync(string query);
}