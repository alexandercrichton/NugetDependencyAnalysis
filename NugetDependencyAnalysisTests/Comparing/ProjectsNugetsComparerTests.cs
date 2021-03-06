﻿using System.Collections.Generic;
using FluentAssertions;
using NugetDependencyAnalysis.Comparing;
using NugetDependencyAnalysis.Parsing;
using Xunit;

namespace NugetDependencyAnalysisTests.Comparing
{
    public class ProjectsNugetsComparerTests
    {
        public ProjectsNugetsComparerTests()
        {
            Target = new ProjectsNugetsComparer();
        }
        
        private ProjectsNugetsComparer Target { get; }

        [Fact]
        public void Returns_Empty_Results_For_Empty_Input()
        {
            var actual = Target.Compare(new List<ProjectNugetsGrouping>());

            actual.Should().BeEmpty();
        }

        [Fact]
        public void Returns_Empty_Results_For_Single_Project_Input()
        {
            var projects = new List<ProjectNugetsGrouping>
            {
                new ProjectNugetsGrouping("SampleProject", new List<NugetPackage> { new NugetPackage("Nuget", "1", "net46") })
            };

            var actual = Target.Compare(projects);

            actual.Should().BeEmpty();
        }

        [Fact]
        public void Returns_Empty_Results_If_Projects_Have_No_Differences()
        {
            var projects = new List<ProjectNugetsGrouping>
            {
                new ProjectNugetsGrouping("SampleProject1", new List<NugetPackage> { new NugetPackage("Nuget1", "1.0.0", "net46") }),
                new ProjectNugetsGrouping("SampleProject2", new List<NugetPackage> { new NugetPackage("Nuget2", "1.1.1", "net46") })
            };

            var actual = Target.Compare(projects);

            actual.Should().BeEmpty();
        }

        [Fact]
        public void Returns_Version_Differences()
        {
            var projects = new List<ProjectNugetsGrouping>
            {
                new ProjectNugetsGrouping("SampleProject1", new List<NugetPackage> { new NugetPackage("Nuget1", "1.0.0", "net46") }),
                new ProjectNugetsGrouping("SampleProject2", new List<NugetPackage> { new NugetPackage("Nuget1", "1.1.1", "net46") })
            };

            var actual = Target.Compare(projects);

            var expected = new NugetDifferences(
                "Nuget1",
                new List<TargetFrameworkProjectsGrouping>(),
                new List<VersionProjectsGrouping>
                {
                    new VersionProjectsGrouping("1.0.0", new List<string> { "SampleProject1" }),
                    new VersionProjectsGrouping("1.1.1", new List<string> { "SampleProject2" })
                });

            actual.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void Returns_TargetFramework_Differences()
        {
            var projects = new List<ProjectNugetsGrouping>
            {
                new ProjectNugetsGrouping("SampleProject1", new List<NugetPackage> { new NugetPackage("Nuget1", "1.0.0", "net46") }),
                new ProjectNugetsGrouping("SampleProject2", new List<NugetPackage> { new NugetPackage("Nuget1", "1.0.0", "net45") })
            };

            var actual = Target.Compare(projects);

            var expected = new NugetDifferences(
                "Nuget1",
                new List<TargetFrameworkProjectsGrouping>
                {
                    new TargetFrameworkProjectsGrouping("net46", new List<string> { "SampleProject1" }),
                    new TargetFrameworkProjectsGrouping("net45", new List<string> { "SampleProject2" })
                },
                new List<VersionProjectsGrouping>());

            actual.Should().BeEquivalentTo(expected);
        }
    }
}
