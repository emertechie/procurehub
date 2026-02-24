using Bunit;
using NSubstitute;
using ProcureHub.BlazorApp.Tests.Infrastructure;
using ProcureHub.Common;
using ProcureHub.Common.Pagination;
using ProcureHub.Features.Users;
using ProcureHub.Infrastructure;
using Radzen;

namespace ProcureHub.BlazorApp.Tests.Features.Users;

using UsersIndex = ProcureHub.BlazorApp.Components.Pages.Admin.Users.Index;

public class UsersIndexTests : BlazorTestContext
{
    private static readonly QueryUsers.Response[] SampleUsers =
    [
        new(
            Id: Guid.NewGuid().ToString(),
            Email: "alice@example.com",
            FirstName: "Alice",
            LastName: "Smith",
            Roles: ["Admin", "Requester"],
            EnabledAt: DateTime.UtcNow,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            DeletedAt: null,
            Department: new QueryUsers.Department(Guid.NewGuid(), "Engineering")),
        new(
            Id: Guid.NewGuid().ToString(),
            Email: "bob@example.com",
            FirstName: "Bob",
            LastName: "Jones",
            Roles: ["Requester"],
            EnabledAt: null,
            CreatedAt: DateTime.UtcNow,
            UpdatedAt: DateTime.UtcNow,
            DeletedAt: null,
            Department: null)
    ];

    private static PagedResult<QueryUsers.Response> CreatePagedResult(
        QueryUsers.Response[]? users = null)
    {
        var data = users ?? SampleUsers;
        return new PagedResult<QueryUsers.Response>(data, 1, 10, data.Length);
    }

    private void SetupDefaultHandlers(PagedResult<QueryUsers.Response>? pagedResult = null)
    {
        AddMockQueryHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>>(
            pagedResult ?? CreatePagedResult());
        AddMockCommandHandler<EnableUser.Command, Result>(Result.Success());
        AddMockCommandHandler<DisableUser.Command, Result>(Result.Success());
    }

    [Fact]
    public void Renders_page_title_and_header()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert
        var h5 = cut.Find("h5");
        Assert.Contains("Users", h5.TextContent);
    }

    [Fact]
    public void Renders_user_names_in_grid()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert
        var markup = cut.Markup;
        
        Assert.Contains("Alice", markup);
        Assert.Contains("Smith", markup);
        Assert.Contains("Bob", markup);
        Assert.Contains("Jones", markup);
    }

    [Fact]
    public void Renders_user_emails_in_grid()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert
        Assert.Contains("alice@example.com", cut.Markup);
        Assert.Contains("bob@example.com", cut.Markup);
    }

    [Fact]
    public void Renders_role_badges()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert
        var markup = cut.Markup;
        Assert.Contains("Admin", markup);
        Assert.Contains("Requester", markup);
    }

    [Fact]
    public void Renders_enabled_badge_for_enabled_user()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert — Alice has EnabledAt set, should show "Enabled" badge
        Assert.Contains("Enabled", cut.Markup);
    }

    [Fact]
    public void Renders_disabled_badge_for_disabled_user()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert — Bob has EnabledAt = null, should show "Disabled" badge
        Assert.Contains("Disabled", cut.Markup);
    }

    [Fact]
    public void Renders_department_name_for_user_with_department()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert
        Assert.Contains("Engineering", cut.Markup);
    }

    [Fact]
    public void Renders_dash_for_user_without_department()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        var users = new[]
        {
            SampleUsers[1] // Bob — no department
        };
        SetupDefaultHandlers(CreatePagedResult(users));

        // Act
        var cut = Render<UsersIndex>();

        // Assert — should show "-" placeholder
        Assert.Contains("-", cut.Markup);
    }

    [Fact]
    public void Renders_empty_grid_when_no_users()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers(CreatePagedResult([]));

        // Act
        var cut = Render<UsersIndex>();

        // Assert — grid rendered but no user data
        Assert.DoesNotContain("alice@example.com", cut.Markup);
    }

    [Fact]
    public void Renders_create_user_button()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        SetupDefaultHandlers();

        // Act
        var cut = Render<UsersIndex>();

        // Assert
        Assert.Contains("Create User", cut.Markup);
    }

    [Fact]
    public async Task Calls_query_handler_on_initial_load()
    {
        // Arrange
        AuthorizeWithRoles("Admin");
        var handler = AddMockQueryHandler<QueryUsers.Request, PagedResult<QueryUsers.Response>>(
            CreatePagedResult());
        AddMockCommandHandler<EnableUser.Command, Result>(Result.Success());
        AddMockCommandHandler<DisableUser.Command, Result>(Result.Success());

        // Act
        Render<UsersIndex>();

        // Assert
        await handler.Received(1).HandleAsync(
            Arg.Any<QueryUsers.Request>(),
            Arg.Any<CancellationToken>());
    }
}
