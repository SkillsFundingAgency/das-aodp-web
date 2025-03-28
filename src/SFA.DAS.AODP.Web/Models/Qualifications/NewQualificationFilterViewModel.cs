namespace SFA.DAS.AODP.Web.Models.Qualifications
{
    public class NewQualificationFilterViewModel
    {
        public NewQualificationFilterViewModel()
        {
            Organisation =string.Empty;
            QualificationName = string.Empty;
            QAN = string.Empty;
        }

        public string Organisation {  get; set; }
        public string QualificationName { get; set; }
        public string QAN { get; set; }
        public Guid? ProcessStatusId { get; set; }
    }
}