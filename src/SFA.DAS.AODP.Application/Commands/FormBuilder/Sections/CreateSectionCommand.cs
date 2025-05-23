﻿using MediatR;

namespace SFA.DAS.AODP.Application.Commands.FormBuilder.Sections;

public class CreateSectionCommand : IRequest<BaseMediatrResponse<CreateSectionCommandResponse>>
{

    public Guid FormVersionId { get; set; }
    public string Title { get; set; }

}