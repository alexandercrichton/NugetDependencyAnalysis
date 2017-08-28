using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Moq;
using NugetDependencyAnalysis.Finding;
using Serilog;
using Xunit;

namespace NugetDependencyAnalysisTests.Finding
{
    public class ProjectPackagesFinderTests
    {
        private static string TestDirectoriesLocation =>
            Path.Combine(Environment.CurrentDirectory, @"Finding\_TestData");

        private ProjectPackagesFinder Target { get; } = new ProjectPackagesFinder(Mock.Of<ILogger>());

        [Fact]
        public void Finds_PackagesConfig_In_Directory()
        {
            var testDirectory = Path.Combine(TestDirectoriesLocation, "SampleSolution");

            var actual = Target.Find(testDirectory);

            var expected = new List<ProjectPackagesFile> {
                new ProjectPackagesFile(
                    "SampleProject",
                    Path.Combine(TestDirectoriesLocation, @"SampleSolution\SampleProject\packages.config")
                )
            };

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Handles_Missing_Packages_Files()
        {
            var testDirectory = Path.Combine(TestDirectoriesLocation, "NoPackagesFile");

            var actual = Target.Find(testDirectory);

            var expected = new List<ProjectPackagesFile> {
                new ProjectPackagesFile("NoPackagesFile")
            };

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Handles_Multiple_Project_Files_In_Directory()
        {
            var testDirectory = Path.Combine(TestDirectoriesLocation, "MultipleProjectFiles");

            var actual = Target.Find(testDirectory);

            actual.Should().BeEmpty();
        }

        [Fact]
        public void Handles_Non_Existent_Directory()
        {
            var actual = Target.Find(@"C:\Not_A_Real_Directory");

            actual.Should().BeEmpty();
        }
    }
}
