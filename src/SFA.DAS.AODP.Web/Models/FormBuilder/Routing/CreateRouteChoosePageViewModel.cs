using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Routing
{
    public class CreateRouteChoosePageViewModel
    {
        [Required]
        public Guid FormVersionId { get; set; }

        [Required]
        public Guid ChosenSectionId { get; set; }

        [Required]
        [DisplayName("Page")]
        public Guid? ChosenPageId { get; set; }

        public List<PageInformation> Pages { get; set; } = new();

        public class PageInformation
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public static CreateRouteChoosePageViewModel MapToViewModel(GetAvailableSectionsAndPagesForRoutingQueryResponse response, Guid formVersionId, Guid sectionId)
        {
            CreateRouteChoosePageViewModel model = new()
            {
                FormVersionId = formVersionId,
                ChosenSectionId = sectionId,
            };

            var section = response.Sections.Where(r => r.Id == sectionId).FirstOrDefault();

            foreach (var page in section?.Pages ?? [])
            {
                model.Pages.Add(new()
                {
                    Id = page.Id,
                    Title = page.Title,
                    Order = page.Order,
                });
            }

            return model;
        }

    }
}
