﻿using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Pages;

public class MovePageDownApiRequest : IPutApiRequest
{

    public Guid PageId { get; set; }
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }

    public string PutUrl => $"api/forms/{FormVersionId}/sections/{SectionId}/Pages/{PageId}/MoveDown";
    public object Data { get; set; }
}