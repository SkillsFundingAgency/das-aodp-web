using DocumentFormat.OpenXml.EMMA;
using SFA.DAS.AODP.Application.Helpers;
using SFA.DAS.AODP.Web.Helpers.File;
using SFA.DAS.AODP.Web.Models.Import;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Admin.Models;

public enum UploadSource
{
    Pldns,
    DefundingList
}

public class UploadImportFileViewModel : IValidatableObject
{
    [Required(ErrorMessage = "You must select an .xlsx file")]
    [Display(Name = "You must select an .xlsx file")]
    public required IFormFile File { get; set; }

    [Required]
    public UploadSource Source { get; set; }

    private const string GenericDefundingListErrorMessage = "The file you provided does not match the required format for a defunding list.";
    private const string GenericPldnsErrorMessage = "The file you provided does not match the required format for a PLDNS list.";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (File == null)
        {
            yield break;
        }

        var allowedExtensions = new[] { ".xlsx" };
        var ext = Path.GetExtension(File.FileName) ?? string.Empty;
        if (!allowedExtensions.Contains(ext.ToLowerInvariant()))
        {
            yield return new ValidationResult("This must be an .xlsx file", new[] { nameof(File) });
            yield break;
        }

        if (File == null
                    || File.Length == 0
                    || !Path.GetExtension(File.FileName ?? string.Empty).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("You must select an .xlsx file.", new[] { nameof(File) });
            yield break;
        }

        var fileValidationService = validationContext.GetService(typeof(IMessageFileValidationService)) as IMessageFileValidationService;

        if (fileValidationService == null)
        {
            yield return new ValidationResult(GenericDefundingListErrorMessage, new[] { nameof(File) });
            yield break;
        }

        bool isValid;
        try
        {
            if (Source == UploadSource.DefundingList)
            {
                var headerKeywords = new[] { "qualification", "qan", "title", "award", "guided", "sector", "route", "funding", "in scope", "comments" };

                isValid = fileValidationService
                    .ValidateImportFile(
                        file: File,
                        fileName: File.FileName,
                        headerKeywords: headerKeywords,
                        importFileValidationOptions: new ImportFileValidationOptions
                        {
                            TargetSheetName = "Approval not extended",
                            DefaultRowIndex = 6,
                            MinMatches = 2,
                            MapColumns = MapDefundingListColumns
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
            else
            {
                var headerKeywords = new[] {
                    "text qan","list updated","note",
                    "pldns 14-16","pldns 16-19","pldns local flex",
                    "legal entitlement","digital entitlement","esf l3/l4",
                    "pldns loans","lifelong learning entitlement","level 3 free courses",
                    "pldns cof","start date"
                };

                isValid = fileValidationService
                    .ValidateImportFile(
                        file: File,
                        fileName: File.FileName,
                        headerKeywords: headerKeywords,
                        importFileValidationOptions: new ImportFileValidationOptions
                        {
                            TargetSheetName = "PLDNS V12F",
                            DefaultRowIndex = 1,
                            MinMatches = 1,
                            MapColumns = MapPldnsColumns
                        },
                        CancellationToken.None)
                    .GetAwaiter()
                    .GetResult();
            }
        }
        catch
        {
            yield break;
        }

        if (!isValid)
        {
            yield return new ValidationResult(Source == UploadSource.DefundingList ? GenericDefundingListErrorMessage : GenericPldnsErrorMessage, new[] { nameof(File) });
        }
    }

    private static PldnsColumnNames MapPldnsColumns(IDictionary<string, string> headerMap)
    {
        return new PldnsColumnNames(
            ImportHelper.FindColumn(headerMap, "text QAN"),
            ImportHelper.FindColumn(headerMap, "Date PLDNS list updated", "list updated"),
            ImportHelper.FindColumn(headerMap, "NOTE", "Notes"),
            ImportHelper.FindColumn(headerMap, "PLDNS 14-16"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS 14-16"),
            ImportHelper.FindColumn(headerMap, "PLDNS 16-19"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS 16-19"),
            ImportHelper.FindColumn(headerMap, "PLDNS Local flex"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Local flex"),
            ImportHelper.FindColumn(headerMap, "PLDNS Legal entitlement L2/L3"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Legal entitlement L2/L3"),
            ImportHelper.FindColumn(headerMap, "PLDNS Legal entitlement Eng/Maths"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Legal entitlement Eng/Maths"),
            ImportHelper.FindColumn(headerMap, "PLDNS Digital entitlement"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Digital entitlement"),
            ImportHelper.FindColumn(headerMap, "PLDNS ESF L3/L4"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS ESF L3/L4"),
            ImportHelper.FindColumn(headerMap, "PLDNS Loans"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Loans"),
            ImportHelper.FindColumn(headerMap, "PLDNS Lifelong Learning Entitlement"),
            ImportHelper.FindColumn(headerMap, "NOTES PLDNS Lifelong Learning Entitlement"),
            ImportHelper.FindColumn(headerMap, "PLDNS Level 3 Free Courses for Jobs"),
            ImportHelper.FindColumn(headerMap, "Level 3 Free Courses for Jobs (Previously known as National skills fund L3 extended entitlement)"),
            ImportHelper.FindColumn(headerMap, "PLDNS CoF"),
            ImportHelper.FindColumn(headerMap, "NOTES  PLDNS CoF"),
            ImportHelper.FindColumn(headerMap, "Start date"),
            ImportHelper.FindColumn(headerMap, "NOTES Start date")
        );
    }

    private static DefundingListColumnNames MapDefundingListColumns(IDictionary<string, string> headerMap)
    {
        return new DefundingListColumnNames(
            ImportHelper.FindColumn(headerMap, "Qualification number"),
            ImportHelper.FindColumn(headerMap, "Title"),
            ImportHelper.FindColumn(headerMap, "Awarding organisation"),
            ImportHelper.FindColumn(headerMap, "Guided Learning Hours"),
            ImportHelper.FindColumn(headerMap, "Sector Subject Area Tier 2"),
            ImportHelper.FindColumn(headerMap, "Relevant route"),
            ImportHelper.FindColumn(headerMap, "Funding offer"),
            ImportHelper.FindColumn(headerMap, "In Scope"),
            ImportHelper.FindColumn(headerMap, "Comments")
        );
    }
}
