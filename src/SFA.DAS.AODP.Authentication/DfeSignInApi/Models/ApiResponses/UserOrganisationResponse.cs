namespace SFA.DAS.AODP.Authentication.DfeSignInApi.Models.ApiResponses
{
    public class UserOrganisationResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LegalName { get; set; }
        public Category Category { get; set; }
        public string Urn { get; set; }
        public string Uid { get; set; }
        public string Upin { get; set; }
        public string Ukprn { get; set; }
        public string EstablishmentNumber { get; set; }
        public Status Status { get; set; }
        public string ClosedOn { get; set; }
        public string Address { get; set; }
        public string Telephone { get; set; }
        public string StatutoryLowAge { get; set; }
        public string StatutoryHighAge { get; set; }
        public string LegacyId { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string SourceSystem { get; set; }
        public string ProviderTypeName { get; set; }
        public string ProviderTypeCode { get; set; }
        public string GIASProviderType { get; set; }
        public string PIMSProviderType { get; set; }
        public string PIMSProviderTypeCode { get; set; }
        public string PIMSStatusName { get; set; }
        public string PimsStatus { get; set; }
        public string GIASStatusName { get; set; }
        public string GIASStatus { get; set; }
        public string MasterProviderStatusName { get; set; }
        public string MasterProviderStatusCode { get; set; }
        public string OpenedOn { get; set; }
        public string DistrictAdministrativeName { get; set; }
        public string DistrictAdministrativeCode { get; set; }
        public string DistrictAdministrative_code { get; set; }
        public string IsOnAPAR { get; set; }

    }
    public class Category
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class Status
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string TagColor { get; set; }
    }
}