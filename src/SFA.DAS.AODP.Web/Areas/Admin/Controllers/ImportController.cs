using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Import;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class ImportController : ControllerBase
    {
        private readonly IUserHelperService _userHelperService;
        public enum SendKeys { RequestFailed, JobStatusFailed }

        public ImportController(ILogger<ImportController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _userHelperService = userHelperService;
        }

        [HttpGet]
        [Route("/admin/import")]
        public IActionResult Index()
        {
            var viewModel = new ImportRequestViewModel();            
            return View(viewModel);
        }

        [HttpPost]
        [Route("/admin/import/selectimport")]
        public IActionResult SelectImport(ImportRequestViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", viewModel);
            }

            return RedirectToAction("ConfirmImportSelection", viewModel);
        }

        [HttpGet]
        [Route("/admin/import/confirmimportselection")]
        public IActionResult ConfirmImportSelection(ImportRequestViewModel viewModel)
        {
            ShowNotificationIfKeyExists(SendKeys.RequestFailed.ToString(), ViewNotificationMessageType.Error, "Request to start job failed.");
            return View(new ConfirmImportRequestViewModel() { ImportType = viewModel.ImportType });
        }

        [HttpPost]
        [Route("/admin/import/confirmimportselection")]
        public async Task<IActionResult> ConfirmImportSelection(ConfirmImportRequestViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return View("ConfirmImportSelection", viewModel);
            }

            var timeSubmitted = DateTime.Now;
            var userName = _userHelperService.GetUserDisplayName();
            var jobName = string.Empty;
            switch (viewModel.ImportType)
            {
                case "Regulated Qualifications": jobName = JobNames.RegulatedQualifications.ToString(); break;
                case "Funded Qualifications": jobName = JobNames.FundedQualifications.ToString(); break;
                default: break;
            }

            var requestStatus = $"Requested on {timeSubmitted.ToShortDateString()} at {timeSubmitted.ToShortTimeString()} by {userName}";
            try
            {
                var response = await Send(new RequestJobRunCommand { JobName = jobName, UserName = userName });                
            }
            catch (Exception ex)
            {
                LogException(ex);
                TempData[SendKeys.RequestFailed.ToString()] = true;
                return RedirectToAction("ConfirmImportSelection", new ImportRequestViewModel() { ImportType = viewModel.ImportType});
            }

            var submitImportRequestViewModel = new SubmitImportRequestViewModel() 
            { 
                ImportType = viewModel.ImportType, 
                SubmittedTime = timeSubmitted, 
                UserName = userName, 
                Status = requestStatus, 
                JobName = jobName 
            };

            return RedirectToAction("SubmitImportRequest", submitImportRequestViewModel);
        }

        [HttpGet]
        [Route("/admin/import/submitimportrequest")]
        public IActionResult SubmitImportRequest(SubmitImportRequestViewModel viewModel)
        {
            ShowNotificationIfKeyExists(SendKeys.JobStatusFailed.ToString(), ViewNotificationMessageType.Error, "Failed to retrieve job status.");
            return View(viewModel);
        }
        

        [HttpPost]
        [Route("/admin/import/checkprogress")]
        public async Task<IActionResult> CheckProgress(SubmitImportRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("SubmitImportRequest", model);
            }            

            try
            {
                var response = await Send(new GetJobQuery { JobName = model.JobName });
                if (response.Id != Guid.Empty)
                {
                    var joinWord = "at";
                    var jobTime = model.SubmittedTime;
                    if (response.Status == JobStatus.Running.ToString())
                    {
                        joinWord = "since";                                  
                    }
                    else
                    {
                        jobTime = response.LastRunTime ?? model.SubmittedTime;
                    }

                    model.Status = $"{response.Status} {joinWord} {jobTime.ToShortDateString()} {jobTime.ToShortTimeString()} by {model.UserName}";
                    
                    if (response.Status == JobStatus.Completed.ToString())
                    {
                        RedirectToAction("Complete", new CompleteViewModel() { Status = model.Status, ImportType = model.ImportType, JobName = model.JobName, SubmittedTime = model.SubmittedTime, UserName = model.UserName });
                    }
                }
                else
                {
                    TempData[SendKeys.JobStatusFailed.ToString()] = true;
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
                TempData[SendKeys.JobStatusFailed.ToString()] = true;
            }


            return View("SubmitImportRequest", model);
        }

        [HttpGet]
        [Route("/admin/import/complete")]
        public IActionResult Complete(CompleteViewModel viewModel)
        {
            return View(viewModel);
        }
    }
}
