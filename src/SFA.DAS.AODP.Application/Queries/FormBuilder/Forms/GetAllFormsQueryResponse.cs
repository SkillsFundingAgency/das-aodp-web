using SFA.DAS.AODP.Application.MediatR.Base;
using SFA.DAS.AODP.Models.Forms.FormBuilder;

namespace SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;

public class GetAllFormsQueryResponse : BaseResponse
{
    public List<Form> Data { get; set; }
}