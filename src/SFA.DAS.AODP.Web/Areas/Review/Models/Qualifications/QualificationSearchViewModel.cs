using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;
using SFA.DAS.AODP.Web.Validators.Attributes;
using SFA.DAS.AODP.Web.Validators.Patterns;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Qualifications;

public class QualificationSearchViewModel : IValidatableObject
{
    public QualificationSearchViewModel()
    {
        Qualifications = new List<QualificationSearchResultViewModel>();
        Pagination = new PaginationViewModel();
    }

    [Display(Name = "Qualification title or QAN")]
    [Required(ErrorMessage = "Enter a search term")]
    [AllowedCharacters(TextCharacterProfile.FreeText)]
    public string SearchTerm { get; set; } = string.Empty; 

    // paging
    public PaginationViewModel Pagination { get; set; }

    public List<QualificationSearchResultViewModel> Qualifications { get; set; }

    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }

    public static QualificationSearchViewModel Map(GetQualificationsQueryResponse response, List<GetProcessStatusesQueryResponse.ProcessStatus> processStatuses, string searchTerm = "")
    {
        var vm = new QualificationSearchViewModel()
        {
            SearchTerm = searchTerm,
            Pagination = new PaginationViewModel(response.TotalRecords, response.Skip, response.Take),
            Qualifications = response.Qualifications.Select(d => new QualificationSearchResultViewModel
            {
                Qan = d.Qan,
                QualificationName = d.QualificationName,
                Status = processStatuses.FirstOrDefault(ps => ps.Id == d.Status)?.Name

            }).ToList()
        };

        return vm;
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var term = SearchTerm?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(term))
        {
            yield break;
        }

        const int MinLength = 5;
        if (term.Length < MinLength)
        {
            var shortBy = MinLength - term.Length;
            yield return new ValidationResult(
                $"Search term is {shortBy} character(s) too short.",
                new[] { nameof(SearchTerm) }
            );
        }
    }
}

public class QualificationSearchResultViewModel
{
    public string? Qan { get; set; }
    public string? QualificationName { get; set; }
    public string? Status { get; set; }
}
