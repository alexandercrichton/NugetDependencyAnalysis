using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Serilog;

namespace NugetDependencyAnalysis.Finding
{
    internal class ProjectPackagesFinder
    {
        public ProjectPackagesFinder(ILogger logger)
        {
            Logger = logger;
        }

        private ILogger Logger { get; }

        public IReadOnlyList<ProjectPackagesFile> Find(string directory)
        {
            var root = new DirectoryInfo(directory);
            if (!root.Exists)
            {
                Logger.Error("{Directory} does not exist", directory);
                return Array.Empty<ProjectPackagesFile>();
            }

            var projects = new List<ProjectPackagesFile>();

            var projectFileGroups = root.GetFiles("*.csproj", SearchOption.AllDirectories)
                .GroupBy(file => file.DirectoryName)
                .ToList();

            foreach (var group in projectFileGroups)
            {
                if (group.Count() > 1)
                {
                    Logger.Warning("Skipping {Directory} because it contains multiple project files.", group.Key);
                }
                else
                {
                    var projectFile = group.Single();
                    projects.Add(CreateProjectPackagesFile(projectFile));
                }
            }

            return projects;
        }

        private static ProjectPackagesFile CreateProjectPackagesFile(FileInfo projectFile)
        {
            var projectName = projectFile.Name.Substring(0, projectFile.Name.Length - projectFile.Extension.Length);

            var packagesFile = projectFile.Directory.GetFiles("packages.config", SearchOption.TopDirectoryOnly)
                .SingleOrDefault();

            return packagesFile != null 
                ? new ProjectPackagesFile(projectName, packagesFile.FullName) 
                : new ProjectPackagesFile(projectName);

        }
    }
}
