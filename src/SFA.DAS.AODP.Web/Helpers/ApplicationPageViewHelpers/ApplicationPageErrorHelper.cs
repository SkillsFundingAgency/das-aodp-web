using SFA.DAS.AODP.Web.Models.Application;

namespace SFA.DAS.AODP.Web.Helpers.ApplicationPageViewHelpers
{
    public static class ApplicationPageErrorHelper
    {
        public static ApplicationPageViewModel.Question? GetQuestionByErrorKey(List<ApplicationPageViewModel.Question> questions, string errorKey)
        {
            var question = questions.FirstOrDefault(q => q.Id.ToString() == errorKey);
            if (question == null)
            {
                try
                {
                    var order = ExtractOrderFromErrorKey(errorKey);
                    if (order != null) question = questions.First(q => q.Order - 1 == order);
                }
                catch { }
            }
            return question;
        }

        public static int? ExtractOrderFromErrorKey(string errorKey)
        {
            try
            {
                int startIndex = errorKey.IndexOf("[") + 1;
                int len = errorKey.LastIndexOf("]") - startIndex;
                string orderStr = errorKey.Substring(startIndex, len);
                if (int.TryParse(orderStr, out int order))
                    return order;
            }
            catch { }

            return null;
        }

    }
}
