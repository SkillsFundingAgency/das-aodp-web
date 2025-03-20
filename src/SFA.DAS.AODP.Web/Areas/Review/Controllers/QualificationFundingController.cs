using CsvHelper.Configuration;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications;
using SFA.DAS.AODP.Web.Models.Qualifications.Fundings;
using System.Globalization;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
    [Route("{controller}/{action}")]
    [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
    public class QualificationFundingController : ControllerBase
    {
        private readonly ILogger<QualificationFundingController> _logger;
        private readonly IMediator _mediator;
        public enum NewQualDataKeys { InvalidPageParams, }
        private readonly IUserHelperService _userHelperService;
        public QualificationFundingController(ILogger<QualificationFundingController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _logger = logger;
            _mediator = mediator;
            _userHelperService = userHelperService;
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("QualificationFunding/{qualificationReference}/qualification-funding-offers-outcome")]
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
        [Route("QualificationFunding/{qualificationVersionId}/qualification-funding-offers-outcome")]
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
        [Route("QualificationFunding/QualificationDetails/{qualificationVersionId}/qualification-funding-offers")]
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
        [Route("QualificationFunding/QualificationDetails/{qualificationVersionId}/qualification-funding-offers")]
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
        [Route("QualificationFunding/{qualificationVersionId}/qualification-funding-offers-details")]
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
        [Route("QualificationFunding/{qualificationVersionId}/qualification-funding-offers-details")]
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
        [Route("QualificationFunding/{qualificationVersionId}/qualification-funding-offers-summary")]
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
        [Route("QualificationFunding/{qualificationVersionId}/qualification-funding-offers-summary")]
        public async Task<IActionResult> QualificationFundingOffersSummary(QualificationFundingsOffersSummaryViewModel model)
        {
            try
            {
                await Send(new CreateQualificationDiscussionHistoryCommand()
                {
                    QualificationVersionId = model.QualificationVersionId,
                    QualificationReference = model.QualificationReference,
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
        [Route("QualificationFunding/{qualificationReference}/qualification-funding-offers-confirm")]
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
    }
}
