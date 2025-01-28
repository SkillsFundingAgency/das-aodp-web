﻿namespace SFA.DAS.AODP.Models.Forms.FormBuilder;

public class Page
{
    public Guid Id { get; set; }
    public Guid Key { get; set; }
    public Guid SectionId { get; set; }
    public Guid SectionKey { get; set; } = Guid.NewGuid();
    public Guid FormVersionId { get; set; } = Guid.NewGuid();
    public string Title { get; set; }
    public string Description { get; set; }
    public int Order { get; set; }
    public int? NextPageId { get; set; }
    public List<Question> Questions { get; set; }

    public Page()
    {
        Questions = new List<Question>();
    }
}
