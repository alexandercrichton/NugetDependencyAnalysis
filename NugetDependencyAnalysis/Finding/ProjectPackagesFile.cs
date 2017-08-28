using System;

namespace NugetDependencyAnalysis.Finding
{
    internal class ProjectPackagesFile : IEquatable<ProjectPackagesFile>
    {
        public ProjectPackagesFile(string projectName)
        {
            ProjectName = projectName;
        }

        public ProjectPackagesFile(string projectName, string packagesFilePath) : this(projectName)
        {
            PackagesFilePath = packagesFilePath;
        }

        public string ProjectName { get; }

        public string PackagesFilePath { get; }

        public bool HasPackages =>
            !string.IsNullOrWhiteSpace(PackagesFilePath);

        public bool Equals(ProjectPackagesFile other)
        {
            return ProjectName == other.ProjectName &&
                PackagesFilePath == other.PackagesFilePath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj) || obj.GetType() != GetType())
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Equals((ProjectPackagesFile)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException($"{nameof(ProjectPackagesFile)} is not intended to be the key in a collection.");
        }

        public override string ToString()
        {
            return $"{ProjectName}: packages file: {PackagesFilePath}";
        }
    }
}
