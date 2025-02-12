
namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Questions;

public class GetQuestionByIdQueryResponse
{
    public Guid Id { get; set; }
    public Guid PageId { get; set; }
    public string Title { get; set; }
    public Guid Key { get; set; }
    public string Hint { get; set; }
    public int Order { get; set; }
    public bool Required { get; set; }
    public string Type { get; set; }
    public List<RouteInformation> Routes { get; set; } = new();

    public TextInputOptions TextInput { get; set; } = new();
    public List<RadioOptionItem> RadioOptions { get; set; } = new();
    public bool Editable { get; set; }

    public class TextInputOptions
    {
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
    }

    public class RadioOptionItem
    {
        public Guid Id { get; set; }
        public string Value { get; set; }
        public int Order { get; set; }
    }

    public class RouteInformation
    {
        public Page? NextPage { get; set; }
        public Section? NextSection { get; set; }
        public bool EndForm { get; set; }
        public bool EndSection { get; set; }
        public RadioOptionItem Option { get; set; }
    }

    public class Section
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        public List<Page> Pages { get; set; } = new();
    }

    public class Page
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int Order { get; set; }
        //public Question Quesiton { get; set; } = new();
    }
}