namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ListAvailableFormsViewModel
    {
        public Guid OrganisationId { get; set; }
        public List<Form> Forms { get; set; }

        public class Form
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int Order { get; set; }
        }
    }
}