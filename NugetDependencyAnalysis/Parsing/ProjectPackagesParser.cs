using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NugetDependencyAnalysis.Finding;
using NugetDependencyAnalysis.Parsing.FileReading;
using Serilog;

namespace NugetDependencyAnalysis.Parsing
{
    internal class ProjectPackagesParser
    {
        public ProjectPackagesParser(ILogger logger, IFileReader fileReader)
        {
            Logger = logger;
            FileReader = fileReader;
        }

        private IFileReader FileReader { get; }

        private ILogger Logger { get; }

        public ProjectNugetsGrouping Parse(ProjectPackagesFile projectPackagesFile)
        {
            if (!projectPackagesFile.HasPackages)
            {
                return EmptyProjectNugetsGrouping(projectPackagesFile.ProjectName);
            }

            var contents = FileReader.ReadFileContents(projectPackagesFile.PackagesFilePath);
            if (contents == string.Empty)
            {
                Logger.Warning("Config file located at {Path} is empty", projectPackagesFile.PackagesFilePath);
                return EmptyProjectNugetsGrouping(projectPackagesFile.ProjectName);
            }

            XDocument xDocument;
            try
            {
                xDocument = XDocument.Parse(contents);
            }
            catch (XmlException e)
            {
                Logger.Warning(
                    "Config file located at {Path} contains invalid XML - {XmlExceptionMessage}", 
                    projectPackagesFile.PackagesFilePath, e.Message
                );
                return EmptyProjectNugetsGrouping(projectPackagesFile.ProjectName);
            }

            try
            {
                var packages = from package in xDocument.Descendants("package")
                               select new NugetPackage(
                                   package.Attribute("id").Value,
                                   package.Attribute("version").Value,
                                   package.Attribute("targetFramework").Value);

                return new ProjectNugetsGrouping(projectPackagesFile.ProjectName, packages.ToList());
            }
            catch (NullReferenceException)
            {
                Logger.Warning(
                    "Config file located at {Path} contains package elements with missing id, version, or targetFramework attributes", 
                    projectPackagesFile.PackagesFilePath
                );
                return EmptyProjectNugetsGrouping(projectPackagesFile.ProjectName);
            }
        }

        private ProjectNugetsGrouping EmptyProjectNugetsGrouping(string projectName)
        {
            return new ProjectNugetsGrouping(projectName, Array.Empty<NugetPackage>());
        }
    }
}
