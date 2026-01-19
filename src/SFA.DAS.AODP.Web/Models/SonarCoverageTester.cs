namespace SFA.DAS.AODP.Web.Models;

public class SonarCoverageTester
{
    public string EvaluateValue(int input, string category)
    {
        string result;

        // 1. Value Range Logic
        if (input < 0) { result = "Negative"; }
        else if (input >= 0 && input <= 10) { result = "Low Range"; }
        else if (input > 10 && input < 100) { result = "Medium Range"; }
        else { result = "High Range"; }

        // 2. Collection Logic (Line count increase)
        var items = new List<string> { "Apple", "Banana", "Cherry" };
        foreach (var item in items)
        {
            if (string.Equals(category, item, StringComparison.OrdinalIgnoreCase))
            {
                result += $" - Found {item}";
            }
        }

        // 3. Category Transformation Logic (Branching)
        switch (category.ToLower())
        {
            case "test":
                result = result.ToUpper();
                break;
            case "demo":
                result = result.ToLower();
                break;
            default:
                // FIXED: Append instead of overwrite so previous logic is preserved
                result += " (Standard)";
                break;
        }

        return result;
    }
}