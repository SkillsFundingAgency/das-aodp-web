using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Qualifications;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Helpers.User;
using SFA.DAS.AODP.Web.Models.Qualifications.Fundings;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{
    [Area("Review")]
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
        [Route("review/qualifications/{qualificationReference}/qualification-funding-offers-outcome")]
        public async Task<IActionResult> QualificationFundingOffersOutcome(string qualificationReference)
        {
            var qualificationVersions = await Send(new GetQualificationVersionsForQualificationByReferenceQuery(qualificationReference));
            var model = new QualificationFundingsOffersOutcomeViewModel
            {
                QualificationReference = qualificationReference
            };
            if (qualificationVersions != null && qualificationVersions.QualificationVersionsList.Count != 0)
            {
                var latestQualificationVersion = qualificationVersions.QualificationVersionsList
                                                  .OrderByDescending(q => q.Version)
                                                  .FirstOrDefault();
                if (latestQualificationVersion != null)
                {
                    var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(latestQualificationVersion.Id));
                    model.QualificationReference = qualificationReference;
                    model.QualificationId = latestQualificationVersion.QualificationId;
                    model.QualificationVersionId = latestQualificationVersion.Id;
                    model.Approved = feedbackForQualificationFunding?.Approved;
                    model.Comments = feedbackForQualificationFunding?.Comments;
                }
            }
            return View(model);
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationReference}/qualification-funding-offers-outcome")]
        public async Task<IActionResult> QualificationFundingOffersOutcome(QualificationFundingsOffersOutcomeViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            await Send(new SaveQualificationsFundingOffersOutcomeCommand()
            {
                QualificationVersionId = model.QualificationVersionId,
                QualificationId = model.QualificationId,
                Approved = model.Approved == true,
                Comments = model.Comments,
                QualificationReference = model.QualificationReference,
                UserDisplayName = _userHelperService.GetUserDisplayName(),
                ActionTypeId = new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])
            });

            if (model.Approved == true)
            {
                return RedirectToAction(nameof(QualificationFundingOffers), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId });
            }

            return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationReference}/qualification/{qualificationVersionId}/funding-offers/{qualificationId}")]
        public async Task<IActionResult> QualificationFundingOffers(string qualificationReference, Guid qualificationVersionId, Guid qualificationId)
        {
            var offers = await Send(new GetFundingOffersQuery());
            var review = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

            QualificationFundingsOffersSelectViewModel model = new()
            {
                SelectedOfferIds = review.QualificationFundedOffers?.Select(s => s.FundingOfferId).ToList() ?? [],
                QualificationVersionId = qualificationVersionId,
                QualificationReference = qualificationReference,
                QualificationId = qualificationId
            };

            model.MapOffers(offers);

            return View(model);
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationReference}/qualification/{qualificationVersionId}/funding-offers/{qualificationId}")]
        public async Task<IActionResult> QualificationFundingOffers(QualificationFundingsOffersSelectViewModel model, string qualificationReference)
        {
            if (!ModelState.IsValid) return View(model);

            await Send(new SaveQualificationsFundingOffersCommand()
            {
                QualificationVersionId = model.QualificationVersionId,
                SelectedOfferIds = model.SelectedOfferIds,
                QualificationReference = model.QualificationReference,
                QualificationId = model.QualificationId,
                UserDisplayName = _userHelperService.GetUserDisplayName(),
                ActionTypeId = new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])
            });

            if (model.SelectedOfferIds.Count == 0)
            {
                return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId });
            }

            return RedirectToAction(nameof(QualificationFundingOffersDetails), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationReference}/qualification/{qualificationVersionId}/funding-offers-details/{qualificationId}")]
        public async Task<IActionResult> QualificationFundingOffersDetails(string qualificationReference, Guid qualificationVersionId, Guid qualificationId)
        {
            var offers = await Send(new GetFundingOffersQuery());
            var review = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

            var model = QualificationFundingsOfferDetailsViewModel.Map(review, offers);
            model.QualificationId = qualificationId;

            return View(model);
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationReference}/qualification/{qualificationVersionId}/funding-offers-details/{qualificationId}")]
        public async Task<IActionResult> QualificationFundingOffersDetails(QualificationFundingsOfferDetailsViewModel model, Guid qualificationId)
        {
            if (!ModelState.IsValid)
            {
                var offers = await Send(new GetFundingOffersQuery());
                model.MapOffers(offers);

                return View(model);
            }

            await Send(QualificationFundingsOfferDetailsViewModel.Map(model, _userHelperService.GetUserDisplayName(), new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])));

            return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationReference}/qualification/{qualificationVersionId}/funding-offers-summary/{qualificationId}")]
        public async Task<IActionResult> QualificationFundingOffersSummary(string qualificationReference, Guid qualificationVersionId, Guid qualificationId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

                var model = QualificationFundingsOffersSummaryViewModel.Map(review, offers);
                model.QualificationVersionId = qualificationVersionId;
                model.QualificationReference = review.QualificationReference;
                model.QualificationId = qualificationId;

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }
        
        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationReference}/qualification/{qualificationVersionId}/funding-offers-summary/{qualificationId}")]
        public async Task<IActionResult> QualificationFundingOffersSummary(QualificationFundingsOffersSummaryViewModel model, string qualificationReference)
        {
            await Send(new CreateQualificationDiscussionHistoryCommand()
            {
                QualificationVersionId = model.QualificationVersionId,
                QualificationId = model.QualificationId,
                QualificationReference = model.QualificationReference,
                UserDisplayName = _userHelperService.GetUserDisplayName(),
                ActionTypeId = new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])
            });

            return RedirectToAction(nameof(QualificationFundingOffersConfirmation), new { qualificationReference = model.QualificationReference });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationReference}/qualification-funding-offers-confirm")]
        public async Task<IActionResult> QualificationFundingOffersConfirmation(string qualificationReference)
        {
            return View(new QualificationFundingsConfirmationViewModel()
            {
                QualificationReference = qualificationReference
            });
        }
    }
}
