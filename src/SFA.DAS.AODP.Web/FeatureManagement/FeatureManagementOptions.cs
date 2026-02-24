using System.Diagnostics.CodeAnalysis;
using Microsoft.FeatureManagement;

namespace SFA.DAS.AODP.Web.FeatureManagement;

/// <summary>
/// Defines feature flags to be uses with the <see cref="FeatureManager"/>, this is a slightly different approach in that I am hard coding the features rather than going via config.
/// </summary>
[ExcludeFromCodeCoverage]
internal record FeatureManagementOptions
{
    public const string SectionName = "FeatureManagement";

    // Left blank intentionally as we may use this in future for short term feature flagging.
}