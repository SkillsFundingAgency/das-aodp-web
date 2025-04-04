﻿using SFA.DAS.AODP.Models.Application;

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
        public DateTime? UpdatedDate { get; set; }
        public ApplicationStatus Status { get; set; }
        public bool NewMessage { get; set; }
    }
}
