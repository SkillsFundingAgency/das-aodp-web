using SFA.DAS.AODP.Domain.FormBuilder.Responses.Questions;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;

public class GetQuestionByIdQueryResponse() : BaseResponse
{
    public Question Data { get; set; }

    public class Question
    {
        public Guid Id { get; set; }
        public Guid PageId { get; set; }
        public string Title { get; set; }
        public Guid Key { get; set; }
        public string Hint { get; set; }
        public int Order { get; set; }
        public bool Required { get; set; }
        public string Type { get; set; }

        public static implicit operator Question(GetQuestionByIdApiResponse.Question entity)
        {
            return new()
            {
                Id = entity.Id,
                PageId = entity.PageId,
                Title = entity.Title,
                Key = entity.Key,
                Hint = entity.Hint,
                Order = entity.Order,
                Required = entity.Required,
                Type = entity.Type

            };
        }
    }


}