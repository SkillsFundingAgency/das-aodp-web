﻿using SFA.DAS.AODP.Application.Queries.Import;
using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Models.Import
{
    public class CompleteViewModel
    {
        [Required]
        public string ImportType { get; set; } = string.Empty;       

        public DateTime SubmittedTime { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string JobName { get; internal set; }
    }
}
