using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Web.Models.Forms
{
    public class FormVersionList
    {
        public List<FormVersion> FormVersions { get; set; }

        public FormVersionList() {
            FormVersions = new List<FormVersion>();
        }
        public class FormVersion
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public DateTime Version { get; set; }
            public string Status { get; set; }
            public int Order { get; set; }
            public DateTime DateCreated { get; set; }
        }
    }
}
