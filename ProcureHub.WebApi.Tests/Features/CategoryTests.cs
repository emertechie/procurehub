using System.Net;
using System.Net.Http.Json;
using ProcureHub.Constants;
using ProcureHub.Features.Categories;
using ProcureHub.WebApi.Responses;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;
using ProcureHub.WebApi.Tests.Infrastructure.Helpers;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Features;

/// <summary>
/// NOTE: DB is only reset once per class instance, so only use for tests that don't persist state
/// </summary>
[Collection("ApiTestHost")]
public class CategoryTestsWithSharedDb(
    ApiTestHostFixture hostFixture,
    ITestOutputHelper testOutputHelper,
    UserSetupFixture userSetupFixture)
    : HttpClientBase(hostFixture, testOutputHelper),
        IClassFixture<ResetDatabaseFixture>,
        IClassFixture<UserSetupFixture>,
        IAsyncLifetime
{
    private const string RequesterUserEmail = "user1@example.com";
    private const string RequesterUserPassword = "Test1234!";

    public async ValueTask InitializeAsync()
    {
        await userSetupFixture.EnsureUserCreated(this,
            AdminEmail,
            AdminPassword,
            RequesterUserEmail,
            RequesterUserPassword,
            RoleNames.Requester
        );
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public static TheoryData<EndpointInfo> GetAllCategoryEndpoints()
    {
        return new TheoryData<EndpointInfo>
        {
            new EndpointInfo("/categories", "POST", "CreateCategory", new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/categories", "GET", "QueryCategories"),
            new EndpointInfo("/categories/{id}", "GET", "GetCategoryById"),
            new EndpointInfo("/categories/{id}", "PUT", "UpdateCategory", new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/categories/{id}", "DELETE", "DeleteCategory", new EndpointTestOptions { RequiresAdmin = true })
        };
    }

    [Theory]
    [MemberData(nameof(GetAllCategoryEndpoints))]
    public async Task All_category_endpoints_require_authentication(EndpointInfo endpoint)
    {
        // Note: Not logging in as anyone initially

        var testId = Guid.NewGuid();

        var path = endpoint.Path.Replace("{id}", testId.ToString());
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAllCategoryEndpoints))]
    public async Task Category_endpoints_enforce_admin_authorization_correctly(EndpointInfo endpoint)
    {
        // Log in as a regular user, not an admin
        await LoginAsync(RequesterUserEmail, RequesterUserPassword);

        var testId = Guid.NewGuid();

        var path = endpoint.Path.Replace("{id}", testId.ToString());
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);

        if (endpoint.Options?.RequiresAdmin ?? false)
        {
            Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
        }
        else
        {
            // Should NOT be Forbidden (403), but could be 404 or other response
            Assert.NotEqual(HttpStatusCode.Forbidden, resp.StatusCode);
        }
    }

    #region Endpoint Validation Tests

    [Theory]
    [MemberData(nameof(GetAllCategoryEndpoints))]
    public void All_category_endpoints_have_validation_tests(EndpointInfo endpoint)
    {
        // Verify test method exists using reflection
        var testMethod = GetType().GetMethod($"Test_{endpoint.Name}_validation");
        Assert.NotNull(testMethod);
    }

    [Fact]
    public async Task Test_CreateCategory_validation()
    {
        await LoginAsAdminAsync();

        // No name
        var reqNoName = new CreateCategory.Request(null!);
        var respNoName = await HttpClient.PostAsync("/categories", JsonContent.Create(reqNoName));
        await respNoName.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Name"] = ["'Name' must not be empty."] });
    }

    [Fact]
    public async Task Test_QueryCategories_validation()
    {
        // No validation - no parameters
    }

    [Fact]
    public async Task Test_GetCategoryById_validation()
    {
        // No validation - id comes from route only
    }

    [Fact]
    public async Task Test_UpdateCategory_validation()
    {
        await LoginAsAdminAsync();

        // No name
        var reqNoName = new UpdateCategory.Request(Guid.NewGuid(), null!);
        var respNoName = await HttpClient.PutAsync($"/categories/{reqNoName.Id}", JsonContent.Create(reqNoName));
        await respNoName.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Name"] = ["'Name' must not be empty."] });

        // Route id must match body id
        var catId = Guid.NewGuid();
        var updateReq = new UpdateCategory.Request(catId, "Software");
        var differentId = Guid.NewGuid();
        var updateResp = await HttpClient.PutAsync($"/categories/{differentId}", JsonContent.Create(updateReq));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Route ID mismatch",
            "Route ID does not match request ID",
            $"PUT /categories/{differentId}");
    }

    [Fact]
    public async Task Test_DeleteCategory_validation()
    {
        // No validation - id comes from route only
    }

    #endregion
}

