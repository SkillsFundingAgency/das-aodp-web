using System.ComponentModel;

namespace SFA.DAS.AODP.Models.Forms;

public enum QuestionType
{
    Text,        // Not null, length min, length max,
    TextArea,        // Not null, length min, length max
    Integer,     // Not null, greater than, less than, equal/not equal to 
    Decimal,     // Not null, greater than, less than, equal/not equal to 
    Date,        // Not null, greater than, less than, equal/not equal to 
    MultiChoice, // Not null
    Radio,
    Boolean      // Not null
}
