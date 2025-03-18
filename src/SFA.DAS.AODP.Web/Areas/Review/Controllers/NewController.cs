﻿using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications.Fundings;
using System.Globalization;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
    public class NewController : ControllerBase
    {
        private readonly ILogger<NewController> _logger;
        private readonly IMediator _mediator;
        public enum NewQualDataKeys { InvalidPageParams, }
        private readonly IUserHelperService _userHelperService;
        public NewController(ILogger<NewController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            _userHelperService = userHelperService;
        }

        [Route("/Review/New/Index")]
        public async Task<IActionResult> Index(int pageNumber = 0, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            var viewModel = new NewQualificationsViewModel();
            try
            {

                if (!ModelState.IsValid || (recordsPerPage != 10 && recordsPerPage != 20 && recordsPerPage != 50) || pageNumber < 0)
                {
                    ShowNotificationIfKeyExists(NewQualDataKeys.InvalidPageParams.ToString(), ViewNotificationMessageType.Error, "Invalid parameters.");
                }

                // Initial page load will not load records and have a page number of 0
                if (pageNumber > 0)
                {
                    var query = new GetNewQualificationsQuery();
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        query.Name = name;
                    }

                    if (!string.IsNullOrWhiteSpace(organisation))
                    {
                        query.Organisation = organisation;
                    }

                    if (!string.IsNullOrWhiteSpace(qan))
                    {
                        query.QAN = qan;
                    }

                    query.Take = recordsPerPage;
                    query.Skip = recordsPerPage * (pageNumber - 1);

                    var response = await Send(query);
                    viewModel = NewQualificationsViewModel.Map(response, organisation, qan, name);
                }                                                                                  
                
                return View(viewModel);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [HttpPost]
        [Route("/Review/New/Search")]
        public async Task<IActionResult> Search(NewQualificationsViewModel viewModel)
        {
            try
            {
                    return RedirectToAction(nameof(Index), new
                    {
                        pageNumber = 1,
                        recordsPerPage = viewModel.PaginationViewModel.RecordsPerPage,
                        name = viewModel.Filter.QualificationName,
                        organisation = viewModel.Filter.Organisation,
                        qan = viewModel.Filter.QAN
                    });               
            }
            catch
            {
                return View("Index", viewModel);
            }
        }

        [HttpGet]
        [Route("/Review/New/Clear")]
        public async Task<IActionResult> Clear(int recordsPerPage = 10)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index), new
                    {
                        pageNumber = 0,
                        recordsPerPage = recordsPerPage,               
                    });
                }
                else
                {
                    return View("Index");
                }
            }
            catch
            {
                return View("Index");
            }
        }

        [HttpGet]
        [Route("/Review/New/ChangePage")]
        public async Task<IActionResult> ChangePage(int newPage = 1, int recordsPerPage = 10, string name = "", string organisation = "", string qan = "")
        {
            try
            {
                if (ModelState.IsValid)
                {
                    return RedirectToAction(nameof(Index), new
                    {
                        pageNumber = newPage,
                        recordsPerPage = recordsPerPage,
                        name = name,
                        organisation = organisation,
                        qan = qan
                    });
                }
                else
                {
                    return View("Index");
                }
            }
            catch
            {
                return View("Index");
            }
        }

        [Route("/Review/New/QualificationDetails")]
        public async Task<IActionResult> QualificationDetails([FromQuery] string qualificationReference)
        {            
            if (string.IsNullOrWhiteSpace(qualificationReference))
            {               
                return Redirect("/Home/Error");
            }

            var result = await Send(new GetQualificationDetailsQuery { QualificationReference = qualificationReference });           

            var viewModel = MapToViewModel(result);
            return View(viewModel);
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("Review/New/{qualificationReference}/qualification-funding-offers-outcome")]
        public async Task<IActionResult> QualificationFundingOffersOutcome(string qualificationReference)
        {
            try
            {
                var qualificationVersions = await Send(new GetQualificationVersionsForQualificationByReferenceQuery(qualificationReference));
                var model = new QualificationFundingsOffersOutcomeViewModel
                {
                    QualificationReference = qualificationReference
                };
                if (qualificationVersions != null &&  qualificationVersions.QualificationVersionsList.Count != 0 )
                {
                    var latestQualificationVersion = qualificationVersions.QualificationVersionsList
                                                      .OrderByDescending(q => q.Version)
                                                      .FirstOrDefault();
                    if(latestQualificationVersion != null)
                    {
                        var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(latestQualificationVersion.Id));
                        model.QualificationVersionId = latestQualificationVersion.Id;
                        model.Approved = feedbackForQualificationFunding?.Approved;
                        model.Comments = feedbackForQualificationFunding?.Comments;
                    }
                }
                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("Review/New/{qualificationVersionId}/qualification-funding-offers-outcome")]
        public async Task<IActionResult> QualificationFundingOffersOutcome(QualificationFundingsOffersOutcomeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) return View(model);

                await Send(new SaveQualificationsFundingOffersOutcomeCommand()
                {
                    QualificationVersionId = model.QualificationVersionId,
                    Approved = model.Approved == true,
                    Comments = model.Comments,
                });

                if (model.Approved == true)
                {
                    return RedirectToAction(nameof(QualificationFundingOffers), new { qualificationVersionId = model.QualificationVersionId });
                }

                return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("Review/New/QualificationDetails/{qualificationVersionId}/qualification-funding-offers")]
        public async Task<IActionResult> QualificationFundingOffers(Guid qualificationVersionId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

                QualificationFundingsOffersSelectViewModel model = new()
                {
                    SelectedOfferIds = review.QualificationFundedOffers?.Select(s => s.FundingOfferId).ToList() ?? [],
                    QualificationVersionId = qualificationVersionId,
                };

                model.MapOffers(offers);

                return View(model);

            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("Review/New/QualificationDetails/{qualificationVersionId}/qualification-funding-offers")]
        public async Task<IActionResult> QualificationFundingOffers(QualificationFundingsOffersSelectViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) return View(model);

                await Send(new SaveQualificationsFundingOffersCommand()
                {
                    QualificationVersionId = model.QualificationVersionId,
                    SelectedOfferIds = model.SelectedOfferIds
                });

                return RedirectToAction(nameof(QualificationFundingOffersDetails), new { qualificationVersionId = model.QualificationVersionId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("Review/New/{qualificationVersionId}/qualification-funding-offers-details")]
        public async Task<IActionResult> QualificationFundingOffersDetails(Guid qualificationVersionId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

                var model = QualificationFundingsOfferDetailsViewModel.Map(review, offers);

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("Review/New/{qualificationVersionId}/qualification-funding-offers-details")]
        public async Task<IActionResult> QualificationFundingOffersDetails(QualificationFundingsOfferDetailsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var offers = await Send(new GetFundingOffersQuery());
                    model.MapOffers(offers);

                    return View(model);
                }

                await Send(QualificationFundingsOfferDetailsViewModel.Map(model));


                return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("Review/New/{qualificationVersionId}/qualification-funding-offers-summary")]
        public async Task<IActionResult> QualificationFundingOffersSummary(Guid qualificationVersionId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

                var model = QualificationFundingsOffersSummaryViewModel.Map(review, offers);
                model.QualificationVersionId = qualificationVersionId;
                model.QualificationReference = review.QualificationReference;

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }
        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("Review/New/{qualificationVersionId}/qualification-funding-offers-summary")]
        public async Task<IActionResult> QualificationFundingOffersSummary(QualificationFundingsOffersSummaryViewModel model)
        {
            try
            {
                await Send(new CreateQualificationDiscussionHistoryCommand()
                {
                    QualificationVersionId = model.QualificationVersionId,
                    UserDisplayName = _userHelperService.GetUserDisplayName(),
                    ActionTypeId = new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])
                });

                return RedirectToAction(nameof(QualificationFundingOffersConfirmation), new { qualificationReference = model.QualificationReference });

            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }


        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("Review/New/{qualificationReference}/qualification-funding-offers-confirm")]
        public async Task<IActionResult> QualificationFundingOffersConfirmation(string? qualificationReference)
        {
            try
            {
                return View(new QualificationFundingsConfirmationViewModel()
                {
                    QualificationReference = qualificationReference
                });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }


        [Route("/Review/New/ExportData")]
        public async Task<IActionResult> ExportData()
        {
            try
            {
                var result = await Send(new GetNewQualificationsCsvExportQuery());
            
                if (result?.QualificationExports != null)
                {
                    return WriteCsvToResponse(result.QualificationExports);
                }
                else
                {
                    return Redirect("/Home/Error");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while generating the CSV file.");
                return Redirect("/Home/Error");
            }
        }

        private FileContentResult WriteCsvToResponse(List<QualificationExport> qualifications)
        {            
            var csvData = GenerateCsv(qualifications);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csvData);
            var fileName = $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}-NewQualificationsExport.csv";
            return File(bytes, "text/csv", fileName);            
        }

        private static string GenerateCsv(List<QualificationExport> qualifications)
        {
            using (var writer = new StringWriter())
            using (var csv = new CsvHelper.CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.WriteRecords(qualifications);
                return writer.ToString();
            }
        }      

        private static QualificationDetailsViewModel MapToViewModel(GetQualificationDetailsQueryResponse response)
        {
            if (response == null)
            {
                return null;
            }

            return new QualificationDetailsViewModel
            {
                Id = response.Id,
                QualificationVersionId = response.QualificationVersionId,
                Status = response.Status,
                Priority = response.Priority,
                Changes = response.Changes,
                QualificationReference = response.QualificationReference,
                AwardingOrganisation = response.AwardingOrganisation,
                Title = response.Title,
                QualificationType = response.QualificationType,
                Level = response.Level,
                ProposedChanges = response.ProposedChanges,
                AgeGroup = response.AgeGroup,
                Category = response.Category,
                Subject = response.Subject,
                SectorSubjectArea = response.SectorSubjectArea,
                Comments = response.Comments
            };
        }

        private class CsvExportResult
        {
            public bool Success { get; set; }
            public string? ErrorMessage { get; set; }
            public List<QualificationExport> QualificationExports { get; set; } = new List<QualificationExport>();
        }

        private class StatusValidationResult
        {
            public bool IsValid { get; set; }
            public string? ErrorMessage { get; set; }
            public string? ProcessedStatus { get; set; }
        }
    }
}
