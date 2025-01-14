using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RestEase;
using SFA.DAS.AODP.Functions.Interfaces;
using SFA.DAS.AODP.Infrastructure.Context;
using SFA.DAS.AODP.Models.Qualification;
using System.Diagnostics;

namespace SFA.DAS.AODP.Functions.Functions
{
    public class RegisteredQualificationsDataFunction
    {
        private readonly IApplicationDbContext _applicationDbContext;
        private readonly ILogger<RegisteredQualificationsDataFunction> _logger;

        public RegisteredQualificationsDataFunction(ILogger<RegisteredQualificationsDataFunction> logger, IApplicationDbContext appDbContext)
        {
            _logger = logger;
            _applicationDbContext = appDbContext;
        }

        [Function("RegisteredQualificationsDataFunction")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "qualifications")] HttpRequest req)
        {
            _logger.LogInformation($"Processing {nameof(RegisteredQualificationsDataFunction)} request...");
            try
            {
                int page = 1;
                int limit = 1000;
                int totalProcessed = 0;

                var config = LoadConfiguration();
                string subscriptionKey = config["OcpApimSubscriptionKey"];          
                if(string.IsNullOrEmpty(subscriptionKey))
                {
                    _logger.LogError("Subscription key not found in configuration.");
                    return new StatusCodeResult(500);
                }

                var api = InitializeApiClient(subscriptionKey);

                var queryParameters = ParseQueryParameters(req.Query);

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                while (true)
                {
                    var paginatedResult = await api.SearchPrivateQualificationsAsync(
                        queryParameters.Title,
                        page,
                        limit,
                        queryParameters.AssessmentMethods,
                        queryParameters.GradingTypes,
                        queryParameters.AwardingOrganisations,
                        queryParameters.Availability,
                        queryParameters.QualificationTypes,
                        queryParameters.QualificationLevels,
                        queryParameters.NationalAvailability,
                        queryParameters.SectorSubjectAreas,
                        queryParameters.MinTotalQualificationTime,
                        queryParameters.MaxTotalQualificationTime,
                        queryParameters.MinGuidedLearningHours,
                        queryParameters.MaxGuidedLearningHours
                    );

                    if (paginatedResult.Results == null || !paginatedResult.Results.Any())
                    {
                        _logger.LogInformation("No more qualifications to process.");
                        break;
                    }

                    _logger.LogInformation($"Processing page {page}. Retrieved {paginatedResult.Results.Count} qualifications.");

                    var qualifications = paginatedResult.Results.Select(q => new SFA.DAS.AODP.Data.Entities.RegisteredQualificationsImport
                    {
                        QualificationNumber = q.QualificationNumber,
                        QualificationNumberNoObliques = q.QualificationNumberNoObliques,
                        Title = q.Title,
                        Status = q.Status,
                        OrganisationName = q.OrganisationName,
                        OrganisationAcronym = q.OrganisationAcronym,
                        OrganisationRecognitionNumber = q.OrganisationRecognitionNumber,
                        Type = q.Type,
                        Ssa = q.Ssa,
                        Level = q.Level,
                        SubLevel = q.SubLevel,
                        EqfLevel = q.EqfLevel,
                        GradingType = q.GradingType,
                        GradingScale = q.GradingScale,
                        TotalCredits = q.TotalCredits,
                        Tqt = q.Tqt,
                        Glh = q.Glh,
                        MinimumGlh = q.MinimumGlh,
                        MaximumGlh = q.MaximumGlh,
                        RegulationStartDate = q.RegulationStartDate,
                        OperationalStartDate = q.OperationalStartDate,
                        OperationalEndDate = q.OperationalEndDate,
                        CertificationEndDate = q.CertificationEndDate,
                        ReviewDate = q.ReviewDate,
                        OfferedInEngland = q.OfferedInEngland,
                        OfferedInNorthernIreland = q.OfferedInNorthernIreland,
                        OfferedInternationally = q.OfferedInternationally,
                        Specialism = q.Specialism,
                        Pathways = q.Pathways,
                        AssessmentMethods = q.AssessmentMethods != null
                             ? string.Join(",", q.AssessmentMethods)
                             : null,
                        ApprovedForDelfundedProgramme = q.ApprovedForDelfundedProgramme,
                        LinkToSpecification = q.LinkToSpecification,
                        ApprenticeshipStandardReferenceNumber = q.ApprenticeshipStandardReferenceNumber,
                        ApprenticeshipStandardTitle = q.ApprenticeshipStandardTitle,
                        RegulatedByNorthernIreland = q.RegulatedByNorthernIreland,
                        NiDiscountCode = q.NiDiscountCode,
                        GceSizeEquivalence = q.GceSizeEquivalence,
                        GcseSizeEquivalence = q.GcseSizeEquivalence,
                        EntitlementFrameworkDesignation = q.EntitlementFrameworkDesignation,
                        LastUpdatedDate = q.LastUpdatedDate,
                        UiLastUpdatedDate = q.UiLastUpdatedDate,
                        InsertedDate= q.InsertedDate,
                        Version = q.Version,
                        AppearsOnPublicRegister = q.AppearsOnPublicRegister,
                        OrganisationId = q.OrganisationId,
                        LevelId = q.LevelId,
                        TypeId = q.TypeId,
                        SsaId = q.SsaId,
                        GradingTypeId = q.GradingTypeId,
                        GradingScaleId = q.GradingScaleId,
                        PreSixteen = q.PreSixteen,
                        SixteenToEighteen = q.SixteenToEighteen,
                        EighteenPlus = q.EighteenPlus,
                        NineteenPlus = q.NineteenPlus,
                        ImportStatus = "New"
                    }).ToList();

                    // Save to batch of qualifications to the database
                    //_applicationDbContext.RegisteredQualificationsImports.AddRange(qualifications);
                    //await _applicationDbContext.SaveChangesAsync(); // import process takes 4.04 mins to fetch and store 50,346 qualifcation records

                    _applicationDbContext.BulkInsertAsync(qualifications);

                    totalProcessed += qualifications.Count;
                    _logger.LogInformation($"Saved {qualifications.Count} qualifications to the database.");

                    if (paginatedResult.Results.Count < limit)
                    {
                        _logger.LogInformation("Reached the end of the results set.");
                        break;
                    }

                    page++;
                }

                stopwatch.Stop();
                _logger.LogInformation($"Total Time Taken: {stopwatch.ElapsedMilliseconds} ms");

                _logger.LogInformation($"Total qualifications processed: {totalProcessed}");
                return new OkObjectResult($"Successfully processed {totalProcessed} qualifications.");
            }
            catch (ApiException ex)
            {
                _logger.LogError($"");
                return new StatusCodeResult((int)ex.StatusCode);
            }
            catch (SystemException ex)
            {
                _logger.LogError($"");
                return new StatusCodeResult(500);
            }
        }

        private IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
        }

        private IOfqualRegisterApi InitializeApiClient(string subscriptionKey)
        {
            const string baseUrl = "https://register-api.ofqual.gov.uk";
            var api = RestClient.For<IOfqualRegisterApi>(baseUrl);
            api.SubscriptionKey = subscriptionKey;
            return api;
        }

        private RegisteredQualificationQueryParameters ParseQueryParameters(IQueryCollection query)
        {
            return new RegisteredQualificationQueryParameters
            {
                Title = query["title"],
                PageNumber = ParseInt(query["page"], 1),
                PageSize = ParseInt(query["limit"], 10),
                AssessmentMethods = query["assessmentMethods"],
                GradingTypes = query["gradingTypes"],
                AwardingOrganisations = query["awardingOrganisations"],
                Availability = query["availability"],
                QualificationTypes = query["qualificationTypes"],
                QualificationLevels = query["qualificationLevels"],
                NationalAvailability = query["nationalAvailability"],
                SectorSubjectAreas = query["sectorSubjectAreas"],
                MinTotalQualificationTime = ParseNullableInt(query["minTotalQualificationTime"]),
                MaxTotalQualificationTime = ParseNullableInt(query["maxTotalQualificationTime"]),
                MinGuidedLearningHours = ParseNullableInt(query["minGuidedLearninghours"]),
                MaxGuidedLearningHours = ParseNullableInt(query["maxGuidedLearninghours"])
            };
        }

        private int ParseInt(string value, int defaultValue) =>
            int.TryParse(value, out var result) ? result : defaultValue;

        private int? ParseNullableInt(string value) =>
            int.TryParse(value, out var result) ? (int?)result : null;

    }
}
