namespace SFA.DAS.AODP.Web.Models;

public class SonarCoverageTester
{
    public string EvaluateValue(int input, string category)
    {
        string result = "Default";

        // Branching logic to force SonarQube to look for multiple paths
        if (input < 0)
        {
            result = "Negative";
        }
        else if (input >= 0 && input <= 10)
        {
            result = "Low Range";
        }
        else if (input > 10 && input < 100)
        {
            result = "Medium Range";
        }
        else
        {
            result = "High Range";
        }

        // Additional logic to increase line count
        var items = new List<string> { "Apple", "Banana", "Cherry" };
        foreach (var item in items)
        {
            if (category == item)
            {
                result += $" - Found {item}";
            }
        }

        switch (category.ToLower())
        {
            case "test":
                result = result.ToUpper();
                break;
            case "demo":
                result = result.ToLower();
                break;
            default:
                result = "Unknown Category";
                break;
        }

        return result;
    }
}