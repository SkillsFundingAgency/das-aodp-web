//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Microsoft.AspNetCore.Mvc.TagHelpers;
//using Microsoft.AspNetCore.Mvc.ViewFeatures;
//using Microsoft.AspNetCore.Razor.TagHelpers;

//namespace SFA.DAS.AODP.Web.TagHelpers
//{
//    [HtmlTargetElement("a", Attributes = ValidationForAttributeName)]
//    public class ValidationLinkTagHelper : TagHelper
//    {
//        public const string ValidationForAttributeName = "sfa-validation-for";

//        [HtmlAttributeName(ValidationForAttributeName)]
//        public ModelExpression For { get; set; }

//        [HtmlAttributeNotBound]
//        [ViewContext]
//        public ViewContext ViewContext { get; set; }

//        public override void Process(TagHelperContext context, TagHelperOutput output)
//        {
//            ModelStateEntry entry;
//            ViewContext.ViewData.ModelState.TryGetValue(For.Name, out entry);
//            if (entry == null || !entry.Errors.Any()) return;

//            var tagBuilder = new TagBuilder("a");

//            tagBuilder.Attributes.Add("href", $"#{For.Name}");
//            output.MergeAttributes(tagBuilder);

//            output.Content.SetContent(entry.Errors[0].ErrorMessage);
//        }
//    }

//    [HtmlTargetElement("div", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
//    [HtmlTargetElement("input", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
//    [HtmlTargetElement("textarea", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
//    [HtmlTargetElement("dt", Attributes = ValidationForAttributeName + "," + ValidationErrorClassName)]
//    public class ValidationClassTagHelper : TagHelper
//    {
//        public const string ValidationErrorClassName = "sfa-validationerror-class";

//        public const string ValidationForAttributeName = "sfa-validation-for";

//        [HtmlAttributeName(ValidationForAttributeName)]
//        public ModelExpression For { get; set; }

//        [HtmlAttributeName(ValidationErrorClassName)]
//        public string ValidationErrorClass { get; set; }

//        [HtmlAttributeNotBound]
//        [ViewContext]
//        public ViewContext ViewContext { get; set; }

//        public override void Process(TagHelperContext context, TagHelperOutput output)
//        {
//            ModelStateEntry entry;
//            ViewContext.ViewData.ModelState.TryGetValue(For.Name, out entry);
//            if (entry == null || !entry.Errors.Any()) return;

//            var tagBuilder = new TagBuilder(context.TagName);
//            tagBuilder.AddCssClass(ValidationErrorClass);
//            output.MergeAttributes(tagBuilder);
//        }
//    }

//    [HtmlTargetElement("div", Attributes = ValidationErrorClassName)]
//    [HtmlTargetElement("input", Attributes = ValidationErrorClassName)]
//    [HtmlTargetElement("fieldset", Attributes = ValidationErrorClassName)]
//    public class ValidationClassListedErrorsTagHelper : TagHelper
//    {
//        public const string ValidationErrorClassName = "sfa-validationerror-class";

//        public const string ValidationListAttributeName = "sfa-validation-for-list";

//        /// <summary>
//        /// This is a CSV list, you can also use it to check for a single entry within the ModelState.
//        /// It is not bound to a ModelExpression so will take any value/free-text.
//        /// </summary>
//        [HtmlAttributeName(ValidationListAttributeName)]
//        public string CsvList { get; set; }

//        [HtmlAttributeName(ValidationErrorClassName)]
//        public string ValidationErrorClass { get; set; }

//        [HtmlAttributeNotBound]
//        [ViewContext]
//        public ViewContext ViewContext { get; set; }

//        public override void Process(TagHelperContext context, TagHelperOutput output)
//        {
//            if (!string.IsNullOrWhiteSpace(CsvList))
//            {
//                foreach (var key in CsvList.Split(","))
//                {
//                    ModelStateEntry entry;
//                    ViewContext.ViewData.ModelState.TryGetValue(key, out entry);
//                    if (entry == null || !entry.Errors.Any()) continue;

//                    var tagBuilder = new TagBuilder(context.TagName);
//                    tagBuilder.AddCssClass(ValidationErrorClass);
//                    output.MergeAttributes(tagBuilder);
//                    break;
//                }
//            }
//        }
//    }

//    [HtmlTargetElement("div", Attributes = ValidationErrorClassName)]
//    [HtmlTargetElement("input", Attributes = ValidationErrorClassName)]
//    [HtmlTargetElement("fieldset", Attributes = ValidationErrorClassName)]
//    public class ValidationClassAnyErrorsTagHelper : TagHelper
//    {
//        public const string ValidationErrorClassName = "sfa-anyvalidationerror-class";

//        [HtmlAttributeName(ValidationErrorClassName)]
//        public string ValidationErrorClass { get; set; }

//        [HtmlAttributeNotBound]
//        [ViewContext]
//        public ViewContext ViewContext { get; set; }

//        public override void Process(TagHelperContext context, TagHelperOutput output)
//        {
//            if (ViewContext.ViewData.ModelState.IsValid) return;

//            var tagBuilder = new TagBuilder(context.TagName);
//            tagBuilder.AddCssClass(ValidationErrorClass);
//            output.MergeAttributes(tagBuilder);
//        }
//    }
//}
