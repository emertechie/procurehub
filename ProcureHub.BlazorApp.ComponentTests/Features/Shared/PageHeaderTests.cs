using Bunit;
using ProcureHub.BlazorApp.Components.Shared;
using ProcureHub.BlazorApp.Tests.Infrastructure;

namespace ProcureHub.BlazorApp.Tests.Features.Shared;

public class PageHeaderTests : BlazorTestContext
{
    [Fact]
    public void Renders_title()
    {
        // Act
        var cut = Render<PageHeader>(parameters => parameters
            .Add(p => p.Title, "Test Title"));

        // Assert
        Assert.Contains("Test Title", cut.Markup);
    }

    [Fact]
    public void Renders_subtitle_when_provided()
    {
        // Act
        var cut = Render<PageHeader>(parameters => parameters
            .Add(p => p.Title, "Title")
            .Add(p => p.Subtitle, "Some subtitle text"));

        // Assert
        Assert.Contains("Some subtitle text", cut.Markup);
    }

    [Fact]
    public void Does_not_render_subtitle_when_null()
    {
        // Act
        var cut = Render<PageHeader>(parameters => parameters
            .Add(p => p.Title, "Title"));

        // Assert — subtitle text style element should not appear for null subtitle
        var bodyTexts = cut.FindAll(".rz-text-body2");
        Assert.Empty(bodyTexts);
    }

    [Fact]
    public void Renders_action_button_when_provided()
    {
        // Act
        var cut = Render<PageHeader>(parameters => parameters
            .Add(p => p.Title, "Title")
            .Add(p => p.ActionButton, "<button>Click Me</button>"));

        // Assert
        Assert.Contains("Click Me", cut.Markup);
    }
}
