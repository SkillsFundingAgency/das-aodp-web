﻿using SFA.DAS.AODP.Models.Application;

public class GetApplicationFormStatusByApplicationIdQueryResponse
{
    public string ApplicationName { get; set; }
    public string Reference { get; set; }
    public string? QualificationNumber { get; set; }


    public bool ReadyForSubmit { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public string Owner { get; set; }
    public bool Submitted { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public ApplicationStatus Status { get; set; }
    public bool NewMessage { get; set; }
    public bool ReviewExists { get; set; }


    public List<Section> Sections { get; set; } = new();

    public class Section
    {
        public Guid SectionId { get; set; }
        public int PagesRemaining { get; set; }
        public int SkippedPages { get; set; }
        public int TotalPages { get; set; }
    }

}