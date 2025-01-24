using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Models.Forms.DTO;

public class Option
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }
}

public class Page
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("questions")]
    public List<Question> Questions { get; set; }
}

public class Question
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("description")]
    public string Description { get; set; }

    [JsonProperty("hint")]
    public string Hint { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }

    [JsonProperty("options")]
    public List<Option> Options { get; set; }
}

public class Form
{
    [JsonProperty("sections")]
    public List<Section> Sections { get; set; }
}

public class Section
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("pages")]
    public List<Page> Pages { get; set; }
}
