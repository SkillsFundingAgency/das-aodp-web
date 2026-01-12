using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Infrastructure.Cache;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Constants;
using SFA.DAS.AODP.Web.Extensions;
using SFA.DAS.AODP.Web.Models.OutputFile;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PolicyConstants.IsAdminImportUser)]
    public class OutputFileController : ControllerBase
    {
        #region Success and Failure Messaging
        public const string OutputFileDefaultErrorMessage =
            @"Something went wrong while generating the output file.

            Please try again later. If the problem continues, contact support.";
        public const string OutputFileNoDataErrorMessage =
            @"There is no data available to create an output file for this cycle because no applications have been reviewed yet. 

            Try again when you've processed one or more applications.";

        public const string OutputFileSuccessMessage = "Your file has been downloaded";

        #endregion

        private readonly IValidator<OutputFileViewModel> _outputModelValidator;
        private readonly ICacheService _cacheService;

        public OutputFileController(ILogger<OutputFileController> logger, IMediator mediator, IValidator<OutputFileViewModel> validator, ICacheService cacheService) : base(mediator, logger)
        { 
            _outputModelValidator = validator;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View(await BuildIndexViewModelAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOutputFile(
            OutputFileViewModel vm)
        {
            ValidationResult validation = await _outputModelValidator.ValidateAsync(vm);

            if (!validation.IsValid)
            {
                validation.AddToModelState(ModelState);

                var viewModel = await BuildIndexViewModelAsync();
                viewModel.DateChoice = vm.DateChoice;
                viewModel.Day = vm.Day;
                viewModel.Month = vm.Month;
                viewModel.Year = vm.Year;

                return View("Index", viewModel);
            }

            DateTime publicationDate = vm.DateChoice == PublicationDateMode.Today
                ? DateTime.UtcNow.Date
                : (vm.ParseDate(out var parsed) ? parsed : DateTime.UtcNow.Date);

            var result = await _mediator.Send(
                new GetQualificationOutputFileQuery(HttpContext.User?.Identity?.Name!, publicationDate));

            if (result == null || !result.Success)
            {
                TempData[OutputFileTempDataKeys.Failed] = true;

                TempData[OutputFileTempDataKeys.FailedMessage] =
                    result?.ErrorCode == ErrorCodes.NoData
                    ? OutputFileNoDataErrorMessage
                    : OutputFileDefaultErrorMessage;

                return RedirectToAction(nameof(Index));
            }

            var token = Guid.NewGuid().ToString("n");
            await _cacheService.SetAsync($"download:{token}", result.Value); 

            TempData[OutputFileTempDataKeys.Success] = true;
            TempData[OutputFileTempDataKeys.SuccessMessage] = OutputFileSuccessMessage;
            TempData[OutputFileTempDataKeys.SuccessToken] = token;

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Download(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return NotFound();

            var file = await _cacheService.GetAsync<GetQualificationOutputFileResponse>($"download:{token}");
            if (file is null) return NotFound();

             await _cacheService.RemoveAsync($"download:{token}");

            return File(file.FileContent, file.ContentType, file.FileName);
        }

        private async Task<OutputFileViewModel> BuildIndexViewModelAsync()
        {
            var logsEnvelope = await _mediator.Send(new GetQualificationOutputFileLogQuery());
            var logs = logsEnvelope.Value?.OutputFileLogs 
                ?? Enumerable.Empty<GetQualificationOutputFileLogResponse.QualificationOutputFileLog>();
            
            return new OutputFileViewModel
            {
                OutputFileLogs = logs.Select(x => new OutputFileLogModel
                {
                    UserDisplayName = x.UserDisplayName,
                    DownloadDate = x.DownloadDate,
                    PublicationDate = x.PublicationDate
                }).ToList()
            };
        }

    }
}
