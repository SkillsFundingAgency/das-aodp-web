using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Application.Queries.Qualifications;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.Models.Qualifications;

namespace SFA.DAS.AODP.Web.Helpers.QanHelper
{
    public class QanLookupHelper : IQanLookupHelper
    {
        private readonly IMediator _mediator;
        private readonly IOptions<AodpConfiguration> _aodpConfiguration;
        private readonly ILogger<QanLookupHelper> _logger;

        public QanLookupHelper(
            IMediator mediator,
            IOptions<AodpConfiguration> aodpConfiguration,
            ILogger<QanLookupHelper> logger)
        {
            _mediator = mediator;
            _aodpConfiguration = aodpConfiguration;
            _logger = logger;
        }

        /// <summary>
        /// Redirects to ofqual register if the Qan is valid.
        /// </summary>
        /// <param name="area"></param>
        /// <param name="controller"></param>
        /// <param name="qan"></param>
        /// <returns>The correct ofqual register url. Otherwise returns the QanInvalid view</returns>
        public async Task<IActionResult> RedirectToRegisterIfQanIsValid(string area, string controller, string qan)
        {
            var mediatorResponse = await _mediator.Send(new GetQualificationDetailsQuery { QualificationReference = qan });

            // If mediatorResponse or its payload is null treat as invalid QAN
            if (mediatorResponse == null || mediatorResponse.Value == null || mediatorResponse.Success == false)
            {
                var invalidViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = qan
                };
                invalidViewData["AreaName"] = area;
                invalidViewData["ControllerName"] = controller;

                return new ViewResult
                {
                    ViewName = "QanInvalid",
                    ViewData = invalidViewData
                };
            }

            var ofqualUrl = $"{_aodpConfiguration.Value.FindRegulatedQualificationUrl}{qan}";
            return new RedirectResult(ofqualUrl);

        }
    }
}