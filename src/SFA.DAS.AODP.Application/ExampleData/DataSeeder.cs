using SFA.DAS.AODP.Application.MemoryCache;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.ExampleData;

public static class DataSeeder
{
    public static void Seed(ICacheManager cacheManager)
    {
        Guid NewGuid() => Guid.NewGuid();

        try
        {
            Console.WriteLine("Starting seeding process...");

            var forms = new List<Form>();
            var sections = new List<Section>();
            var pages = new List<Page>();
            var questions = new List<Question>();
            var validationRules = new List<ValidationRule>();
            var options = new List<Option>();

            for (int i = 1; i <= 20; i++)
            {
                var formId = NewGuid();
                var formOptions = CreateOptions(4);
                var formValidationRules = CreateValidationRules();
                var formSections = CreateSections(formId, formOptions, formValidationRules);

                foreach (var section in formSections)
                {
                    sections.Add(section);
                    foreach (var page in section.Pages)
                    {
                        pages.Add(page);
                        foreach (var question in page.Questions)
                        {
                            questions.Add(question);
                            validationRules.AddRange(question.ValidationRules);
                            options.AddRange(question.Options);
                        }
                    }
                }

                var form = new Form
                {
                    Id = formId,
                    Name = $"Sample Form {i}",
                    Version = $"{i}.0",
                    Published = i % 2 == 0,
                    Key = $"SampleKey{i}",
                    ApplicationTrackingTemplate = $"Template{i}",
                    Order = i,
                    Description = $"Form {i} description.",
                    Sections = formSections
                };

                forms.Add(form);
                Console.WriteLine($"Created Form {i} with ID: {formId}");
            }

            Console.WriteLine("Seeding data into cache...");

            cacheManager.Set("Forms", forms);
            cacheManager.Set("Sections", sections.Distinct().ToList());
            cacheManager.Set("Pages", pages.Distinct().ToList());
            cacheManager.Set("Questions", questions.Distinct().ToList());
            cacheManager.Set("ValidationRules", validationRules.Distinct().ToList());
            cacheManager.Set("Options", options.Distinct().ToList());

            Console.WriteLine("Seeding process completed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Seeding error: {ex.Message}");
        }
    }

    private static List<Option> CreateOptions(int count)
    {
        var options = new List<Option>();
        for (int i = 1; i <= count; i++)
        {
            options.Add(new Option
            {
                Id = Guid.NewGuid(),
                Text = $"Option {i}",
                Value = i.ToString()
            });
        }
        return options;
    }

    private static List<ValidationRule> CreateValidationRules()
    {
        return new List<ValidationRule>
        {
            new ValidationRule
            {
                Id = Guid.NewGuid(),
                Type = "Required",
                Value = "true",
                ErrorMessage = "This field is required."
            }
        };
    }

    private static List<Section> CreateSections(Guid formId, List<Option> options, List<ValidationRule> validationRules)
    {
        var sections = new List<Section>();
        for (int i = 1; i <= 3; i++)
        {
            var sectionId = Guid.NewGuid();
            var pages = CreatePages(sectionId, options, validationRules);

            sections.Add(new Section
            {
                Id = sectionId,
                FormId = formId,
                Title = $"Sample Section {i}",
                Description = $"Section {i} description.",
                Order = i,
                Pages = pages
            });
        }
        return sections;
    }

    private static List<Page> CreatePages(Guid sectionId, List<Option> options, List<ValidationRule> validationRules)
    {
        var pages = new List<Page>();
        for (int i = 1; i <= 3; i++)
        {
            var pageId = Guid.NewGuid();
            var questions = CreateQuestions(pageId, options, validationRules);

            pages.Add(new Page
            {
                Id = pageId,
                SectionId = sectionId,
                Title = $"Sample Page {i}",
                Description = $"Page {i} description.",
                Order = i,
                Questions = questions
            });
        }
        return pages;
    }

    private static List<Question> CreateQuestions(Guid pageId, List<Option> options, List<ValidationRule> validationRules)
    {
        var questions = new List<Question>();
        for (int i = 1; i <= 3; i++)
        {
            questions.Add(new Question
            {
                Id = Guid.NewGuid(),
                PageId = pageId,
                Title = $"Sample Question {i}",
                Type = "Text",
                Required = true,
                Order = i,
                Description = $"Describe something for question {i}.",
                Hint = $"Hint text for question {i}",
                Options = options,
                ValidationRules = validationRules
            });
        }
        return questions;
    }
}