using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Web.Areas.Review.Models.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Models.Qualifications;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers;

[Area("Review")]
[Authorize(Policy = PolicyConstants.IsReviewUser)]
public class QualificationSearchController : ControllerBase
{
    public QualificationSearchController(ILogger<QualificationSearchController> logger, IMediator mediator) : base(mediator, logger)
    {
    }

    [Route("review/qualifications")]
    public async Task<IActionResult> Index(QualificationSearchViewModel model)
    {
        model ??= new QualificationSearchViewModel();

        var vm = new QualificationSearchViewModel()
        {
            SearchTerm = model.SearchTerm,
            QAN = model.QAN,
            Pagination = model.Pagination ?? new PaginationViewModel()
        };

        var hasSearchCriteria = !string.IsNullOrWhiteSpace(model.SearchTerm)
                        || !string.IsNullOrWhiteSpace(model.QAN)
                        || (model.Pagination != null && model.Pagination.CurrentPage > 1);

        if (hasSearchCriteria)
        {
            // default paging
            var take = model.Pagination?.RecordsPerPage ?? 10;
            var page = model.Pagination?.CurrentPage ?? 1;
            var skip = (page - 1) * take;

            var response = await Send(new GetQualificationsQuery
            {
                Name = string.IsNullOrWhiteSpace(model.SearchTerm) ? null : model.SearchTerm,
                QAN = string.IsNullOrWhiteSpace(model.QAN) ? null : model.QAN,
                Status = string.IsNullOrWhiteSpace(model.Status) ? null : model.Status,
                Skip = skip,
                Take = take
            });

            vm = QualificationSearchViewModel.Map(response!, model.SearchTerm, model.QAN, model.Status);
        }
        else
        {
            ModelState.Clear();
            vm.Results = [];
            vm.Pagination = vm.Pagination ?? new PaginationViewModel();
        }

        try
        {
            // regulated qualifications latest run
            var regulatedResp = await Send(new GetJobRunsQuery { JobName = JobNames.RegulatedQualifications.ToString() });
            if (regulatedResp.JobRuns.Count > 0)
            {
                var latest = regulatedResp.JobRuns
                    .OrderByDescending(j => j.EndTime ?? DateTime.MinValue)
                    .FirstOrDefault();
                vm.RegulatedQualificationsLastImported = latest?.EndTime ?? latest?.StartTime;
            }

            // funded qualifications latest run
            var fundedResp = await Send(new GetJobRunsQuery { JobName = JobNames.FundedQualifications.ToString() });
            if (fundedResp.JobRuns.Count > 0)
            {
                var latest = fundedResp.JobRuns
                    .OrderByDescending(j => j.EndTime ?? DateTime.MinValue)
                    .FirstOrDefault();
                vm.FundedQualificationsLastImported = latest?.EndTime ?? latest?.StartTime;
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
        }

        return View("Index", vm);
    }

    [HttpPost]
    [Route("review/qualifications")]
    public IActionResult Search(QualificationSearchViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.SearchTerm) || model.SearchTerm.Length < 5)
        {
            return View("Index", model);
        }

        return RedirectToAction(nameof(Index), new
        {
            searchTerm = model.SearchTerm,
            qan = model.QAN,
            status = model.Status,
            // reset page to first
            page = 1,
            recordsPerPage = model.Pagination?.RecordsPerPage ?? 10
        });
    }
}
