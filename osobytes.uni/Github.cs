using Newtonsoft.Json;
using osobytes.uni.API;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace osobytes.uni
{
    public static class Github
    {
        private const string ReposGet = "https://api.github.com/repos/{0}/{1}";
        private const string ContentRepoGet = "https://raw.githubusercontent.com/{0}/{1}/{2}/{3}";
        private const string UniSpecFileName = "uni.json";
        private static readonly HttpClient client = new HttpClient();

        static Github()
        {
            client.DefaultRequestHeaders.Add("User-Agent", "osobytes-dotnet-uni");
            // client.DefaultRequestHeaders.Add("X-GitHub-Media-Type", "gitub.v3");
        }

        public static async Task<(bool, PublicRepositoryResponse)> ValidatePublicRepositoryAsync(string username, string repo)
        {
            var url = string.Format(ReposGet, username, repo);
            try
            {
                var response = await client.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Github api call was not succesfull", inner: null, statusCode: response.StatusCode);
                }
                var jsonResult = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PublicRepositoryResponse>(jsonResult);
                return (true, result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error has ocurred while attempting to get public repository.");
                Console.WriteLine(ex);
                return (false, null);
            }
        }

        public static async Task<bool> ValidateBranchExists(PublicRepositoryResponse response, string branchName)
        {
            if (string.IsNullOrWhiteSpace(branchName))
            {
                return false;
            }
            var url = response.BranchesUrl.Replace("{/branch}", $"/{branchName}");
            try
            {
                var result = await client.GetAsync(url);
                return result.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<string> GetDefaultBranchName(PublicRepositoryResponse response)
        {
            var url = response.BranchesUrl.Replace("{/branch}", "");
            try
            {
                var result = await client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    return null;
                }
                var jsonResult = await result.Content.ReadAsStringAsync();
                var branches = JsonConvert.DeserializeObject<GithubBranch[]>(jsonResult);
                var validBranches = branches.Where(b => b.Name == "main" || b.Name == "master");
                if (!validBranches.Any())
                {
                    return null;
                }

                // Give main priority over master.
                if (validBranches.Any(b => b.Name == "main"))
                {
                    return "main";
                }
                if (validBranches.Any(b => b.Name == "master"))
                {
                    return "master";
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<UniPackage> GetUniPackage(string owner, string repo, string branch)
        {
            var url = string.Format(ContentRepoGet, owner, repo, branch, UniSpecFileName);
            try
            {
                var result = await client.GetAsync(url);
                if (!result.IsSuccessStatusCode)
                {
                    throw new HttpRequestException("Response was not succesful while trying to fetch uni.json file from repository.", inner: null, statusCode: result.StatusCode);
                }
                var jsonResult = await result.Content.ReadAsStringAsync();
                var uniPackage = JsonConvert.DeserializeObject<UniPackage>(jsonResult);
                return uniPackage;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public static async Task DownloadToolFilesAsync(string owner, string repo, string branch, ToolDefinition tool, DirectoryInfo unityRootDir)
        {
            var folderPath = Path.Combine(unityRootDir.FullName, "Assets", "Scripts");
            var folderName = string.IsNullOrWhiteSpace(tool.Folder) ? tool.Name : tool.Folder;
            var downloadPath = Path.Combine(folderPath, folderName);
            foreach (var file in tool.Includes)
            {
                var filePath = file;
                if (file.StartsWith("/"))
                {
                    filePath = file[1..];
                }
                var url = string.Format(ContentRepoGet, owner, repo, branch, filePath);
                try
                {
                    var fileResult = await client.GetAsync(url);
                    if (fileResult.IsSuccessStatusCode)
                    {
                        var fileName = Path.GetFileName(filePath);
                        if (!Directory.Exists(downloadPath))
                        {
                            Directory.CreateDirectory(downloadPath);
                        }
                        var downloadFilePath = Path.Combine(downloadPath, fileName);
                        var stringResult = await fileResult.Content.ReadAsStringAsync();
                        File.WriteAllText(downloadFilePath, stringResult);
                    }
                }
                catch
                {
                    Console.WriteLine($"Error aquiring file {filePath} from repository. Skipping file..");
                }
            }
        }
    }
}
