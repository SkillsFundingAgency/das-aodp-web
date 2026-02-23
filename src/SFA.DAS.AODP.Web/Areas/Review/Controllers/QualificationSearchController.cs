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
[Authorize(Policy = PolicyConstants.IsImportAndFormUser)]
public class QualificationSearchController : ControllerBase
{
    public QualificationSearchController(ILogger<QualificationSearchController> logger, IMediator mediator) : base(mediator, logger)
    {
    }

    [Route("/Review/Qualifications")]
    public async Task<IActionResult> Index(string searchTerm = "", int pageNumber = 0, int recordsPerPage = 10)
    {
        var vm = new QualificationSearchViewModel()
        {
            SearchTerm = searchTerm,
            Pagination = new PaginationViewModel()
        };
        var procStatuses = await Send(new GetProcessStatusesQuery());

        if (pageNumber > 0)
        {
            var take = recordsPerPage;
            var skip = (pageNumber - 1) * take;

            var response = await Send(new GetQualificationsQuery
            {
                SearchTerm = string.IsNullOrWhiteSpace(searchTerm) ? null : searchTerm,
                Skip = skip,
                Take = take
            });

            vm = QualificationSearchViewModel.Map(response!, procStatuses.ProcessStatuses, searchTerm);

            // Ensure pagination is set from response so view can render page links exactly like NewController
            vm.Pagination = new PaginationViewModel(response!.TotalRecords, response.Skip, response.Take)
            {
                CurrentPage = pageNumber,
                RecordsPerPage = recordsPerPage
            };
        }
        else
        {
            ModelState.Clear();
            vm.Qualifications = [];
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
    [Route("/Review/Qualifications")]
    public IActionResult Search(QualificationSearchViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Index", model);
        }

        return RedirectToAction(nameof(Index), new
        {
            searchTerm = model.SearchTerm,
            pageNumber = 1,
            recordsPerPage = model.Pagination?.RecordsPerPage ?? 10
        });
    }

    [HttpGet]
    [Route("/Review/Qualifications/ChangePage")]
    public IActionResult ChangePage(int newPage = 1, int recordsPerPage = 10, string searchTerm = "")
    {
        try
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index), new
                {
                    pageNumber = newPage,
                    recordsPerPage = recordsPerPage,
                    searchTerm = searchTerm
                });
            }
            else
            {
                return View("Index");
            }
        }
        catch (Exception ex)
        {
            LogException(ex);
            return View("Index");
        }
    }
}
