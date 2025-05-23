﻿using System.ComponentModel.DataAnnotations;

namespace SFA.DAS.AODP.Web.Areas.Review.Models.ApplicationsReview
{
    public class SkillsEnglandReviewViewModel
    {
        public Guid ApplicationReviewId { get; set; }
        [Required]
        public string? Comments { get; set; }
        [Required]
        public bool? Approved { get; set; }
        public bool NewDecision { get; set; }
        public MessageActions AdditionalActions { get; set; } = new();

        public class MessageActions
        {
            public bool Preview { get; set; }
            public bool Send { get; set; }
            public bool Edit { get; set; }
        }
    }

}
