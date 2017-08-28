using System.Collections.Generic;
using System.Linq;
using NugetDependencyAnalysis.Parsing;
using Serilog;

namespace NugetDependencyAnalysis.Upgrading
{
    internal class ProjectDependencyUpgrader
    {
        public ProjectDependencyUpgrader(ILogger logger)
        {
            Logger = logger;
        }

        private ILogger Logger { get; }

        public (bool success, IReadOnlyCollection<ProjectNugetsGrouping> projectUpgradeOrder) ProjectUpgradeOrderStartingFromTargetProject(
            IReadOnlyCollection<ProjectNugetsGrouping> projects, string targetProjectName)
        {
            var targetProject = projects.SingleOrDefault(project => project.ProjectName == targetProjectName);
            if (targetProject == null)
            {
                Logger.Error("{TargetProject} not found", targetProjectName);
                return (false, null);
            }

            var doneProjects = new HashSet<string>();
            var projectUpgradeOrder = ProjectUpgradeOrderFromDependency(projects, targetProject, targetProject, doneProjects).ToList();
            
            return (true, projectUpgradeOrder);
        }

        private static IEnumerable<ProjectNugetsGrouping> ProjectUpgradeOrderFromDependency(
            IReadOnlyCollection<ProjectNugetsGrouping> allProjects, 
            ProjectNugetsGrouping targetProject,
            ProjectNugetsGrouping currentProject,
            HashSet<string> doneProjects)
        {
            var allDependenciesAreDone = AllDependenciesAreDone(allProjects, targetProject, currentProject, doneProjects);

            if (allDependenciesAreDone && !doneProjects.Contains(currentProject.ProjectName))
            {
                doneProjects.Add(currentProject.ProjectName);

                yield return currentProject;

                var dependents = ImmediateDependents(allProjects, currentProject);

                var dependentsOrder = dependents.SelectMany(dependent =>
                    ProjectUpgradeOrderFromDependency(allProjects, targetProject, dependent, doneProjects)
                );

                foreach (var dependentProject in dependentsOrder)
                {
                    yield return dependentProject;
                }
            }
        }

        private static bool AllDependenciesAreDone(
            IReadOnlyCollection<ProjectNugetsGrouping> allProjects, 
            ProjectNugetsGrouping targetProject, 
            ProjectNugetsGrouping currentProject, 
            IReadOnlyCollection<string> doneProjects)
        {
            var dependencies = ImmediateDependencies(allProjects, currentProject);
            var remainingDependencies = dependencies.Where(dependency => 
                !doneProjects.Contains(dependency.ProjectName) &&
                    DependsOnTargetProject(allProjects, targetProject, dependency)
            );
            return !remainingDependencies.Any();
        }

        private static IEnumerable<ProjectNugetsGrouping> ImmediateDependencies(
            IEnumerable<ProjectNugetsGrouping> allProjects, ProjectNugetsGrouping currentProject
        ) =>
            allProjects.Where(project =>
                currentProject.Nugets.Any(nuget =>
                    nuget.Name == project.ProjectName
                )
            );

        private static bool DependsOnTargetProject(
            IReadOnlyCollection<ProjectNugetsGrouping> allProjects,
            ProjectNugetsGrouping targetProject, 
            ProjectNugetsGrouping currentProject)
        {
            var dependencies = ImmediateDependencies(allProjects, currentProject);
            return dependencies.Any(dependency => 
                dependency.ProjectName == targetProject.ProjectName ||
                    DependsOnTargetProject(allProjects, targetProject, dependency)
            );
        }

        private static IEnumerable<ProjectNugetsGrouping> ImmediateDependents(
            IEnumerable<ProjectNugetsGrouping> allProjects, ProjectNugetsGrouping currentProject
        ) =>
            allProjects.Where(project =>
                project.Nugets.Any(nuget =>
                    nuget.Name == currentProject.ProjectName
                )
            );
    }
}
