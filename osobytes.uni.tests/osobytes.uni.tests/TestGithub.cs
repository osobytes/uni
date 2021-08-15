using Moq;
using osobytes.uni.API;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace osobytes.uni.tests
{
    public class TestGithub
    {
        [Fact]
        public async Task DefaultBranchIsReturned()
        {
            var mockResponse = new Mock<PublicRepositoryResponse>();
            mockResponse.SetupGet(r => r.BranchesUrl).Returns("https://api.github.com/repos/osobytes/uni/branches{/branch}");
            var result = await Github.GetDefaultBranchName(mockResponse.Object);
            Assert.Equal("main", result);
        }

        [Fact]
        public async Task UniPackageIsFetched()
        {
            var package = await Github.GetUniPackage("osobytes", "coretools", "main");
            Assert.NotNull(package);
            Assert.NotEmpty(package.Tools);
        }

        [Theory]
        [InlineData("osobytes.core", null)]
        [InlineData("osobytes.core", "Core")]
        public async Task ToolsAreDownloadedToFolder(string name, string folderName)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "uni.tests");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
            Directory.CreateDirectory(tempPath);
            var mockDefinition = new Mock<ToolDefinition>();
            mockDefinition.SetupGet(d => d.Name).Returns(name);
            mockDefinition.SetupGet(d => d.Folder).Returns(folderName);
            mockDefinition.SetupGet(d => d.Includes).Returns(new string[] { "core/Require.cs", "/core/Singleton.cs", "core/MonoBehaviourSingleton.cs" });
            await Github.DownloadToolFilesAsync("osobytes", "coretools", "main", mockDefinition.Object, new DirectoryInfo(tempPath));
            var folder = folderName ?? name;
            Assert.True(Directory.Exists(Path.Combine(tempPath, "Assets", "Scripts", folder)));
            Assert.True(File.Exists(Path.Combine(tempPath, "Assets", "Scripts", folder, "Require.cs")));
            Assert.True(File.Exists(Path.Combine(tempPath, "Assets", "Scripts", folder, "Singleton.cs")));
            Assert.True(File.Exists(Path.Combine(tempPath, "Assets", "Scripts", folder, "MonoBehaviourSingleton.cs")));
        }
    }
}