[Collection("ApiTestHost")]
public class CategoryTests(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientAndDbResetBase(hostFixture, testOutputHelper)
{
    [Fact]
    public async Task Can_create_and_fetch_category()
    {
        await LoginAsAdminAsync();

        // Create category
        var createCatReq = new CreateCategory.Request("New Category");
        var createCatResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createCatReq));
        Assert.Equal(HttpStatusCode.Created, createCatResp.StatusCode);

        // Extract category ID from response
        var createdCat = await createCatResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var newCategoryId = createdCat.Id;

        // Assert category returned in list
        var categories = await HttpClient.GetAsync("/categories")
            .ReadJsonAsync<DataResponse<QueryCategories.Response[]>>();
        Assert.Contains(categories.Data, c => c.Id == newCategoryId && c.Name == "New Category");

        // Assert can get category by ID
        var category = await HttpClient.GetAsync($"/categories/{newCategoryId}")
            .ReadJsonAsync<DataResponse<GetCategoryById.Response>>();
        Assert.Equal(newCategoryId, category.Data.Id);
        Assert.Equal("New Category", category.Data.Name);
    }

    [Fact]
    public async Task Cannot_create_category_with_duplicate_name()
    {
        await LoginAsAdminAsync();

        const string categoryName = "Facilities";

        var createReq = new CreateCategory.Request(categoryName);
        var createResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createReq));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var duplicateResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createReq));
        await duplicateResp.AssertValidationProblemAsync(
            title: "Category name must be unique.",
            detail: "Category.DuplicateName",
            errors: new Dictionary<string, string[]>
            {
                ["Name"] = [$"Category '{categoryName}' already exists."]
            });
    }

    [Fact]
    public async Task Cannot_update_category_to_existing_name()
    {
        await LoginAsAdminAsync();

        const string duplicateName = "Facilities";
        const string equipmentName = "Equipment";

        var createFacilitiesReq = new CreateCategory.Request(duplicateName);
        var facilitiesResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createFacilitiesReq));
        Assert.Equal(HttpStatusCode.Created, facilitiesResp.StatusCode);

        var createEquipmentReq = new CreateCategory.Request(equipmentName);
        var equipmentResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createEquipmentReq));
        var equipmentCat = await equipmentResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var equipmentId = equipmentCat.Id;

        var updateReq = new UpdateCategory.Request(equipmentId, duplicateName);
        var updateResp = await HttpClient.PutAsync($"/categories/{equipmentId}", JsonContent.Create(updateReq));

        await updateResp.AssertValidationProblemAsync(
            title: "Category name must be unique.",
            detail: "Category.DuplicateName",
            errors: new Dictionary<string, string[]>
            {
                ["Name"] = [$"Category '{duplicateName}' already exists."]
            });

        var equipment = await HttpClient.GetAsync($"/categories/{equipmentId}")
            .ReadJsonAsync<DataResponse<GetCategoryById.Response>>();
        Assert.Equal(equipmentName, equipment.Data.Name);
    }

    [Fact]
    public async Task Admin_can_update_category_name()
    {
        await LoginAsAdminAsync();

        // Create category
        var createReq = new CreateCategory.Request("Consulting");
        var createResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createReq));
        var createdCat = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var catId = createdCat.Id;

        // Update category name
        var updateReq = new UpdateCategory.Request(catId, "Consulting Services");
        var updateResp = await HttpClient.PutAsync($"/categories/{catId}", JsonContent.Create(updateReq));
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        // Verify update
        var getCatResp = await HttpClient.GetAsync($"/categories/{catId}")
            .ReadJsonAsync<DataResponse<GetCategoryById.Response>>();
        Assert.Equal("Consulting Services", getCatResp.Data.Name);
    }

    [Fact]
    public async Task Update_category_returns_not_found_for_nonexistent_category()
    {
        await LoginAsAdminAsync();

        var nonexistentId = Guid.NewGuid();
        var updateReq = new UpdateCategory.Request(nonexistentId, "Nonexistent Category");
        var updateResp = await HttpClient.PutAsync($"/categories/{nonexistentId}", JsonContent.Create(updateReq));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Category not found",
            "NotFound",
            $"PUT /categories/{nonexistentId}");
    }

    [Fact]
    public async Task Admin_can_delete_unused_category()
    {
        await LoginAsAdminAsync();

        // Create category
        var createReq = new CreateCategory.Request("Temporary Category");
        var createResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createReq));
        var createdCat = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var catId = createdCat.Id;

        // Verify category exists in list
        var catsBefore = await HttpClient.GetAsync("/categories")
            .ReadJsonAsync<DataResponse<QueryCategories.Response[]>>();
        Assert.Contains(catsBefore.Data, c => c.Id == catId);

        // Delete category
        var deleteResp = await HttpClient.DeleteAsync($"/categories/{catId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Verify deletion - category no longer in list
        var catsAfter = await HttpClient.GetAsync("/categories")
            .ReadJsonAsync<DataResponse<QueryCategories.Response[]>>();
        Assert.DoesNotContain(catsAfter.Data, c => c.Id == catId);
    }

    [Fact]
    public async Task Cannot_delete_category_with_purchase_requests()
    {
        // TODO: Implement once PurchaseRequest model is created
        // This test should:
        // 1. Create a category
        // 2. Create a purchase request with that category
        // 3. Attempt to delete the category
        // 4. Assert that deletion fails with validation error
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Delete_category_returns_not_found_for_nonexistent_category()
    {
        await LoginAsAdminAsync();

        var unknownCatId = Guid.NewGuid();
        var deleteResp = await HttpClient.DeleteAsync($"/categories/{unknownCatId}");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Category not found",
            "NotFound",
            $"DELETE /categories/{unknownCatId}");
    }
}
