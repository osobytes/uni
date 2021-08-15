using Moq;
using osobytes.uni.API;
using System;
using System.Threading.Tasks;
using Xunit;

namespace osobytes.uni.tests
{
    public class TestValidators
    {
        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("/repository")]
        [InlineData("SomeString")]
        [InlineData("owner/")]
        public void ValidateRepositoryTests(string repository)
        {
            Assert.Throws<ArgumentException>(() => Validator.ValidateRepositoryArgument(repository));
        }

        [Theory]
        [InlineData("osobytes", "coretools", true)]
        [InlineData("nonexistent", "nonexistent", false)]
        public async Task ValidatePublicRepositoryTestsAsync(string owner, string repository, bool shouldExist)
        {
            var (success, data)  = await Github.ValidatePublicRepositoryAsync(owner, repository);
            Assert.Equal(shouldExist, success);
            if (shouldExist)
            {
                Assert.NotNull(data);
            }
        }

        [Theory]
        [InlineData("main", true)]
        [InlineData("nonexistent", false)]
        public async Task ValidateBranchExistsAsync(string branch, bool shouldExist)
        {
            var mockResponse = new Mock<PublicRepositoryResponse>();
            mockResponse.SetupGet(r => r.BranchesUrl).Returns("https://api.github.com/repos/osobytes/uni/branches{/branch}");
            var result = await Github.ValidateBranchExists(mockResponse.Object, branch);
            Assert.Equal(shouldExist, result);
        }
    }
}
