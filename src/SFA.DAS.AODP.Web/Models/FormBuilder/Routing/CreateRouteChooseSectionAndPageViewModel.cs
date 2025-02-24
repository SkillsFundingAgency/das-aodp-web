using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Routing
{
    public class CreateRouteChooseSectionAndPageViewModel
    {
        [Required]
        public Guid FormVersionId { get; set; }

        [DisplayName("Section")]
        [Required]
        public Guid? ChosenSectionId { get; set; }

        [Required]
        [DisplayName("Page")]
        public Guid? ChosenPageId { get; set; }

        public List<SectionInformation> Sections { get; set; } = new();
        public bool AreSectionsEmpty => !Sections.Any();

        public class SectionInformation
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
            public List<PageInformation> Pages { get; set; }


        }

        public class PageInformation
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public static CreateRouteChooseSectionAndPageViewModel MapToViewModel(GetAvailableSectionsAndPagesForRoutingQueryResponse response, Guid formVersionId)
        {
            CreateRouteChooseSectionAndPageViewModel model = new()
            {
                FormVersionId = formVersionId,
                Sections = new()
            };

            foreach (var section in response.Sections)
            {
                var modelSection = new SectionInformation()
                {
                    Id = section.Id,
                    Title = section.Title,
                    Order = section.Order,
                    Pages = new()
                };

                foreach (var page in section.Pages ?? [])
                {
                    modelSection.Pages.Add(new()
                    {
                        Id = page.Id,
                        Title = page.Title,
                        Order = page.Order,
                    });
                }
                model.Sections.Add(modelSection);
            }
            return model;
        }

    }
}
