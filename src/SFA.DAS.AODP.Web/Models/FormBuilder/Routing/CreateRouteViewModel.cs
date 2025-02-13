using SFA.DAS.AODP.Application.Commands.FormBuilder.Routes;
using SFA.DAS.AODP.Application.Queries.FormBuilder.Routes;

namespace SFA.DAS.AODP.Web.Models.FormBuilder.Routing
{
    public class CreateRouteViewModel
    {
        public const string DefaultNextId = "Default";
        public const string EndId = "End";

        public Guid FormVersionId { get; set; }
        public Guid SectionId { get; set; }
        public Guid PageId { get; set; }
        public Guid QuestionId { get; set; }

        public string QuestionTitle { get; set; }
        public string SectionTitle { get; set; }
        public string PageTitle { get; set; }
        public bool Editable { get; set; }


        public List<NextPageOption> NextPageOptions { get; set; } = new();
        public List<NextSectionOption> NextSectionOptions { get; set; } = new();

        public List<OptionInformation> Options { get; set; } = new();

        public List<SelectedOption> SelectedOptions { get; set; } = new();

        public class SelectedOption
        {
            public Guid OptionId { get; set; }
            public string SelectedPageId { get; set; }
            public string SelectedSectionId { get; set; }
        }

        public class OptionInformation
        {
            public Guid Id { get; set; }
            public string Value { get; set; }
            public int Order { get; set; }
        }

        public class NextPageOption
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public class NextSectionOption
        {
            public string Id { get; set; }
            public string Title { get; set; }
            public int Order { get; set; }
        }

        public static CreateRouteViewModel MapToViewModel(GetRoutingInformationForQuestionQueryResponse value, Guid formVersionId, Guid sectionId, Guid pageId)
        {
            CreateRouteViewModel model = new()
            {
                FormVersionId = formVersionId,
                PageId = pageId,
                SectionId = sectionId,
                QuestionId = value.QuestionId,
                QuestionTitle = value.QuestionTitle,
                PageTitle = value.PageTitle,
                SectionTitle = value.SectionTitle,
                Editable = value.Editable,
            };

            foreach (var option in value.RadioOptions?.OrderBy(o => o.Order).ToList() ?? [])
            {
                model.Options.Add(new()
                {
                    Id = option.Id,
                    Order = option.Order,
                    Value = option.Value
                });
            }

            model.NextPageOptions.Add(new()
            {
                Id = DefaultNextId,
                Order = 0,
                Title = "Default next page"
            });

            foreach (var page in value.NextPages?.OrderBy(o => o.Order).ToList() ?? [])
            {
                model.NextPageOptions.Add(new()
                {
                    Id = page.Id.ToString(),
                    Order = page.Order,
                    Title = $"{page.Order}. {page.Title}"
                });
            }

            model.NextPageOptions.Add(new()
            {
                Id = EndId,
                Order = model.NextPageOptions.Max(m => m.Order) + 1,
                Title = "End of section"
            });

            model.NextSectionOptions.Add(new()
            {
                Id = DefaultNextId,
                Order = 0,
                Title = "Default next section"
            });

            foreach (var section in value.NextSections?.OrderBy(o => o.Order).ToList() ?? [])
            {
                model.NextSectionOptions.Add(new()
                {
                    Id = section.Id.ToString(),
                    Order = section.Order,
                    Title = $"{section.Order}. {section.Title}"
                });
            }

            model.NextSectionOptions.Add(new()
            {
                Id = EndId,
                Order = model.NextSectionOptions.Max(m => m.Order) + 1,
                Title = "End of Form"
            });

            foreach (var route in value.Routes)
            {
                var option = new SelectedOption()
                {
                    OptionId = route.OptionId,
                };

                if (route.EndSection) option.SelectedPageId = EndId;
                else if (route.NextPageId != default) option.SelectedPageId = route.NextPageId.ToString();
                else option.SelectedPageId = DefaultNextId;

                if (route.EndForm) option.SelectedSectionId = EndId;
                else if (route.NextSectionId != default) option.SelectedSectionId = route.NextSectionId.ToString();
                else option.SelectedSectionId = DefaultNextId;

                model.SelectedOptions.Add(option);

            }

            return model;
        }

        public static ConfigureRoutingForQuestionCommand MapToCommand(CreateRouteViewModel model)
        {
            ConfigureRoutingForQuestionCommand command = new()
            {
                PageId = model.PageId,
                QuestionId = model.QuestionId,
                FormVersionId = model.FormVersionId,
                SectionId = model.SectionId,
                Routes = new()
            };

            foreach (var route in model.SelectedOptions)
            {
                var nextPage = route.SelectedPageId;
                var nextSection = route.SelectedSectionId;

                var commandRoute = new ConfigureRoutingForQuestionCommand.Route()
                {
                    OptionId = route.OptionId
                };

                if (nextPage == EndId)
                {
                    commandRoute.EndSection = true;
                }
                else if (nextPage != DefaultNextId)
                {
                    commandRoute.NextPageId = Guid.Parse(nextPage);
                }

                if (nextSection == EndId)
                {
                    commandRoute.EndForm = true;
                }
                else if (nextSection != DefaultNextId)
                {
                    commandRoute.NextSectionId = Guid.Parse(nextSection);
                }
                command.Routes.Add(commandRoute);
            }

            return command;
        }
    }
}
