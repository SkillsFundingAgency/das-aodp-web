using AutoFixture.Kernel;

namespace SFA.DAS.AODP.UnitTests.Helper
{
    public class DateOnlySpecimenBuilder : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context)
        {
            if (request is Type type && type == typeof(DateOnly))
            {
                return new DateOnly(2023, 1, 1); 
            }

            return new NoSpecimen();
        }
    }
}
