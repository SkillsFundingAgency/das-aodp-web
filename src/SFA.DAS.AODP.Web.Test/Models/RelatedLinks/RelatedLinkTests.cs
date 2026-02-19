using SFA.DAS.AODP.Web.Models.RelatedLinks;

namespace SFA.DAS.AODP.Web.UnitTests.Models.RelatedLinks
{
    public class RelatedLinkTests
    {
        [Fact]
        public void Constructor_NullText_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RelatedLink(null!, "/url"));
        }

        [Fact]
        public void Constructor_NullUrl_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new RelatedLink("Text", null!));
        }

        [Fact]
        public void Constructor_SetsPropertiesCorrectly()
        {
            var link = new RelatedLink("Text", "/url", false, "target");

            Assert.Equal("Text", link.Text);
            Assert.Equal("/url", link.Url);
            Assert.False(link.OpenInNewTab);
            Assert.Equal("target", link.TargetName);
        }
    }
}
