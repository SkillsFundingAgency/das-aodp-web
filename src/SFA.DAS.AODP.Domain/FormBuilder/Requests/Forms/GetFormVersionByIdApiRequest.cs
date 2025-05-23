﻿using SFA.DAS.AODP.Domain.Interfaces;

namespace SFA.DAS.AODP.Domain.FormBuilder.Requests.Forms;

public class GetFormVersionByIdApiRequest : IGetApiRequest
{
    private readonly Guid _formVersionId;
    public GetFormVersionByIdApiRequest(Guid formVersionId)
    {
        _formVersionId = formVersionId;
    }

    public string GetUrl => $"api/forms/{_formVersionId}";
}