namespace SFA.DAS.AODP.Web.Models.Routing
{
    public class SectionInformation
    {
        public Guid Key { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
    }

    public class PageInformation
    {
        public Guid Key { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
    }

    public class QuestionInformation
    {
        public Guid Key { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
    }

    public class RouteInformation
    {
        public Guid OptionKey { get; set; }
        public string NextPageKey { get; set; }
        public string NextSectionKey { get; set; }
    }

    public class CreateRouteChoosePageViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionKey { get; set; }
        public Guid ChosenPageKey { get; set; }
        public List<PageInformation> Pages { get; set; }


    }

    public class CreateRouteChooseQuestionViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionKey { get; set; }
        public Guid PageKey { get; set; }
        public Guid ChosenQuestionKey { get; set; }

        public List<QuestionInformation> Questions { get; set; }
        public List<SectionInformation> Sections { get; set; }

    }

    public class CreateRouteViewModel
    {
        public Guid FormVersionId { get; set; }
        public Guid SectionKey { get; set; }
        public Guid PageKey { get; set; }
        public Guid QuestionKey { get; set; }
        public List<PageInformation> Pages { get; set; }

        public List<RouteInformation> Routes { get; set; }

        public List<OptionInformation> Options { get; set; }

        public class OptionInformation
        {
            public Guid OptionKey { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }

        }
    }

    public class ListRoutesViewModel
    {
        public Guid FormVersionId { get; set; }
        public Dictionary<QuestionInformation, List<RouteInformation>> QuestionRoutes { get; set; } = new();

    }
}
