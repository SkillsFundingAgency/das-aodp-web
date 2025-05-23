﻿using MediatR;
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
        public async Task<IActionResult> QualificationFundingOffersOutcome(string qualificationReference, [FromQuery] string mode)
        {
            var qualificationVersions = await Send(new GetQualificationVersionsForQualificationByReferenceQuery(qualificationReference));
            var model = new QualificationFundingsOffersOutcomeViewModel
            {
                QualificationReference = qualificationReference,
                Mode = mode,
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
                return RedirectToAction(nameof(QualificationFundingOffers), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId, mode = model.Mode });
            }

            return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId, mode = model.Mode });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationId}/qualificationVersion/{qualificationVersionId}/funding-offers/{qualificationReference}")]
        public async Task<IActionResult> QualificationFundingOffers(string qualificationReference, Guid qualificationVersionId, Guid qualificationId, [FromQuery] string mode)
        {
            var fundingOffers = await Send(new GetFundingOffersQuery());
            var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

            QualificationFundingsOffersSelectViewModel model = new()
            {
                SelectedOfferIds = feedbackForQualificationFunding.QualificationFundedOffers?.Select(s => s.FundingOfferId).ToList() ?? [],
                QualificationVersionId = qualificationVersionId,
                QualificationReference = qualificationReference,
                QualificationId = qualificationId,
                Mode = mode,
            };

            model.MapOffers(fundingOffers);

            return View(model);
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationId}/qualificationVersion/{qualificationVersionId}/funding-offers/{qualificationReference}")]
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
                return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId, mode = model.Mode });
            }

            return RedirectToAction(nameof(QualificationFundingOffersDetails), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId, mode = model.Mode });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationId}/qualificationVersion/{qualificationVersionId}/funding-offers-details/{qualificationReference}")]
        public async Task<IActionResult> QualificationFundingOffersDetails(string qualificationReference, Guid qualificationVersionId, Guid qualificationId, [FromQuery] string mode)
        {
            var fundingOffers = await Send(new GetFundingOffersQuery());
            var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

            var model = QualificationFundingsOfferDetailsViewModel.Map(feedbackForQualificationFunding, fundingOffers);
            model.QualificationId = qualificationId;
            model.Mode = mode;

            return View(model);
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationId}/qualificationVersion/{qualificationVersionId}/funding-offers-details/{qualificationReference}")]
        public async Task<IActionResult> QualificationFundingOffersDetails(QualificationFundingsOfferDetailsViewModel model, Guid qualificationId)
        {
            if (!ModelState.IsValid)
            {
                var fundingOffers = await Send(new GetFundingOffersQuery());
                model.MapOffers(fundingOffers);

                return View(model);
            }

            await Send(QualificationFundingsOfferDetailsViewModel.Map(model, _userHelperService.GetUserDisplayName(), new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])));

            return RedirectToAction(nameof(QualificationFundingOffersSummary), new { qualificationVersionId = model.QualificationVersionId, qualificationReference = model.QualificationReference, qualificationId = model.QualificationId, mode = model.Mode });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationId}/qualificationVersion/{qualificationVersionId}/funding-offers-summary/{qualificationReference}")]
        public async Task<IActionResult> QualificationFundingOffersSummary(string qualificationReference, Guid qualificationVersionId, Guid qualificationId, [FromQuery] string mode)
        {
            try
            {
                var fundingOffers = await Send(new GetFundingOffersQuery());
                var feedbackForQualificationFunding = await Send(new GetFeedbackForQualificationFundingByIdQuery(qualificationVersionId));

                var model = QualificationFundingsOffersSummaryViewModel.Map(feedbackForQualificationFunding, fundingOffers);
                model.QualificationVersionId = qualificationVersionId;
                model.QualificationReference = feedbackForQualificationFunding.QualificationReference;
                model.QualificationId = qualificationId;
                model.Mode = mode;

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }
        
        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/qualifications/{qualificationId}/qualificationVersion/{qualificationVersionId}/funding-offers-summary/{qualificationReference}")]
        public async Task<IActionResult> QualificationFundingOffersSummary(QualificationFundingsOffersSummaryViewModel model, string qualificationReference)
        {
            await Send(new CreateQualificationDiscussionHistoryNoteForFundingOffersCommand()
            {
                QualificationVersionId = model.QualificationVersionId,
                QualificationId = model.QualificationId,
                QualificationReference = model.QualificationReference,
                UserDisplayName = _userHelperService.GetUserDisplayName(),
                ActionTypeId = new Guid(ActionTypeDisplay.Dictionary[ActionType.ActionRequired])
            });

            return RedirectToAction(nameof(QualificationFundingOffersConfirmation), new { qualificationReference = model.QualificationReference, mode = model.Mode });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/qualifications/{qualificationReference}/qualification-funding-offers-confirm")]
        public async Task<IActionResult> QualificationFundingOffersConfirmation(string qualificationReference, [FromQuery] string mode)
        {
            return View(new QualificationFundingsConfirmationViewModel()
            {
                QualificationReference = qualificationReference,
                Mode = mode,
            });
        }
    }
}
