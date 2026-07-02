using SFA.DAS.AODP.Application.Queries.Application.Review;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Extensions;

namespace SFA.DAS.AODP.Web.Helpers.Export
{
    public static class ApplicationExportPathBuilder
    {
        public static string GetZipFileName(ApplicationExportMetadataResponse metadata)
        {
            var org = metadata.OrganisationName.SanitiseFileName();
            org = org.Length <= 50 ? org : org[..50];
            var qan = string.IsNullOrWhiteSpace(metadata.Qan)
                ? ApplicationExportConstants.NoQanFolderName
                : metadata.Qan.SanitiseFileName();
            var submission = metadata.SubmissionId.ToString().PadLeft(6, '0');

            return $"{org}_{qan}_{submission}.zip";
        }
        
    }
}
