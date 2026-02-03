using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.Qualifications;

public class QualificationSearchViewModel
{
    public QualificationSearchViewModel()
    {
        Results = new List<QualificationSearchResultViewModel>();
        Pagination = new PaginationViewModel();
    }

    [Display(Name = "Qualification title or QAN")]
    [MinLength(5, ErrorMessage = "Qualification title or QAN must be 5 characters or more")]
    public string SearchTerm { get; set; } = string.Empty; 
    public string QAN { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 

    // paging
    public PaginationViewModel Pagination { get; set; }

    // results
    public List<QualificationSearchResultViewModel> Results { get; set; }

    public DateTime? RegulatedQualificationsLastImported { get; set; }
    public DateTime? FundedQualificationsLastImported { get; set; }

    public static QualificationSearchViewModel Map(GetQualificationsQueryResponse response, string searchTerm = "", string qan = "", string status = "")
    {
        var vm = new QualificationSearchViewModel()
        {
            SearchTerm = searchTerm,
            QAN = qan,
            Status = status,
            Pagination = new PaginationViewModel(response.TotalRecords, response.Skip, response.Take),
            Results = response.Data.Select(d => new QualificationSearchResultViewModel
            {
                Reference = d.Reference,
                Title = d.Title,
                AwardingOrganisation = d.AwardingOrganisation,
                Status = d.Status,
                AgeGroup = d.AgeGroup
            }).ToList()
        };

        return vm;
    }
}

public class QualificationSearchResultViewModel
{
    public string? Reference { get; set; }
    public string? Title { get; set; }
    public string? AwardingOrganisation { get; set; }
    public string? Status { get; set; }
    public string? AgeGroup { get; set; }
}
