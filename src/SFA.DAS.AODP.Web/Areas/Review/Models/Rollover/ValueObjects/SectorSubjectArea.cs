namespace SFA.DAS.AODP.Web.Areas.Review.Models.Rollover.ValueObjects;

public record SectorSubjectArea
{
    public static readonly SectorSubjectArea MedicineAndDentistry = new("1.1", "Medicine and Dentistry");
    public static readonly SectorSubjectArea NursingAndAllied = new("1.2", "Nursing and subjects and vocations allied to medicine");
    public static readonly SectorSubjectArea HealthAndSocialCare = new("1.3", "Health and social care");
    public static readonly SectorSubjectArea PublicServices = new("1.4", "Public services");
    public static readonly SectorSubjectArea ChildDevelopment = new("1.5", "Child development and well-being");
    public static readonly SectorSubjectArea Science = new("2.1", "Science");
    public static readonly SectorSubjectArea MathematicsAndStatistics = new("2.2", "Mathematics and statistics");
    public static readonly SectorSubjectArea Agriculture = new("3.1", "Agriculture");
    public static readonly SectorSubjectArea HorticultureAndForestry = new("3.2", "Horticulture and forestry");
    public static readonly SectorSubjectArea AnimalCareAndVeterinary = new("3.3", "Animal care and veterinary science");
    public static readonly SectorSubjectArea EnvironmentalConservation = new("3.4", "Environmental conservation");
    public static readonly SectorSubjectArea Engineering = new("4.1", "Engineering");
    public static readonly SectorSubjectArea ManufacturingTechnologies = new("4.2", "Manufacturing technologies");
    public static readonly SectorSubjectArea TransportationOperations = new("4.3", "Transportation operations and maintenance");
    public static readonly SectorSubjectArea Architecture = new("5.1", "Architecture");
    public static readonly SectorSubjectArea BuildingAndConstruction = new("5.2", "Building and construction");
    public static readonly SectorSubjectArea UrbanRegionalPlanning = new("5.3", "Urban, rural and regional planning");
    public static readonly SectorSubjectArea DigitalTechnologyPractitioners = new("6.1", "Digital technology (practitioners)");
    public static readonly SectorSubjectArea DigitalTechnologyUsers = new("6.2", "Digital technology (users)");
    public static readonly SectorSubjectArea RetailingAndWholesaling = new("7.1", "Retailing and wholesaling");
    public static readonly SectorSubjectArea WarehousingAndDistribution = new("7.2", "Warehousing and distribution");
    public static readonly SectorSubjectArea ServiceEnterprises = new("7.3", "Service enterprises");
    public static readonly SectorSubjectArea HospitalityAndCatering = new("7.4", "Hospitality and catering");
    public static readonly SectorSubjectArea SportLeisureRecreation = new("8.1", "Sport, leisure and recreation");
    public static readonly SectorSubjectArea TravelAndTourism = new("8.2", "Travel and tourism");
    public static readonly SectorSubjectArea PerformingArts = new("9.1", "Performing arts");
    public static readonly SectorSubjectArea CraftsCreativeArtsDesign = new("9.2", "Crafts, creative arts and design");
    public static readonly SectorSubjectArea MediaAndCommunication = new("9.3", "Media and communication");
    public static readonly SectorSubjectArea PublishingInformationServices = new("9.4", "Publishing and information services");
    public static readonly SectorSubjectArea History = new("10.1", "History");
    public static readonly SectorSubjectArea Archaeology = new("10.2", "Archaeology and archaeological sciences");
    public static readonly SectorSubjectArea Philosophy = new("10.3", "Philosophy");
    public static readonly SectorSubjectArea TheologyReligiousStudies = new("10.4", "Theology and religious studies");
    public static readonly SectorSubjectArea Geography = new("11.1", "Geography");
    public static readonly SectorSubjectArea SociologySocialPolicy = new("11.2", "Sociology and social policy");
    public static readonly SectorSubjectArea Politics = new("11.3", "Politics");
    public static readonly SectorSubjectArea Economics = new("11.4", "Economics");
    public static readonly SectorSubjectArea Anthropology = new("11.5", "Anthropology");
    public static readonly SectorSubjectArea LanguagesLiteratureBritishIsles = new("12.1", "Languages, literature and culture of the British Isles");
    public static readonly SectorSubjectArea OtherLanguagesLiteratureCulture = new("12.2", "Other languages, literature and culture");
    public static readonly SectorSubjectArea Linguistics = new("12.3", "Linguistics");
    public static readonly SectorSubjectArea TeachingAndLecturing = new("13.1", "Teaching and lecturing");
    public static readonly SectorSubjectArea DirectLearningSupport = new("13.2", "Direct learning support");
    public static readonly SectorSubjectArea FoundationsLearningLife = new("14.1", "Foundations for learning and life");
    public static readonly SectorSubjectArea PreparationForWork = new("14.2", "Preparation for work");
    public static readonly SectorSubjectArea AccountingAndFinance = new("15.1", "Accounting and finance");
    public static readonly SectorSubjectArea Administration = new("15.2", "Administration");
    public static readonly SectorSubjectArea BusinessManagement = new("15.3", "Business management");
    public static readonly SectorSubjectArea MarketingAndSales = new("15.4", "Marketing and sales");
    public static readonly SectorSubjectArea LawAndLegalServices = new("15.5", "Law and legal services");
    public static readonly SectorSubjectArea NotSpecified = new("99.9", "Not Specified");

