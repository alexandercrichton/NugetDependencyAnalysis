using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NugetDependencyAnalysis.Analysis;
using NugetDependencyAnalysis.Parsing;
using Serilog;
using Xunit;

namespace NugetDependencyAnalysisTests.Analysis
{
    public class ProjectDependencyUpgraderTests
    {
        private ProjectDependencyUpgrader Target { get; } = 
            new ProjectDependencyUpgrader(Mock.Of<ILogger>());

        [Theory]
        [MemberData(nameof(Data))]
        public void Test_Target_Project_Upgrade_Order(TestData testData)
        {
            var actual = Target.ProjectUpgradeOrderStartingFromTargetProject(
                TestProjects, 
                testData.TargetProjectName
            );

            var actualProjectUpgradeOrderNames = actual.projectUpgradeOrder
                .Select(project => project.ProjectName)
                .ToList();

            actualProjectUpgradeOrderNames.Should().Equal(testData.ExpectedProjectUpgradeOrder);
        }

        /// <summary>
        /// Test project dependency graph
        ///       a
        ///      / \
        ///     b   c
        ///    / \   \
        ///   d   e   f
        ///  / \ / \ /|
        /// g   h   i | 
        ///          \|
        ///           j
        /// </summary>
        private static IReadOnlyCollection<ProjectNugetsGrouping> TestProjects { get; } =
            new[]
            {
                Project("a", Dependencies("b", "c")),
                Project("b", Dependencies("d", "e")),
                Project("c", Dependencies("f")),
                Project("d", Dependencies("g", "h")),
                Project("e", Dependencies("h", "i")),
                Project("f", Dependencies("i", "j")),
                Project("g"),
                Project("h"),
                Project("i", Dependencies("j")),
                Project("j"),
            };

        private static IEnumerable<NugetPackage> Dependencies(params string[] dependencies) =>
            dependencies.Select(Nuget).ToArray();

        public static IEnumerable<object[]> Data =
            new[]
            {
                new TestData(
                    targetProjectName: "g",
                    expectedProjectUpgradeOrder: new[] { "g", "d", "b", "a" }
                ),
                new TestData(
                    targetProjectName: "h",
                    expectedProjectUpgradeOrder: new[] { "h", "d", "e", "b", "a" }
                ),
                new TestData(
                    targetProjectName: "i",
                    expectedProjectUpgradeOrder: new[] { "i", "e", "b", "f", "c", "a" }
                ),
                new TestData(
                    targetProjectName: "j",
                    expectedProjectUpgradeOrder: new[] { "j", "i", "e", "b", "f", "c", "a" }
                ),
            }
            .Select(x => new object[] { x });

        private static ProjectNugetsGrouping Project(string name, IEnumerable<NugetPackage> packages = null) =>
            new ProjectNugetsGrouping(name, packages?.ToList() ?? new List<NugetPackage>());

        private static NugetPackage Nuget(string name) =>
            new NugetPackage(name, "1.0.0", "net46");

        public class TestData
        {
            public TestData(string targetProjectName, IEnumerable<string> expectedProjectUpgradeOrder)
            {
                TargetProjectName = targetProjectName;
                ExpectedProjectUpgradeOrder = expectedProjectUpgradeOrder.ToList();
            }

            public string TargetProjectName { get; }

            public IReadOnlyCollection<string> ExpectedProjectUpgradeOrder { get; }
        }
    }
}
