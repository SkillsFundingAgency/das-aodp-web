using SFA.DAS.AODP.Application.Queries.OutputFile;
using System.Globalization;

namespace SFA.DAS.AODP.Web.Models.OutputFile;

public class GenerateViewModel
{
    public List<File> GeneratedFiles { get; set; } = new List<File>();
    public FormActions AdditionalFormActions { get; set; } = new FormActions();
    public static implicit operator GenerateViewModel(GetPreviousOutputFilesQueryResponse response)
    {
        return new GenerateViewModel
        {
            GeneratedFiles = [..response.GeneratedFiles]
        };
    }
    public class FormActions
    {
        public bool GenerateFile { get; set; }
    }
    public class File
    {
        public string DisplayName { get; set; } = string.Empty;
        public string? BlobName { get; set; }
        public bool IsInProgress { get; set; }
        public DateTime? Created { get; set; }
        public string FormattedTimestamp
        {
            get => Created is null ? "" :
                Created.Value.ToString("dd MMM yyyy", CultureInfo.InvariantCulture)
                    + " at "
                    + Created.Value.ToString("HH:mm", CultureInfo.InvariantCulture);
        }

        public static implicit operator File(GetPreviousOutputFilesQueryResponse.File file)
        {
            return new File
            {
                DisplayName = file.DisplayName,
                BlobName = file.BlobName,
                IsInProgress = file.IsInProgress,
                Created = file.Created,
            };
        }
    }
}
