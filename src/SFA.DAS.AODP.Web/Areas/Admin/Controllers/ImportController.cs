using Azure;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Commands.Import;
using SFA.DAS.AODP.Application.Queries.Import;
using SFA.DAS.AODP.Common.Exceptions;
using SFA.DAS.AODP.Infrastructure.File;
using SFA.DAS.AODP.Web.Areas.Admin.Models;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Import;
using System.Globalization;
using System.Reflection;
using System.Threading;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class ImportController : ControllerBase
    {
        private readonly IUserHelperService _userHelperService;
        private readonly IFileService _fileService;
        public enum SendKeys { RequestFailed, JobStatusFailed }
        private const string PldnsViewPath = "~/Areas/Admin/Views/Import/Pldns.cshtml";
        private const string DefundingListViewPath = "~/Areas/Admin/Views/Import/DefundingList.cshtml";
        private const string ConfirmImportSelectionAction = nameof(ConfirmImportSelection);

        public ImportController(ILogger<ImportController> logger, IMediator mediator, IUserHelperService userHelperService, IFileService fileService) : base(mediator, logger)
        {
            _userHelperService = userHelperService;
            _fileService = fileService;
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

            if (!string.IsNullOrWhiteSpace(viewModel?.ImportType) &&
                string.Equals(viewModel.ImportType.Trim(), "Pldns", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("ImportPldns");
            }

            if (!string.IsNullOrWhiteSpace(viewModel?.ImportType) &&
                string.Equals(viewModel.ImportType.Trim(), "Defunding list", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("ImportDefundingList");
            }

            return RedirectToAction(ConfirmImportSelectionAction, viewModel);
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
                return View(ConfirmImportSelectionAction, viewModel);
            }

            var timeSubmitted = DateTime.Now;
            var userName = _userHelperService.GetUserDisplayName();
            var jobName = string.Empty;
            switch (viewModel.ImportType)
            {
                case "Regulated Qualifications": jobName = JobNames.RegulatedQualifications.ToString(); break;
                case "Funded Qualifications": jobName = JobNames.FundedQualifications.ToString(); break;
                case "Pldns": jobName = JobNames.Pldns.ToString(); break;
                case "DefundingList": jobName = JobNames.DefundingList.ToString(); break;
                default: break;
            }

            var jobAlreadyRunning = false;
            var jobRunningResponse = await Send(new GetJobRunsQuery { JobName = jobName });
            if (jobRunningResponse?.JobRuns != null && jobRunningResponse.JobRuns.Any())
            {
                var lastJobRun = jobRunningResponse.JobRuns.OrderByDescending(o => o.StartTime).FirstOrDefault();

                if (lastJobRun.Status == JobStatus.Running.ToString()
                    || lastJobRun.Status == JobStatus.RequestSent.ToString()
                    || lastJobRun.Status == JobStatus.Requested.ToString())
                {
                    jobAlreadyRunning = true;
                    userName = lastJobRun.User;
                    timeSubmitted = lastJobRun.StartTime;
                }
            }

            if (!jobAlreadyRunning)
            {
                try
                {
                    var response = await Send(new RequestJobRunCommand { JobName = jobName, UserName = userName });
                }
                catch (Exception ex)
                {
                    LogException(ex);
                    TempData[SendKeys.RequestFailed.ToString()] = true;
                    return RedirectToAction(ConfirmImportSelectionAction, new ImportRequestViewModel() { ImportType = viewModel.ImportType });
                }
            }

            var submitImportRequestViewModel = new SubmitImportRequestViewModel() 
            { 
                ImportType = viewModel.ImportType, 
                SubmittedTime = timeSubmitted, 
                UserName = userName, 
                Status = JobStatus.Requested.ToString(), 
                JobName = jobName 
            };

            return View("SubmitImportRequest", submitImportRequestViewModel);
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
                JobRun lastJobRun = new JobRun();

                if (model.JobRunId == Guid.Empty)
                {
                    var response = await Send(new GetJobRunsQuery { JobName = model.JobName });
                    if (response?.JobRuns != null && response.JobRuns.Any())
                    {
                        lastJobRun = response.JobRuns.OrderByDescending(o => o.StartTime).First();                                             
                    }
                    else
                    {
                        TempData[SendKeys.JobStatusFailed.ToString()] = true;                 
                    }
                }
                else
                {
                    var response = await Send(new GetJobRunByIdQuery(model.JobRunId));
                    if (response != null)
                    {
                        lastJobRun = new JobRun()
                        {
                            Id = response.Id,
                            EndTime = response.EndTime,
                            JobId = response.JobId,
                            RecordsProcessed = response.RecordsProcessed,
                            StartTime = response.StartTime,
                            Status = response.Status,
                            User = response.User,
                        };                        
                    }
                    else
                    {
                        TempData[SendKeys.JobStatusFailed.ToString()] = true;           
                    }
                }

                model.JobRunId = lastJobRun.Id;
                model.SubmittedTime = lastJobRun.StartTime;
                model.Status = lastJobRun.Status;
                model.UserName = lastJobRun.User;
                if (lastJobRun.Status == JobStatus.Completed.ToString())
                {
                    return RedirectToAction("Complete", new CompleteViewModel() { Status = model.Status, ImportType = model.ImportType, JobName = model.JobName, JobRunId = model.JobRunId });
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
        public async Task<IActionResult> Complete(CompleteViewModel viewModel)
        {
            try
            {
                var response = await Send(new GetJobRunByIdQuery(viewModel.JobRunId));
                if (response != null)
                {
                    viewModel.Status = response.Status;                   
                    viewModel.CompletedTime = response.EndTime ?? DateTime.Now;
                    viewModel.UserName = response.User;                    
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

            return View(viewModel);
        }


        [HttpGet]
        [Route("/admin/import/pldns")]
        public IActionResult ImportPldns()
        {
            return View(PldnsViewPath);
        }

        [HttpPost]
        [Route("/admin/import/pldns")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.QFAUImport)]
        public async Task<IActionResult> ImportPldns([FromForm] UploadImportFileViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(PldnsViewPath, model);
            }

            if (model.File == null
                    || model.File.Length == 0
                    || !Path.GetExtension(model.File.FileName ?? string.Empty).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.File), "You must select an .xlsx file.");
                return View(PldnsViewPath, model);
            }

            try
            {
                var command = new ImportPldnsCommand
                {
                    File = model.File,
                    FileName = model.File.FileName,
                };

                await Send(command);

                var folderName = JobNames.Pldns.ToString();
                var contentType = model.File.ContentType;
                var fileNamePrefix = _userHelperService.GetUserDisplayName() ?? string.Empty;

                using var stream = model.File.OpenReadStream();
                await _fileService.UploadXlsxFileAsync(folderName, "Pldns.xlsx", stream, contentType, fileNamePrefix);
            }
            catch (MediatorRequestHandlingException ex)
            {
                LogException(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(PldnsViewPath, model);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(PldnsViewPath, model);
            }

            var viewModel = new ImportRequestViewModel() { ImportType = JobNames.Pldns.ToString() };
            return RedirectToAction(ConfirmImportSelectionAction, viewModel);
        }

        [HttpGet]
        [Route("/admin/import/defunding-list")]
        public IActionResult ImportDefundingList()
        {
            return View(DefundingListViewPath);
        }

        [HttpPost]
        [Route("/admin/import/defunding-list")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConstants.QFAUImport)]
        public async Task<IActionResult> ImportDefundingList([FromForm] UploadImportFileViewModel model)
        {

            if (!ModelState.IsValid)
            {
                return View(DefundingListViewPath, model);
            }

            if (model.File == null
                    || model.File.Length == 0
                    || !Path.GetExtension(model.File.FileName ?? string.Empty).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.File), "You must select an .xlsx file.");
                return View(DefundingListViewPath, model);
            }

            try
            {
                var command = new ImportDefundingListCommand
                {
                    File = model.File,
                    FileName = model.File.FileName,
                };

                await Send(command);

                var folderName = JobNames.DefundingList.ToString();
                var contentType = model.File.ContentType;
                var fileNamePrefix = _userHelperService.GetUserDisplayName() ?? string.Empty;

                using var stream = model.File.OpenReadStream();
                await _fileService.UploadXlsxFileAsync(folderName, "DefundingList.xlsx", stream, contentType, fileNamePrefix);
            }
            catch (MediatorRequestHandlingException ex)
            {
                LogException(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(DefundingListViewPath, model);
            }
            catch (Exception ex)
            {
                LogException(ex);
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(DefundingListViewPath, model);
            }

            var viewModel = new ImportRequestViewModel() { ImportType = JobNames.DefundingList.ToString() };
            return RedirectToAction(ConfirmImportSelectionAction, viewModel);
        }
    }
}
