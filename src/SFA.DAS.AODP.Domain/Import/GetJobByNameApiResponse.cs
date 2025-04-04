﻿namespace SFA.DAS.AODP.Domain.Import
{
    public class GetJobByNameApiResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public bool Enabled { get; set; }

        public string Status { get; set; } = null!;

        public DateTime? LastRunTime { get; set; }
    }   
}
