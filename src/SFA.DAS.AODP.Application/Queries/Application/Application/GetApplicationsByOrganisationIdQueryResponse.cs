﻿using SFA.DAS.AODP.Application;

public class GetApplicationsByOrganisationIdQueryResponse
{
    public List<Application> Applications { get; set; } = new();

    public class Application
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? SubmittedDate { get; set; }
        public bool Submitted { get; set; }
        public string Owner { get; set; }
        public string Reference { get; set; }
        public Guid FormVersionId { get; set; }

    }
}
