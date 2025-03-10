using System;

namespace SFA.DAS.AODP.Application.Queries.Qualifications
{
    public class GetActionTypesResponse
    {

        public List<ActionTypeViewModel>? ActionTypes { get; set; } = new();

    
    }
    public class ActionTypeViewModel
    {

        public Guid Id { get; set; }
        public string? Description { get; set; }

    }
}