    public string Code { get; private set; }

    public string Name { get; private set; }

    private SectorSubjectArea(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static readonly IReadOnlyList<SectorSubjectArea> All = new List<SectorSubjectArea>
    {
        MedicineAndDentistry, NursingAndAllied, HealthAndSocialCare, PublicServices, ChildDevelopment,
        Science, MathematicsAndStatistics, Agriculture, HorticultureAndForestry, AnimalCareAndVeterinary,
        EnvironmentalConservation, Engineering, ManufacturingTechnologies, TransportationOperations,
        Architecture, BuildingAndConstruction, UrbanRegionalPlanning, DigitalTechnologyPractitioners,
        DigitalTechnologyUsers, RetailingAndWholesaling, WarehousingAndDistribution, ServiceEnterprises,
        HospitalityAndCatering, SportLeisureRecreation, TravelAndTourism, PerformingArts,
        CraftsCreativeArtsDesign, MediaAndCommunication, PublishingInformationServices, History,
        Archaeology, Philosophy, TheologyReligiousStudies, Geography, SociologySocialPolicy,
        Politics, Economics, Anthropology, LanguagesLiteratureBritishIsles, OtherLanguagesLiteratureCulture,
        Linguistics, TeachingAndLecturing, DirectLearningSupport, FoundationsLearningLife,
        PreparationForWork, AccountingAndFinance, Administration, BusinessManagement,
        MarketingAndSales, LawAndLegalServices
    }.OrderBy(o => o.Name).ToList();

    private static readonly IReadOnlyDictionary<string, SectorSubjectArea> CodeLookup = All.ToDictionary(x => x.Code);

    private static readonly IReadOnlyDictionary<string, SectorSubjectArea> NameLookup = CodeLookup.Values.ToDictionary(x => x.Name);

    public static SectorSubjectArea FromTiers(string tier1, string tier2)
    {
        var lookupCode = $"{tier1}.{tier2}";
        return CodeLookup.GetValueOrDefault(lookupCode, NotSpecified);
    }

    public static SectorSubjectArea FromName(string name) => NameLookup.GetValueOrDefault(name, NotSpecified);

    public static bool TryParse(string? value, out SectorSubjectArea? result)
    {
        result = All.SingleOrDefault(x => string.Equals(x.Name, value, StringComparison.OrdinalIgnoreCase));
        return result is not null;
    }

    public override string ToString() => Name;
}