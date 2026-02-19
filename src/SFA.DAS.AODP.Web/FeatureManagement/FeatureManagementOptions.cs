using System.Diagnostics.CodeAnalysis;
using Microsoft.FeatureManagement;

namespace SFA.DAS.AODP.Web.FeatureManagement;

/// <summary>
/// Defines feature flags to be uses with the <see cref="FeatureManager"/>, this is a slightly different approach in that I am hard coding the features rather than going via config.
/// This is to limit the complexity as it's a short term implementation whilst we reconcile all the merges and misordering of PRs
/// </summary>
[ExcludeFromCodeCoverage]
internal record FeatureManagementOptions
{
    public const string SectionName = "FeatureManagement";

    public bool DefundingListImport => false;

    public bool PldnsImport => false;

    public bool Rollover => false;
}