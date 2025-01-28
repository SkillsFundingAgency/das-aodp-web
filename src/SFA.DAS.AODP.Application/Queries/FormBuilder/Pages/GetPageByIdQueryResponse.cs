﻿namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;

public class GetPageByIdQueryResponse
{

    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string Title { get; set; }
    public Guid Key { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public List<Question> Questions { get; set; }

    public class Question
    {
        public Guid Id { get; set; }
        public Guid Key { get; set; }
        public int Order { get; set; }
        public string Title { get; set; }


    }
}
