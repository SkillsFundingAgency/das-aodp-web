﻿using SFA.DAS.AODP.Domain.Interfaces;

public class GetApplicationSectionByIdApiRequest : IGetApiRequest
{
    public Guid FormVersionId { get; set; }
    public Guid SectionId { get; set; }

    public string GetUrl => $"/api/applications/forms/{FormVersionId}/sections/{SectionId}";
}
