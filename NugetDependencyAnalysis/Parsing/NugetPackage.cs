using System;

namespace NugetDependencyAnalysis.Parsing
{
    public class NugetPackage
    {
        public NugetPackage(string name, string version, string targetFramework)
        {
            Name = name;
            Version = version;
            TargetFramework = targetFramework;
        }

        public string Name { get; }

        public string Version { get; }

        public string TargetFramework { get; }

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

            return Equals((NugetPackage)obj);
        }

        protected bool Equals(NugetPackage other)
        {
            return Name == other.Name &&
                Version == other.Version &&
                TargetFramework == other.TargetFramework;
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException($"{nameof(NugetPackage)} is not intended to be the key in a collection.");
        }

        public override string ToString()
        {
            return $"{Name}, {Version}, {TargetFramework}";
        }
    }
}
