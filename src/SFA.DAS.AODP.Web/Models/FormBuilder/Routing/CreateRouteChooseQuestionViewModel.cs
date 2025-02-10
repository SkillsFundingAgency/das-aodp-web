using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Routing
{
    public class CreateRouteChooseQuestionViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }

        [Required]
        [DisplayName("Question")]
        public Guid? ChosenQuestionId { get; set; }

        public List<QuestionInformation> Questions { get; set; } = new();

        public class QuestionInformation
        {
            public Guid Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }


        public static CreateRouteChooseQuestionViewModel MapToViewModel(GetAvailableQuestionsForRoutingQueryResponse value, Guid formVersionId, Guid sectionId, Guid pageId)
        {
            CreateRouteChooseQuestionViewModel model = new()
            {
                FormVersionId = formVersionId,
                PageId = pageId,
                SectionId = sectionId,
                Questions = new()
            };

            foreach (var question in value.Questions ?? [])
            {
                model.Questions.Add(new()
                {
                    Id = question.Id,
                    Title = question.Title,
                    Order = question.Order,
                });
            }
            return model;
        }
    }
}
