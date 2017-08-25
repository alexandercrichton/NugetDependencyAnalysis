using System;
using System.Collections.Generic;
using System.Linq;
using NugetDependencyAnalysis.Analysis;
using NugetDependencyAnalysis.Comparing;
using NugetDependencyAnalysis.Finding;
using NugetDependencyAnalysis.Parsing;
using NugetDependencyAnalysis.Parsing.FileReading;
using Serilog;

namespace NugetDependencyAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                OutputUsageInstructions();
                return;
            }

            var logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var command = TryParseCommand(args[0]);

            var directory = args[1];
            var projects = ParseProjectsFromDirectory(logger, directory);

            if (command == Command.Differences && args.Length == 2)
            {
                OutputDependencyDifferences(logger, projects);
            }
            else if (command == Command.UpgradeOrder && args.Length == 3)
            {
                var targetProjectName = args[2];
                OutputTargetProjectDependencyUpgradeOrder(logger, projects.ToList(), targetProjectName);
            }
            else
            {
                OutputUsageInstructions();
            }
        }

        private static void OutputUsageInstructions()
        {
            Console.WriteLine();
            Console.WriteLine("NugetDependencyAnalysis provides two functions:");
            Console.WriteLine();
            Console.WriteLine("  Nuget differences -");
            Console.WriteLine($"    Usage: NugetDependencyAnalysis.exe {CommandArguments.Differences} <directory to analyse>");
            Console.WriteLine($"      E.g. NugetDependencyAnalysis.exe {CommandArguments.Differences} \"C:\\repos\"");
            Console.WriteLine();
            Console.WriteLine("  Nuget upgrade order -");
            Console.WriteLine($"    Usage: NugetDependencyAnalysis.exe {CommandArguments.UpgradeOrder} <directory to analyse> <target nuget package name>");
            Console.WriteLine($"      E.g. NugetDependencyAnalysis.exe {CommandArguments.UpgradeOrder} \"C:\\repos\" \"My.Nuget\"");
            Console.WriteLine();

            Console.WriteLine("NugetDependencyAnalysis expects a single parameter, the directory to analyse in.");
            Console.WriteLine();
            Console.WriteLine();
        }

        private static Command? TryParseCommand(string argument)
        {
            argument = argument.ToLower();

            if (argument == CommandArguments.Differences.ToLower())
            {
                return Command.Differences;
            }
            else if (argument == CommandArguments.UpgradeOrder.ToLower())
            {
                return Command.UpgradeOrder;
            }
            else
            {
                return null;
            }
        }

        private static class CommandArguments
        {
            public const string Differences = "-differences";
            public const string UpgradeOrder = "-upgradeOrder";
        }

        private enum Command
        {
            Differences = 0,
            UpgradeOrder = 1
        }

        private static IEnumerable<ProjectNugetsGrouping> ParseProjectsFromDirectory(ILogger logger, string directory)
        {
            var finder = new PackagesConfigFinder(logger);
            var packagesConfigFiles = finder.Find(directory);

            var parser = new PackagesConfigParser(logger, new FileReader());
            return packagesConfigFiles.Select(packagesConfigFile => parser.Parse(packagesConfigFile));
        }

        private static void OutputDependencyDifferences(ILogger logger, IEnumerable<ProjectNugetsGrouping> projects)
        {
            var comparer = new ProjectsNugetsComparer();
            var nugetDifferences = comparer.Compare(projects);

            foreach (var difference in nugetDifferences)
            {
                if (difference.TargetFrameworkDifferences.Count > 0)
                {
                    logger.Information("{Package} has different frameworks targetted {TargetFrameworks} across different projects"
                        , difference.PackageName
                        , difference.TargetFrameworkDifferences.Select(targetFrameworkDifference => targetFrameworkDifference.TargetFramework));

                    foreach (var targetFrameworkDifference in difference.TargetFrameworkDifferences)
                    {
                        logger.Information("{Package} {TargetFramework} referenced in projects {Projects}",
                            difference.PackageName,
                            targetFrameworkDifference.TargetFramework,
                            targetFrameworkDifference.ProjectNames);
                    }
                }

                if (difference.VersionDifferences.Count > 0)
                {
                    logger.Information("{Package} has multiple versions {Versions} referenced in different projects"
                        , difference.PackageName
                        , difference.VersionDifferences.Select(versionDifference => versionDifference.Version));

                    foreach (var versionDifference in difference.VersionDifferences)
                    {
                        logger.Information("{Package} {Version} referenced in projects {Projects}",
                            difference.PackageName,
                            versionDifference.Version,
                            versionDifference.ProjectNames);
                    }
                }
            }
        }

        private static void OutputTargetProjectDependencyUpgradeOrder(
            ILogger logger, IReadOnlyCollection<ProjectNugetsGrouping> projects, string targetProjectName)
        {
            var upgrader = new ProjectDependencyUpgrader(logger);
            var (success, projectUpgradeOrder) =
                upgrader.ProjectUpgradeOrderStartingFromTargetProject(projects, targetProjectName);

            if (success)
            {
                logger.Information(
                    "From targeted project {TargetedProject}, projects should be upgraded in this order:",
                    targetProjectName
                );

                foreach (var project in projectUpgradeOrder)
                {
                    logger.Information("{Project}", project.ProjectName);
                }
            }
        }
    }
}
