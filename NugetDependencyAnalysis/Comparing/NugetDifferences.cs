﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace NugetDependencyAnalysis.Comparing
{
    internal class NugetDifferences : IEquatable<NugetDifferences>
    {
        public NugetDifferences(string packageName, IReadOnlyList<VersionProjectsGrouping> versionDifferences)
        {
            PackageName = packageName;
            VersionDifferences = versionDifferences;
        }

        public string PackageName { get; }

        public IReadOnlyList<VersionProjectsGrouping> VersionDifferences { get; }

        public bool Equals(NugetDifferences other)
        {
            return PackageName == other.PackageName &&
                VersionDifferences.SequenceEqual(other.VersionDifferences);
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

            return Equals((NugetDifferences)obj);
        }

        public override int GetHashCode()
        {
            throw new InvalidOperationException($"{nameof(NugetDifferences)} is not intended to be the key in a collection.");
        }
    }
}
