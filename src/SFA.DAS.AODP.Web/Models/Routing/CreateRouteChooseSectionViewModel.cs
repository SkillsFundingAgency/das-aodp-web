using SFA.DAS.AODP.Application.Queries.FormBuilder.Sections;

namespace SFA.DAS.AODP.Web.Models.Routing
{
    public class CreateRouteChooseSectionViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid ChosenSectionKey { get; set; }
        public List<SectionInformation> Sections { get; set; }

        public class SectionInformation
        {
            public Guid Key { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public static CreateRouteChooseSectionViewModel MapToViewModel(GetAllSectionsQueryResponse response, Guid formVersionId)
        {
            CreateRouteChooseSectionViewModel model = new()
            {
                FormVersionId = formVersionId,
                Sections = new()
            };

            foreach (var section in response.Data)
            {
                model.Sections.Add(new()
                {
                    Key = section.Key,
                    Title = section.Title,
                    Order = section.Order,
                });
            }
            return model;
        }

    }
}
