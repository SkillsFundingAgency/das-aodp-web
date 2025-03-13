using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.AODP.Models.Application;
using SFA.DAS.AODP.Models.Users;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview;
using SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview.FundingApproval;
using SFA.DAS.AODP.Web.Authentication;
using SFA.DAS.AODP.Web.Enums;
using SFA.DAS.AODP.Web.Helpers.User;
using ControllerBase = SFA.DAS.AODP.Web.Controllers.ControllerBase;

namespace SFA.DAS.AODP.Web.Areas.Review.Controllers
{

    [Area("Review")]
    [Authorize(Policy = PolicyConstants.IsReviewUser)]
    public class ApplicationsReviewController : ControllerBase
    {
        enum UpdateKeys
        {
            SharingStatusUpdated
        }
        private readonly IUserHelperService _userHelperService;

        public ApplicationsReviewController(ILogger<ApplicationsReviewController> logger, IMediator mediator, IUserHelperService userHelperService) : base(mediator, logger)
        {
            _userHelperService = userHelperService;
        }

        [Route("review/application-reviews")]
        public async Task<IActionResult> Index(ApplicationsReviewListViewModel model)
        {

            try
            {
                string userType = _userHelperService.GetUserType().ToString();
                var response = await Send(new GetApplicationsForReviewQuery()
                {
                    ReviewUser = userType,
                    ApplicationStatuses = model.Status?.Select(s => s.ToString()).ToList(),
                    ApplicationsWithNewMessages = model.Status?.Contains(ApplicationStatus.NewMessage) == true,
                    ApplicationSearch = model.ApplicationSearch,
                    AwardingOrganisationSearch = model.AwardingOrganisationSearch,
                    Limit = model.ItemsPerPage,
                    Offset = model.ItemsPerPage * (model.Page - 1)

                });

                model.MapApplications(response);
                model.UserType = userType;
                return View(model);

            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [HttpPost]
        [Route("review/application-reviews")]
        public async Task<IActionResult> Search(ApplicationsReviewListViewModel model)
        {
            return RedirectToAction(nameof(Index), new ApplicationsReviewListViewModel()
            {
                Page = 1,
                ItemsPerPage = model.ItemsPerPage,
                ApplicationSearch = model.ApplicationSearch,
                AwardingOrganisationSearch = model.AwardingOrganisationSearch,
                Status = model.Status
            });
        }

        [Route("review/application-reviews/{applicationReviewId}")]
        public async Task<IActionResult> ViewApplication(Guid applicationReviewId)
        {
            try
            {
                var userType = _userHelperService.GetUserType();
                var review = await Send(new GetApplicationForReviewByIdQuery(applicationReviewId));

                if (userType == UserType.Ofqual && review.SharedWithOfqual == false) return NotFound();
                if (userType == UserType.SkillsEngland && review.SharedWithSkillsEngland == false) return NotFound();

                ShowNotificationIfKeyExists(UpdateKeys.SharingStatusUpdated.ToString(), ViewNotificationMessageType.Success, "The application's sharing status has been updated.");
                return View(ApplicationReviewViewModel.Map(review, userType));
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding")]
        public async Task<IActionResult> QfauFundingReviewOutcome(Guid applicationReviewId)
        {
            try
            {
                var review = await Send(new GetFeedbackForApplicationReviewByIdQuery(applicationReviewId, _userHelperService.GetUserType().ToString()));

                var model = new QfauFundingReviewOutcomeViewModel()
                {
                    ApplicationReviewId = applicationReviewId,
                    Approved = review.Status == ApplicationStatus.Approved.ToString(),
                    Comments = review.Comments,
                    NewDecision = review.Status != ApplicationStatus.Approved.ToString() && review.Status != ApplicationStatus.NotApproved.ToString(),
                };

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding")]
        public async Task<IActionResult> QfauFundingReviewOutcome(QfauFundingReviewOutcomeViewModel model)
        {
            try
            {
                if (!ModelState.IsValid) return View(model);

                await Send(new SaveQfauFundingReviewOutcomeCommand()
                {
                    ApplicationReviewId = model.ApplicationReviewId,
                    Approved = model.Approved == true,
                    Comments = model.Comments,
                });

                if (model.Approved == true)
                {
                    return RedirectToAction(nameof(QfauFundingReviewOffers), new { applicationReviewId = model.ApplicationReviewId });
                }

                return RedirectToAction(nameof(QfauFundingReviewSummary), new { applicationReviewId = model.ApplicationReviewId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding-offers")]
        public async Task<IActionResult> QfauFundingReviewOffers(Guid applicationReviewId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForApplicationReviewByIdQuery(applicationReviewId, _userHelperService.GetUserType().ToString()));

                QfauFundingReviewOutcomeOffersSelectViewModel model = new()
                {
                    SelectedOfferIds = review.FundedOffers?.Select(s => s.FundingOfferId).ToList() ?? [],
                    ApplicationReviewId = applicationReviewId,
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
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding-offers")]
        public async Task<IActionResult> QfauFundingReviewOffers(QfauFundingReviewOutcomeOffersSelectViewModel model)
        {
            try
            {
                await Send(new SaveQfauFundingReviewOffersCommand()
                {
                    ApplicationReviewId = model.ApplicationReviewId,
                    SelectedOfferIds = model.SelectedOfferIds
                });

                if (model.SelectedOfferIds.Count == 0)
                {
                    return RedirectToAction(nameof(QfauFundingReviewSummary), new { applicationReviewId = model.ApplicationReviewId });
                }

                return RedirectToAction(nameof(QfauFundingReviewOfferDetails), new { applicationReviewId = model.ApplicationReviewId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding-offers-details")]
        public async Task<IActionResult> QfauFundingReviewOfferDetails(Guid applicationReviewId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForApplicationReviewByIdQuery(applicationReviewId, _userHelperService.GetUserType().ToString()));

                var model = QfauFundingReviewOutcomeOfferDetailsViewModel.Map(review, offers);

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding-offers-details")]
        public async Task<IActionResult> QfauFundingReviewOfferDetails(QfauFundingReviewOutcomeOfferDetailsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var offers = await Send(new GetFundingOffersQuery());
                    model.MapOffers(offers);

                    return View(model);
                }

                await Send(QfauFundingReviewOutcomeOfferDetailsViewModel.Map(model));


                return RedirectToAction(nameof(QfauFundingReviewSummary), new { applicationReviewId = model.ApplicationReviewId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }


        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding-summary")]
        public async Task<IActionResult> QfauFundingReviewSummary(Guid applicationReviewId)
        {
            try
            {
                var offers = await Send(new GetFundingOffersQuery());
                var review = await Send(new GetFeedbackForApplicationReviewByIdQuery(applicationReviewId, _userHelperService.GetUserType().ToString()));

                var model = QfauFundingReviewOutcomeSummaryViewModel.Map(review, offers);
                model.ApplicationReviewId = applicationReviewId;

                return View(model);
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/application-reviews/{applicationReviewId}/qfau-funding-confirm")]
        public async Task<IActionResult> QfauFundingReviewConfirmation(Guid applicationReviewId)
        {
            return View(new QfauFundingReviewConfirmationViewModel()
            {
                ApplicationReviewId = applicationReviewId
            });
        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpGet]
        [Route("review/application-reviews/{applicationReviewId}/share")]
        public async Task<IActionResult> ShareApplicationConfirmation(Guid applicationReviewId, bool share, UserType userType)
        {
            return View(new ShareApplicationViewModel()
            {
                ApplicationReviewId = applicationReviewId,
                Share = share,
                UserType = userType
            });

        }

        [Authorize(Policy = PolicyConstants.IsInternalReviewUser)]
        [HttpPost]
        [Route("review/application-reviews/{applicationReviewId}/share")]
        public async Task<IActionResult> ShareApplicationConfirmation(ShareApplicationViewModel model)
        {
            try
            {
                await Send(
                    new UpdateApplicationReviewSharingCommand()
                    {
                        ApplicationReviewId = model.ApplicationReviewId,
                        ShareApplication = model.Share,
                        ApplicationReviewUserType = model.UserType.ToString(),
                        SentByEmail = _userHelperService.GetUserEmail(),
                        SentByName = _userHelperService.GetUserDisplayName(),
                        UserType = _userHelperService.GetUserType().ToString()
                    });

                TempData[UpdateKeys.SharingStatusUpdated.ToString()] = true;
                return RedirectToAction(nameof(ViewApplication), new { applicationReviewId = model.ApplicationReviewId });
            }
            catch
            {
                return Redirect("/Home/Error");
            }
        }
    }
}