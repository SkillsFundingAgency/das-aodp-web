////using Microsoft.AspNetCore.Mvc.ModelBinding;
////using SFA.DAS.AODP.Models.Exceptions.FormValidation;
////using SFA.DAS.AODP.Web.Models.Application;

////namespace SFA.DAS.AODP.Web.Validators
////{
////    public class ApplicationAnswersValidatorTests : IApplicationAnswersValidator
////    {
////        private readonly IEnumerable<IAnswerValidator> _validators;

////        public ApplicationAnswersValidatorTests(IEnumerable<IAnswerValidator> validators)
////        {
////            _validators = validators;
////        }

////        public void ValidateApplicationPageAnswers(ModelStateDictionary modelState, GetApplicationPageByIdQueryResponse page, ApplicationPageViewModel viewModel)
////        {
////            foreach (var question in page.Questions)
////            {
////                var questionAnswer = viewModel.Questions.First(q => q.Id == question.Id);
////                try
////                {
////                    if (questionAnswer.Type.ToString() != question.Type.ToString())
////                        throw new InvalidOperationException("Stored question type does not match the received question type");

////                    var validator = _validators.FirstOrDefault(v => v.QuestionTypes.Any(c => c.ToString() == question.Type))
////                        ?? throw new NotImplementedException($"Unable to validate the answer for question type {question.Type}");

////                    validator.Validate(question, questionAnswer.Answer, viewModel);
////                }
////                catch (QuestionValidationFailedException ex)
////                {
////                    modelState.AddModelError(question.Id.ToString(), ex.Message);
////                }


////            }
////        }
////    }
////}

//using AutoFixture;
//using MediatR;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using Moq;
//using SFA.DAS.AODP.Application;
//using SFA.DAS.AODP.Application.Queries.FormBuilder.Forms;
//using Microsoft.AspNetCore.Mvc.ModelBinding;
//using SFA.DAS.AODP.Web.Models.Application;
//using SFA.DAS.AODP.Models.Exceptions.FormValidation;
//using SFA.DAS.AODP.Application.Queries.FormBuilder.Pages;
//using SFA.DAS.AODP.Web.Validators;


//namespace SFA.DAS.AODP.Web.Test.Validators
//{
//    public class ApplicationAnswersValidatorTests
//    {
//        //private Mock<IMediator> _mediatorMock = new();
//        private IEnumerable<IAnswerValidator> _validators;
//        private readonly Fixture _fixture = new();
//        //private readonly Mock<ILogger<IEnumerable<Web.Validators.IAnswerValidator>>> _loggerMock = new();
//        //private ModelStateDictionary _modelState;
//        //private ApplicationPageViewModel _viewModel;


//        //public ApplicationAnswersValidatorTests()
//        //{
//        //    //_validator = new();
//        //}
//        public ApplicationAnswersValidatorTests(IEnumerable<IAnswerValidator> validators)
//        {
//            _validators = validators;
//        }

//        [Fact]
//        public async Task Index_Get_ValidRequest_ReturnsOk()
//        {
//            // Arrange
//            var page = _fixture.Create<GetApplicationPageByIdQueryResponse>();
//            var viewModel = _fixture.Create<ApplicationPageViewModel>();
//            var question = page.Questions.First();
//            var questionAnswer = viewModel.Questions.First(q => q.Id == question.Id);
//            var validator = _validators.FirstOrDefault(v => v.QuestionTypes.Any(c => c.ToString() == question.Type));

//            // Act
//            validator.Validate(question, questionAnswer.Answer, viewModel);


//            // Assert


//            //foreach (var question in page.Questions)
//            //{
//            //    var questionAnswer = viewModel.Questions.First(q => q.Id == question.Id);
//            //    try
//            //    {
//            //        if (questionAnswer.Type.ToString() != question.Type.ToString())
//            //            throw new InvalidOperationException("Stored question type does not match the received question type");

//            //        var validator = _validators.FirstOrDefault(v => v.QuestionTypes.Any(c => c.ToString() == question.Type))
//            //            ?? throw new NotImplementedException($"Unable to validate the answer for question type {question.Type}");

//            //        validator.Validate(question, questionAnswer.Answer, viewModel);
//            //    }
//            //var query = new GetApplicationPageByIdQueryResponse();
//            //var request = new GetAllFormVersionsQuery();
//            //var expectedResponse = _fixture
//            //    .Build<BaseMediatrResponse<GetAllFormVersionsQueryResponse>>()
//            //    .With(w => w.Success, true)
//            //    .Create();

//                //_mediatorMock.Setup(x => x.Send(It.IsAny<GetAllFormVersionsQuery>(), It.IsAny<CancellationToken>())).ReturnsAsync(expectedResponse);

//                ////Act
//                //var result = await _controller.Index();

//                ////Assert
//                //Assert.IsType<ViewResult>(result);
//                //var okResult = (ViewResult)result;

//                //var returnedData = okResult.Model as FormVersionListViewModel;

//                //Assert.NotNull(returnedData);
//                //Assert.Equal(returnedData.FormVersions.Count, expectedResponse.Value.Data.Count);
//        }
//        public async Task Index_Get_InvalidRequest_Returns_InvalidOperationException()
//        {

//        }
//        public async Task Index_Get_InvalidRequest_Returns_NotImplementedException()
//        {

//        }
//    }
//}