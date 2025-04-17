using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Routing
{
    public class CreateRouteChooseSectionViewModel
    {
        [Required]
        public Guid FormVersionId { get; set; }

        [DisplayName("Section")]
        [Required]
        public Guid? ChosenSectionId { get; set; }

        public List<SectionInformation> Sections { get; set; } = new();
        public bool AreSectionsEmpty => !Sections.Any();

        public class SectionInformation
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public static CreateRouteChooseSectionViewModel MapToViewModel(GetAvailableSectionsAndPagesForRoutingQueryResponse response, Guid formVersionId)
        {
            CreateRouteChooseSectionViewModel model = new()
            {
                FormVersionId = formVersionId,
                Sections = new()
            };

            foreach (var section in response.Sections)
            {
                model.Sections.Add(new()
                {
                    Id = section.Id,
                    Title = section.Title,
                    Order = section.Order,
                });
            }
            return model;
        }

    }
}
