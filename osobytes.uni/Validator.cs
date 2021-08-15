using osobytes.uni.API;
using System;

namespace osobytes.uni
{
    public class Validator
    {
        public static (string owner, string repository) ValidateRepositoryArgument(string repository)
        {
            if (string.IsNullOrWhiteSpace(repository))
            {
                throw new ArgumentException("Argument repository is not valid or is empty.");
            }

            var repoParts = repository.Split('/');

            if (repoParts.Length != 2)
            {
                throw new ArgumentException("Argument repository given was not well formed. Remember to pass it in the following form: owner/repository i.e osobytes/coretools");
            }

            var owner = repoParts[0];
            var repo = repoParts[1];

            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("Given owner is not valid.");
            }

            if (string.IsNullOrWhiteSpace(repo))
            {
                throw new ArgumentException("Given repository is not valid.");
            }

            return (owner, repo);
        }

        public static bool ValidateToolDefinition(ToolDefinition tool)
        {
            if (string.IsNullOrWhiteSpace(tool.Name))
            {
                return false;
            }
            return true;
        }
    }
}
