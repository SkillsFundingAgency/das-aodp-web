namespace SFA.DAS.AODP.Web.Models.Application
{
    public class ListAvailableFormsViewModel
    {
        public Guid OrganisationId { get; set; }
        public List<Form> Forms { get; set; } = new();

        public static ListAvailableFormsViewModel Map(GetApplicationFormsQueryResponse value)
        {
            ListAvailableFormsViewModel model = new();

            foreach (var form in value.Forms)
            {
                model.Forms.Add(new()
                {
                    Description = form.Description,
                    Id = form.Id,
                    Order = form.Order,
                    Title = form.Title,
                });
            }

            return model;
        }

      

        public class Form
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public int Order { get; set; }
        }
    }
}