namespace SFA.DAS.AODP.Web.Models.Routing
{
    public class CreateRouteChooseSectionViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid ChosenSectionId { get; set; }
        public List<SectionInformation> Sections { get; set; }

        public class SectionInformation
        {
            public Guid Key { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }
    }

    public class CreateRouteChoosePageViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionKey { get; set; }
        public Guid ChosenPageId { get; set; }
        public List<PageInformation> Pages { get; set; }

        public class PageInformation
        {
            public Guid Key { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }
    }

    public class CreateRouteChooseQuestionViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionKey { get; set; }
        public Guid PageKey { get; set; }
        public Guid ChosenQuestionId { get; set; }
        public List<QuestionInformation> Questions { get; set; }

        public class QuestionInformation
        {
            public Guid Key { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }
    }

    public class CreateRouteViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid ChosenQuestionId { get; set; }

        public List<SectionInformation> Sections { get; set; }

        public class SectionInformation
        {
            public Guid SectionId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }

            public List<PageInformation> Pages { get; set; }
        }
        public class PageInformation
        {
            public Guid PageId { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }

            public List<QuestionInformation> Questions { get; set; }



        }

        public class QuestionInformation
        {
            public Guid QuestionId { get; set; }
            public string Title { get; set; }
        }



    }
}
