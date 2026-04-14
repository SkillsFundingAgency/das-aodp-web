using System.Text.Encodings.Web;
using Microsoft.Extensions.Options;
using SFA.DAS.AODP.Models.Settings;
using SFA.DAS.AODP.Web.TagHelpers;

namespace SFA.DAS.AODP.Web.UnitTests.TagHelpers;

public class OfqualRegisterExternalLinkTagHelperUnitTests : TagHelpersUnitTestBase
{
    [Fact]
    public async Task Process_SuccessfullyRenderHtml()
    {
        // Arrange
        var tagHelper = new OfqualRegisterExternalLinkTagHelper(Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/",
        }))
        {
            QualificationReference = "12345"
        };

        var tagHelperContext = GenerateTagHelperContext(OfqualRegisterExternalLinkTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(OfqualRegisterExternalLinkTagHelper.TagName, []);

        // Act
        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        // Assert
        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        var html = writer.ToString();

        Assert.Equal("<a class=\"govuk-link\" target=\"12345\" href=\"https://find-a-qualification.services.ofqual.gov.uk/qualifications/12345\">12345</a>", html);
    }

    [Fact]
    public async Task Process_SuccessfullyRenderHtml_OpenInNamedTabFalse()
    {
        // Arrange
        var tagHelper = new OfqualRegisterExternalLinkTagHelper(Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/",
        }))
        {
            QualificationReference = "12345",
            OpensInNamedTab = false
        };

        var tagHelperContext = GenerateTagHelperContext(OfqualRegisterExternalLinkTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(OfqualRegisterExternalLinkTagHelper.TagName, []);

        // Act
        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        // Assert
        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        var html = writer.ToString();

        Assert.Equal("<a class=\"govuk-link\" target=\"_blank\" href=\"https://find-a-qualification.services.ofqual.gov.uk/qualifications/12345\">12345</a>", html);
    }

    [Fact]
    public async Task Process_SuccessfullyRenderHtml_OpenInNamedTabTrue()
    {
        // Arrange
        var tagHelper = new OfqualRegisterExternalLinkTagHelper(Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/",
        }))
        {
            QualificationReference = "12345",
            OpensInNamedTab = true
        };

        var tagHelperContext = GenerateTagHelperContext(OfqualRegisterExternalLinkTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(OfqualRegisterExternalLinkTagHelper.TagName, []);

        // Act
        await tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput);

        // Assert
        var writer = new StringWriter();
        tagHelperOutput.WriteTo(writer, HtmlEncoder.Default);

        var html = writer.ToString();

        Assert.Equal("<a class=\"govuk-link\" target=\"12345\" href=\"https://find-a-qualification.services.ofqual.gov.uk/qualifications/12345\">12345</a>", html);
    }

    [Fact]
    public async Task Process_QualificationReferenceNotPassed_ThrowNullReferenceException()
    {
        // Arrange
        var tagHelper = new OfqualRegisterExternalLinkTagHelper(Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = "https://find-a-qualification.services.ofqual.gov.uk/qualifications/",
        }));

        var tagHelperContext = GenerateTagHelperContext(OfqualRegisterExternalLinkTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(OfqualRegisterExternalLinkTagHelper.TagName, []);

        // Act
        await Assert.ThrowsAsync<TagHelperRenderingException>(() => tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput));
    }

    [Fact]
    public async Task Process_ConfigNotSet_ThrowNullReferenceException()
    {
        // Arrange
        var tagHelper = new OfqualRegisterExternalLinkTagHelper(Options.Create(new AodpConfiguration
        {
            FindRegulatedQualificationUrl = string.Empty!,
        }));

        var tagHelperContext = GenerateTagHelperContext(OfqualRegisterExternalLinkTagHelper.TagName);
        var tagHelperOutput = GenerateTagHelperOutput(OfqualRegisterExternalLinkTagHelper.TagName, []);

        // Act
        await Assert.ThrowsAsync<TagHelperRenderingException>(() => tagHelper.ProcessAsync(tagHelperContext, tagHelperOutput));
    }
}