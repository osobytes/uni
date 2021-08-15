using System;
using System.IO;
using System.Linq;

namespace osobytes.uni
{
    public class Unity
    {
        public static (bool, DirectoryInfo rootDir) ValidateCurrentDirectoryIsUnityProject()
        {
            var currentPath = Environment.CurrentDirectory;
            var dir = new DirectoryInfo(currentPath);
            var current = dir;
            while (current != null)
            {
                var packagesDir = current.EnumerateDirectories().FirstOrDefault(d => d.Name == "Packages");
                if (packagesDir != null)
                {
                    if(packagesDir.EnumerateFiles().Any(f => f.Name == "manifest.json"))
                    {
                        return (true, current);
                    }
                }
                current = current.Parent;
            }
            return (false, null);
        }
    }
}
