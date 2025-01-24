using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.AODP.Models.Forms.Application;

public class Application
{
    public int Id { get; set; }
    public int FormSchemaId { get; set; }
    public List<AnsweredQuestion> AnsweredQuestions { get; set; } = new List<AnsweredQuestion>();
}
