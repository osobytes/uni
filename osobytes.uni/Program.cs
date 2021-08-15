using CommandLine;
using osobytes.uni.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace osobytes.uni
{
    public class Program
    {
        private static DirectoryInfo RootDir;
        class Options
        {
            [Option('i', "install", Required = true, HelpText = "The name of the repository that wants to be accessed. It should be in the format of owner/repository")]
            public string Repository { get; set; }

            [Option('t', "tool", Required = false, HelpText = "The name of the tool to install.")]
            public string Tool { get; set; }

            [Option('b', "branch", Required = false, HelpText = "Specify branch to use for download")]
            public string Branch { get; set; }
        }

        static async Task<int> Main(string[] args)
        {
            var (validDir, rootDir) = Unity.ValidateCurrentDirectoryIsUnityProject();
            if (!validDir)
            {
                Console.WriteLine("Error: This tool can only be used inside a valid Unity project.");
                return 1;
            }

            RootDir = rootDir;

            return await Parser.Default.ParseArguments<Options>(args)
            .MapResult(
            async (Options opts) => await RunUniAndReturnCodeAsync(opts),
            errs => Task.FromResult(1));
        }

        static async Task<int> RunUniAndReturnCodeAsync(Options options)
        {
            try
            {
                var (owner, repo) = Validator.ValidateRepositoryArgument(options.Repository);
                // Validate Github repository
                var (valid, response) = await Github.ValidatePublicRepositoryAsync(owner, repo);
                if (!valid)
                {
                    throw new Exception($"public repository https://github.com/{owner}/{repo} could not be found.");
                }

                var branch = options.Branch;
                if (string.IsNullOrWhiteSpace(branch))
                {
                    branch = await Github.GetDefaultBranchName(response);
                    if (string.IsNullOrWhiteSpace(branch))
                    {
                        throw new Exception("Could not obtain a default branch name from the repository. Please make sure a 'main' or 'master' branch is available. Otherwise you can specify the branch by using the --branch argument.");
                    }
                }
                else
                {
                    var validBranch = await Github.ValidateBranchExists(response, branch);
                    if (!validBranch)
                    {
                        throw new Exception($"Could not find branch '{branch}' in repository.");
                    }
                }

                var uniPackage = await Github.GetUniPackage(owner, repo, branch);
                if (uniPackage == null)
                {
                    throw new Exception($"Uni package not found in repository.");
                }

                ToolDefinition tool = null;
                if (!string.IsNullOrWhiteSpace(options.Tool))
                {
                    tool = uniPackage.Tools.FirstOrDefault(t => t.Name == options.Tool);
                    if (tool == null)
                    {
                        throw new Exception($"Could not find tool with name {options.Tool} \n Available tools: [{string.Join(", ", uniPackage.Tools.Select(t => t.Name).ToArray())}]");
                    }
                }
                else
                {
                    if (!uniPackage.Tools.Any(t => t.Default))
                    {
                        throw new Exception($"No default tool is specified for this repository. Use the --tool argument to specify the tool to use. \n Available tools: [{string.Join(", ", uniPackage.Tools.Select(t => t.Name).ToArray())}]");
                    }
                    tool = uniPackage.Tools.FirstOrDefault(t => t.Default);
                }

                var validTool = Validator.ValidateToolDefinition(tool);
                if (!validTool)
                {
                    throw new Exception($"Tool name is invalid.");
                }

                await Github.DownloadToolFilesAsync(owner, repo, branch, tool, RootDir);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            return 0;
        }

        static void InvalidArguments(IEnumerable<Error> errors) =>  throw new ArgumentException("Could not parse arguments given. run dotnet uni -h for more information on how to pass the arguments to the cli.");
    }
}
