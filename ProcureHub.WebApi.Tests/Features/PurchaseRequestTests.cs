using System.Net;
using System.Net.Http.Json;
using ProcureHub.Constants;
using ProcureHub.Features.Categories;
using ProcureHub.Features.Departments;
using ProcureHub.Features.PurchaseRequests;
using ProcureHub.Models;
using ProcureHub.WebApi.Responses;
using ProcureHub.WebApi.Tests.Infrastructure.BaseTestTypes;
using ProcureHub.WebApi.Tests.Infrastructure.Helpers;
using ProcureHub.WebApi.Tests.Infrastructure.Xunit;

namespace ProcureHub.WebApi.Tests.Features;

/// <summary>
/// NOTE: DB is only reset once per class instance, so only use for tests that don't persist state
/// </summary>
[Collection("ApiTestHost")]
public class PurchaseRequestTestsWithSharedDb(
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

    public static TheoryData<EndpointInfo> GetAllPurchaseRequestEndpoints()
    {
        return new TheoryData<EndpointInfo>
        {
            new EndpointInfo("/purchase-requests", "POST", "CreatePurchaseRequest"),
            new EndpointInfo("/purchase-requests", "GET", "QueryPurchaseRequests"),
            new EndpointInfo("/purchase-requests/{id}", "GET", "GetPurchaseRequestById"),
            new EndpointInfo("/purchase-requests/{id}", "PUT", "UpdatePurchaseRequest"),
            new EndpointInfo("/purchase-requests/{id}/submit", "POST", "SubmitPurchaseRequest"),
            new EndpointInfo("/purchase-requests/{id}/approve", "POST", "ApprovePurchaseRequest",
                new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/purchase-requests/{id}/reject", "POST", "RejectPurchaseRequest",
                new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/purchase-requests/{id}", "DELETE", "DeletePurchaseRequest")
        };
    }

    [Theory]
    [MemberData(nameof(GetAllPurchaseRequestEndpoints))]
    public async Task All_purchase_request_endpoints_require_authentication(EndpointInfo endpoint)
    {
        // Note: Not logging in as anyone initially

        var testId = Guid.NewGuid();

        var path = endpoint.Path.Replace("{id}", testId.ToString());
        var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), path);

        var resp = await HttpClient.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Theory]
    [MemberData(nameof(GetAllPurchaseRequestEndpoints))]
    public async Task Purchase_request_endpoints_enforce_admin_authorization_correctly(EndpointInfo endpoint)
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
    [MemberData(nameof(GetAllPurchaseRequestEndpoints))]
    public void All_purchase_request_endpoints_have_validation_tests(EndpointInfo endpoint)
    {
        // Verify test method exists using reflection
        var testMethod = GetType().GetMethod($"Test_{endpoint.Name}_validation");
        Assert.NotNull(testMethod);
    }

    [Fact]
    public async Task Test_CreatePurchaseRequest_validation()
    {
        await LoginAsync(RequesterUserEmail, RequesterUserPassword);

        // No title
        var reqNoTitle = new CreatePurchaseRequest.Request(
            Title: null!,
            Description: "Test description",
            EstimatedAmount: 1000,
            BusinessJustification: "Business need",
            CategoryId: Guid.NewGuid(),
            DepartmentId: Guid.NewGuid(),
            RequesterUserId: "user-id"
        );
        var respNoTitle = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(reqNoTitle));
        await respNoTitle.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Title"] = ["'Title' must not be empty."] });

        // Invalid amount (zero)
        var reqZeroAmount = new CreatePurchaseRequest.Request(
            Title: "Test Request",
            Description: "Test description",
            EstimatedAmount: 0,
            BusinessJustification: "Business need",
            CategoryId: Guid.NewGuid(),
            DepartmentId: Guid.NewGuid(),
            RequesterUserId: "user-id"
        );
        var respZeroAmount = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(reqZeroAmount));
        await respZeroAmount.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["EstimatedAmount"] = ["'Estimated Amount' must be greater than '0'."] });
    }

    [Fact]
    public async Task Test_QueryPurchaseRequests_validation()
    {
        // No validation - all parameters are optional
    }

    [Fact]
    public async Task Test_GetPurchaseRequestById_validation()
    {
        // No validation - id comes from route only
    }

    [Fact]
    public async Task Test_UpdatePurchaseRequest_validation()
    {
        await LoginAsync(RequesterUserEmail, RequesterUserPassword);

        // No title
        var reqNoTitle = new UpdatePurchaseRequest.Request(
            Id: Guid.NewGuid(),
            Title: null!,
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: Guid.NewGuid(),
            DepartmentId: Guid.NewGuid()
        );
        var respNoTitle = await HttpClient.PutAsync($"/purchase-requests/{reqNoTitle.Id}", JsonContent.Create(reqNoTitle));
        await respNoTitle.AssertValidationProblemAsync(
            errors: new Dictionary<string, string[]> { ["Title"] = ["'Title' must not be empty."] });

        // Route id must match body id
        var prId = Guid.NewGuid();
        var updateReq = new UpdatePurchaseRequest.Request(
            Id: prId,
            Title: "Test",
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: Guid.NewGuid(),
            DepartmentId: Guid.NewGuid()
        );
        var differentId = Guid.NewGuid();
        var updateResp = await HttpClient.PutAsync($"/purchase-requests/{differentId}", JsonContent.Create(updateReq));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Route ID mismatch",
            "Route ID does not match request ID",
            $"PUT /purchase-requests/{differentId}");
    }

    [Fact]
    public async Task Test_SubmitPurchaseRequest_validation()
    {
        // No validation - id comes from route only
    }

    [Fact]
    public async Task Test_ApprovePurchaseRequest_validation()
    {
        // No validation - id comes from route only
    }

    [Fact]
    public async Task Test_RejectPurchaseRequest_validation()
    {
        // No validation - id comes from route only
    }

    [Fact]
    public async Task Test_DeletePurchaseRequest_validation()
    {
        // No validation - id comes from route only
    }

    #endregion
}

[Collection("ApiTestHost")]
public class PurchaseRequestTests(ApiTestHostFixture hostFixture, ITestOutputHelper testOutputHelper)
    : HttpClientAndDbResetBase(hostFixture, testOutputHelper)
{
    private async Task<Guid> CreateCategoryAsync(string name = "TestCategory")
    {
        await LoginAsAdminAsync();
        // Use unique names by appending timestamp or random guid
        var uniqueName = $"{name}-{Guid.NewGuid().ToString()[..8]}";
        var createReq = new CreateCategory.Request(uniqueName);
        var createResp = await HttpClient.PostAsync("/categories", JsonContent.Create(createReq));
        var created = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        return created.Id;
    }

    private async Task<Guid> CreateDepartmentAsync(string name = "TestDept")
    {
        await LoginAsAdminAsync();
        // Use unique names by appending timestamp or random guid
        var uniqueName = $"{name}-{Guid.NewGuid().ToString()[..8]}";
        var createReq = new CreateDepartment.Request(uniqueName);
        var createResp = await HttpClient.PostAsync("/departments", JsonContent.Create(createReq));
        var created = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        return created.Id;
    }

    private async Task<Guid> CreatePurchaseRequestAsync(
        Guid categoryId,
        Guid departmentId,
        string title = "New Laptop",
        decimal amount = 1500)
    {
        var createReq = new CreatePurchaseRequest.Request(
            Title: title,
            Description: "Need new laptop for development",
            EstimatedAmount: amount,
            BusinessJustification: "Current laptop is outdated",
            CategoryId: categoryId,
            DepartmentId: departmentId,
            RequesterUserId: "will-be-replaced"
        );
        var createResp = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(createReq));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        return created.Id;
    }

    [Fact]
    public async Task Can_create_and_fetch_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync("Software");
        var departmentId = await CreateDepartmentAsync("IT");
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        // Log in as a requester
        await LoginAsync("requester@example.com", ValidPassword);

        // Create purchase request
        var createReq = new CreatePurchaseRequest.Request(
            Title: "Microsoft Office License",
            Description: "Need license for new employee",
            EstimatedAmount: 500,
            BusinessJustification: "New hire requires productivity tools",
            CategoryId: categoryId,
            DepartmentId: departmentId,
            RequesterUserId: "will-be-replaced"
        );
        var createResp = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(createReq));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);

        var created = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        var prId = created.Id;

        // Get by ID
        var getPrResp = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(prId, getPrResp.Data.Id);
        Assert.Equal("Microsoft Office License", getPrResp.Data.Title);
        Assert.Equal(500, getPrResp.Data.EstimatedAmount);
        Assert.Equal(PurchaseRequestStatus.Draft, getPrResp.Data.Status);
        Assert.Null(getPrResp.Data.SubmittedAt);

        // Query - should appear in list
        var queryResp = await HttpClient.GetAsync("/purchase-requests")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.Contains(queryResp.Data, pr => pr.Id == prId);
    }

    [Fact]
    public async Task Cannot_create_purchase_request_with_nonexistent_category()
    {
        await LoginAsAdminAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);

        var nonexistentCategoryId = Guid.NewGuid();
        var createReq = new CreatePurchaseRequest.Request(
            Title: "Test Request",
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: nonexistentCategoryId,
            DepartmentId: departmentId,
            RequesterUserId: "will-be-replaced"
        );
        var createResp = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(createReq));

        await createResp.AssertValidationProblemAsync(
            title: "Category not found",
            detail: "PurchaseRequest.CategoryNotFound",
            errors: new Dictionary<string, string[]>
            {
                ["CategoryId"] = ["The specified category does not exist."]
            });
    }

    [Fact]
    public async Task Cannot_create_purchase_request_with_nonexistent_department()
    {
        await LoginAsAdminAsync();
        var departmentId = await CreateDepartmentAsync();
        var categoryId = await CreateCategoryAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);

        var nonexistentDepartmentId = Guid.NewGuid();
        var createReq = new CreatePurchaseRequest.Request(
            Title: "Test Request",
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: categoryId,
            DepartmentId: nonexistentDepartmentId,
            RequesterUserId: "will-be-replaced"
        );
        var createResp = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(createReq));

        await createResp.AssertValidationProblemAsync(
            title: "Department not found",
            detail: "PurchaseRequest.DepartmentNotFound",
            errors: new Dictionary<string, string[]>
            {
                ["DepartmentId"] = ["The specified department does not exist."]
            });
    }

    [Fact]
    public async Task Can_update_draft_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync("New Category");
        var departmentId = await CreateDepartmentAsync("New Department");
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId, "Original Title", 1000);

        // Update the purchase request
        var updateReq = new UpdatePurchaseRequest.Request(
            Id: prId,
            Title: "Updated Title",
            Description: "Updated description",
            EstimatedAmount: 2000,
            BusinessJustification: "Updated justification",
            CategoryId: categoryId,
            DepartmentId: departmentId
        );
        var updateResp = await HttpClient.PutAsync($"/purchase-requests/{prId}", JsonContent.Create(updateReq));
        Assert.Equal(HttpStatusCode.NoContent, updateResp.StatusCode);

        // Verify update
        var getResp = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal("Updated Title", getResp.Data.Title);
        Assert.Equal(2000, getResp.Data.EstimatedAmount);
        Assert.Equal(PurchaseRequestStatus.Draft, getResp.Data.Status);
    }

    [Fact]
    public async Task Can_submit_draft_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Verify initial state is Draft
        var getDraft = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Draft, getDraft.Data.Status);
        Assert.Null(getDraft.Data.SubmittedAt);

        // Submit the request
        var submitResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);
        Assert.Equal(HttpStatusCode.NoContent, submitResp.StatusCode);

        // Verify new state is Pending with SubmittedAt set
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Pending, getPending.Data.Status);
        Assert.NotNull(getPending.Data.SubmittedAt);
    }

    [Fact]
    public async Task Cannot_submit_already_pending_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Submit the first time
        var submitResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);
        Assert.Equal(HttpStatusCode.NoContent, submitResp.StatusCode);

        // Try to submit again
        var submitAgainResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        await submitAgainResp.AssertValidationProblemAsync(
            title: "Invalid status transition",
            detail: "PurchaseRequest.InvalidStatusTransition",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Cannot submit a purchase request that is not in Draft status."]
            });
    }

    [Fact]
    public async Task Cannot_update_non_draft_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Submit to change status to Pending
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Try to update
        var updateReq = new UpdatePurchaseRequest.Request(
            Id: prId,
            Title: "Trying to update",
            Description: "Test",
            EstimatedAmount: 5000,
            BusinessJustification: "Test",
            CategoryId: categoryId,
            DepartmentId: departmentId
        );
        var updateResp = await HttpClient.PutAsync($"/purchase-requests/{prId}", JsonContent.Create(updateReq));

        await updateResp.AssertValidationProblemAsync(
            title: "Cannot update submitted request",
            detail: "PurchaseRequest.CannotUpdateNonDraft",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Only purchase requests in Draft status can be updated."]
            });
    }

    [Fact]
    public async Task Approver_can_approve_pending_purchase_request()
    {
        // Log in as admin and set up related entities and users
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        string approverUserId = await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        // Log in as a requester
        await LoginAsync("requester@example.com", ValidPassword);

        // Create and submit a PR to make it Pending
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Verify Pending status
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Pending, getPending.Data.Status);
        Assert.Null(getPending.Data.ReviewedAt);
        Assert.Null(getPending.Data.ReviewedBy);

        // Log in as approver
        await LoginAsync("approver@example.com", ValidPassword);

        // Approve
        var approveResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResp.StatusCode);

        // Verify Approved status with ReviewedAt and ReviewedBy set
        var getApproved = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Approved, getApproved.Data.Status);
        Assert.NotNull(getApproved.Data.ReviewedAt);
        Assert.Equal(approverUserId, getApproved.Data.ReviewedBy?.Id);
    }

    [Fact]
    public async Task Cannot_approve_own_purchase_request()
    {
        // Log in as admin and set up related entities and users
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester-approver@example.com", ValidPassword, [RoleNames.Requester, RoleNames.Approver], departmentId);

        // Log in as a user with Approver role who will create their own request
        await LoginAsync("requester-approver@example.com", ValidPassword);

        // Create and submit a PR as this user
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Verify Pending status
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Pending, getPending.Data.Status);

        // Try to approve own request (still logged in as the requester)
        var approveResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/approve", null);

        await approveResp.AssertProblemDetailsAsync(
            HttpStatusCode.BadRequest,
            "Cannot approve your own request",
            "PurchaseRequest.CannotApproveOwnRequest",
            $"POST /purchase-requests/{prId}/approve");

        // Verify still Pending
        var getStillPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Pending, getStillPending.Data.Status);
    }

    [Fact]
    public async Task Approver_can_reject_pending_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        string approverUserId = await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Submit to make it Pending
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Verify Pending status
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Pending, getPending.Data.Status);

        // Login as approver and reject
        await LoginAsync("approver@example.com", ValidPassword);
        var rejectResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/reject", null);
        Assert.Equal(HttpStatusCode.NoContent, rejectResp.StatusCode);

        // Verify Rejected status with ReviewedAt and ReviewedBy set
        var getRejected = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(PurchaseRequestStatus.Rejected, getRejected.Data.Status);
        Assert.NotNull(getRejected.Data.ReviewedAt);
        Assert.Equal(approverUserId, getRejected.Data.ReviewedBy?.Id);
    }

    [Fact]
    public async Task Cannot_approve_draft_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Login as approver and try to approve without submitting first
        await LoginAsync("approver@example.com", ValidPassword);
        var approveResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/approve", null);

        await approveResp.AssertValidationProblemAsync(
            title: "Invalid status transition",
            detail: "PurchaseRequest.InvalidStatusTransition",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Can only approve purchase requests in Pending status."]
            });
    }

    [Fact]
    public async Task Cannot_reject_draft_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Login as approver and try to reject without submitting first
        await LoginAsync("approver@example.com", ValidPassword);
        var rejectResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/reject", null);

        await rejectResp.AssertValidationProblemAsync(
            title: "Invalid status transition",
            detail: "PurchaseRequest.InvalidStatusTransition",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Can only reject purchase requests in Pending status."]
            });
    }

    [Fact]
    public async Task Can_delete_draft_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Verify exists
        var getBefore = await HttpClient.GetAsync($"/purchase-requests/{prId}");
        Assert.Equal(HttpStatusCode.OK, getBefore.StatusCode);

        // Delete
        var deleteResp = await HttpClient.DeleteAsync($"/purchase-requests/{prId}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResp.StatusCode);

        // Verify deleted
        var getAfter = await HttpClient.GetAsync($"/purchase-requests/{prId}");
        Assert.Equal(HttpStatusCode.NotFound, getAfter.StatusCode);
    }

    [Fact]
    public async Task Cannot_delete_pending_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Submit to make it Pending
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Try to delete
        var deleteResp = await HttpClient.DeleteAsync($"/purchase-requests/{prId}");

        await deleteResp.AssertValidationProblemAsync(
            title: "Cannot delete non-draft request",
            detail: "PurchaseRequest.CannotDeleteNonDraft",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Only purchase requests in Draft status can be deleted."]
            });

        // Verify still exists
        var getAfter = await HttpClient.GetAsync($"/purchase-requests/{prId}");
        Assert.Equal(HttpStatusCode.OK, getAfter.StatusCode);
    }

    [Fact]
    public async Task Cannot_delete_approved_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Submit
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Login as approver and approve
        await LoginAsync("approver@example.com", ValidPassword);
        await HttpClient.PostAsync($"/purchase-requests/{prId}/approve", null);

        // Login back as requester to try delete
        await LoginAsync("requester@example.com", ValidPassword);

        // Try to delete
        var deleteResp = await HttpClient.DeleteAsync($"/purchase-requests/{prId}");

        await deleteResp.AssertValidationProblemAsync(
            title: "Cannot delete non-draft request",
            detail: "PurchaseRequest.CannotDeleteNonDraft",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Only purchase requests in Draft status can be deleted."]
            });
    }

    [Fact]
    public async Task Cannot_delete_rejected_purchase_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var prId = await CreatePurchaseRequestAsync(categoryId, departmentId);

        // Submit
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Login as approver and reject
        await LoginAsync("approver@example.com", ValidPassword);
        await HttpClient.PostAsync($"/purchase-requests/{prId}/reject", null);

        // Login back as requester to try delete
        await LoginAsync("requester@example.com", ValidPassword);

        // Try to delete
        var deleteResp = await HttpClient.DeleteAsync($"/purchase-requests/{prId}");

        await deleteResp.AssertValidationProblemAsync(
            title: "Cannot delete non-draft request",
            detail: "PurchaseRequest.CannotDeleteNonDraft",
            errors: new Dictionary<string, string[]>
            {
                ["Status"] = ["Only purchase requests in Draft status can be deleted."]
            });
    }

    [Fact]
    public async Task Can_query_purchase_requests_by_status()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);
        await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], departmentId);

        // Create multiple purchase requests in different states
        await LoginAsync("requester@example.com", ValidPassword);
        var draftId = await CreatePurchaseRequestAsync(categoryId, departmentId, "Draft Request", 1000);
        var pendingId = await CreatePurchaseRequestAsync(categoryId, departmentId, "Pending Request", 2000);
        var approvedId = await CreatePurchaseRequestAsync(categoryId, departmentId, "Approved Request", 3000);

        // Submit pending and approved requests
        await HttpClient.PostAsync($"/purchase-requests/{pendingId}/submit", null);
        await HttpClient.PostAsync($"/purchase-requests/{approvedId}/submit", null);

        // Login as approver to approve
        await LoginAsync("approver@example.com", ValidPassword);
        await HttpClient.PostAsync($"/purchase-requests/{approvedId}/approve", null);

        // Login as requester to query
        await LoginAsync("requester@example.com", ValidPassword);

        // Query by Draft status
        var draftQuery = await HttpClient.GetAsync("/purchase-requests?status=Draft")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.Contains(draftQuery.Data, pr => pr.Id == draftId);
        Assert.DoesNotContain(draftQuery.Data, pr => pr.Id == pendingId);
        Assert.DoesNotContain(draftQuery.Data, pr => pr.Id == approvedId);

        // Query by Pending status
        var pendingQuery = await HttpClient.GetAsync("/purchase-requests?status=Pending")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.DoesNotContain(pendingQuery.Data, pr => pr.Id == draftId);
        Assert.Contains(pendingQuery.Data, pr => pr.Id == pendingId);
        Assert.DoesNotContain(pendingQuery.Data, pr => pr.Id == approvedId);

        // Query by Approved status
        var approvedQuery = await HttpClient.GetAsync("/purchase-requests?status=Approved")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.DoesNotContain(approvedQuery.Data, pr => pr.Id == draftId);
        Assert.DoesNotContain(approvedQuery.Data, pr => pr.Id == pendingId);
        Assert.Contains(approvedQuery.Data, pr => pr.Id == approvedId);
    }

    [Fact]
    public async Task Can_search_purchase_requests_by_title()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);
        var laptopId = await CreatePurchaseRequestAsync(categoryId, departmentId, "New Laptop Purchase", 1500);
        var softwareId = await CreatePurchaseRequestAsync(categoryId, departmentId, "Software License", 500);

        // Search for "laptop"
        var laptopSearch = await HttpClient.GetAsync("/purchase-requests?search=laptop")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.Contains(laptopSearch.Data, pr => pr.Id == laptopId);
        Assert.DoesNotContain(laptopSearch.Data, pr => pr.Id == softwareId);

        // Search for "software"
        var softwareSearch = await HttpClient.GetAsync("/purchase-requests?search=software")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.DoesNotContain(softwareSearch.Data, pr => pr.Id == laptopId);
        Assert.Contains(softwareSearch.Data, pr => pr.Id == softwareId);
    }

    [Fact]
    public async Task Update_purchase_request_returns_not_found_for_nonexistent_request()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], departmentId);

        await LoginAsync("requester@example.com", ValidPassword);

        var nonexistentId = Guid.NewGuid();
        var updateReq = new UpdatePurchaseRequest.Request(
            Id: nonexistentId,
            Title: "Test",
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: categoryId,
            DepartmentId: departmentId
        );
        var updateResp = await HttpClient.PutAsync($"/purchase-requests/{nonexistentId}", JsonContent.Create(updateReq));

        await updateResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Purchase request not found",
            "NotFound",
            $"PUT /purchase-requests/{nonexistentId}");
    }

    [Fact]
    public async Task Delete_purchase_request_returns_not_found_for_nonexistent_request()
    {
        await LoginAsAdminAsync();
        await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester]);

        await LoginAsync("requester@example.com", ValidPassword);

        var nonexistentId = Guid.NewGuid();
        var deleteResp = await HttpClient.DeleteAsync($"/purchase-requests/{nonexistentId}");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Purchase request not found",
            "NotFound",
            $"DELETE /purchase-requests/{nonexistentId}");
    }

    #region Authorization Tests

    [Fact]
    public async Task Admin_can_see_all_purchase_requests()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var dept1Id = await CreateDepartmentAsync("Dept1");
        var dept2Id = await CreateDepartmentAsync("Dept2");

        // Create requester in dept1
        var requester1Id = await CreateUserAsync("requester1@example.com", ValidPassword, [RoleNames.Requester], dept1Id);

        // Create requester in dept2
        var requester2Id = await CreateUserAsync("requester2@example.com", ValidPassword, [RoleNames.Requester], dept2Id);

        // Login as requester1 and create PR
        await LoginAsync("requester1@example.com", ValidPassword);
        var pr1Id = await CreatePurchaseRequestAsync(categoryId, dept1Id, "Request from Dept1");

        // Login as requester2 and create PR
        await LoginAsync("requester2@example.com", ValidPassword);
        var pr2Id = await CreatePurchaseRequestAsync(categoryId, dept2Id, "Request from Dept2");

        // Login as admin
        await LoginAsAdminAsync();

        // Admin should see both requests
        var queryResp = await HttpClient.GetAsync("/purchase-requests")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.Contains(queryResp.Data, pr => pr.Id == pr1Id);
        Assert.Contains(queryResp.Data, pr => pr.Id == pr2Id);

        // Admin should access both by ID
        var getPr1Resp = await HttpClient.GetAsync($"/purchase-requests/{pr1Id}");
        Assert.Equal(HttpStatusCode.OK, getPr1Resp.StatusCode);

        var getPr2Resp = await HttpClient.GetAsync($"/purchase-requests/{pr2Id}");
        Assert.Equal(HttpStatusCode.OK, getPr2Resp.StatusCode);
    }

    [Fact]
    public async Task Requester_can_only_see_own_requests()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var deptId = await CreateDepartmentAsync();

        // Create two requesters in same department
        var requester1Id = await CreateUserAsync("requester1@example.com", ValidPassword, [RoleNames.Requester], deptId);
        var requester2Id = await CreateUserAsync("requester2@example.com", ValidPassword, [RoleNames.Requester], deptId);

        // Login as requester1 and create PR
        await LoginAsync("requester1@example.com", ValidPassword);
        var pr1Id = await CreatePurchaseRequestAsync(categoryId, deptId, "Request from Requester1");

        // Login as requester2 and create PR
        await LoginAsync("requester2@example.com", ValidPassword);
        var pr2Id = await CreatePurchaseRequestAsync(categoryId, deptId, "Request from Requester2");

        // Requester2 should see only own request
        var queryResp = await HttpClient.GetAsync("/purchase-requests")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.DoesNotContain(queryResp.Data, pr => pr.Id == pr1Id);
        Assert.Contains(queryResp.Data, pr => pr.Id == pr2Id);

        // Requester2 should not access requester1's PR
        var getPr1Resp = await HttpClient.GetAsync($"/purchase-requests/{pr1Id}");
        await getPr1Resp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Purchase request not found",
            "NotFound",
            $"GET /purchase-requests/{pr1Id}");

        // Requester2 should access own PR
        var getPr2Resp = await HttpClient.GetAsync($"/purchase-requests/{pr2Id}");
        Assert.Equal(HttpStatusCode.OK, getPr2Resp.StatusCode);
    }

    [Fact]
    public async Task Approver_with_department_can_see_department_requests_and_own_requests()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var dept1Id = await CreateDepartmentAsync("Dept1");
        var dept2Id = await CreateDepartmentAsync("Dept2");

        // Create approver assigned to dept1
        var approverId = await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver], dept1Id);

        // Create requester in dept1
        var requester1Id = await CreateUserAsync("requester1@example.com", ValidPassword, [RoleNames.Requester], dept1Id);

        // Create requester in dept2
        var requester2Id = await CreateUserAsync("requester2@example.com", ValidPassword, [RoleNames.Requester], dept2Id);

        // Login as requester1 and create PR in dept1
        await LoginAsync("requester1@example.com", ValidPassword);
        var pr1Id = await CreatePurchaseRequestAsync(categoryId, dept1Id, "Request from Dept1");

        // Login as requester2 and create PR in dept2
        await LoginAsync("requester2@example.com", ValidPassword);
        var pr2Id = await CreatePurchaseRequestAsync(categoryId, dept2Id, "Request from Dept2");

        // Add Requester role to approver so they can create PR
        await LoginAsAdminAsync();
        await AssignRoleToUserAsync(approverId, RoleNames.Requester);

        // Login as approver and create PR in dept2 (different from their assigned dept)
        await LoginAsync("approver@example.com", ValidPassword);
        var pr3Id = await CreatePurchaseRequestAsync(categoryId, dept2Id, "Approver's own request in Dept2");

        // Approver should see dept1 requests + own requests (even if in different dept)
        var queryResp = await HttpClient.GetAsync("/purchase-requests")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.Contains(queryResp.Data, pr => pr.Id == pr1Id); // Dept1 request
        Assert.DoesNotContain(queryResp.Data, pr => pr.Id == pr2Id); // Dept2 request (not theirs)
        Assert.Contains(queryResp.Data, pr => pr.Id == pr3Id); // Own request in Dept2

        // Approver should access dept1 PR
        var getPr1Resp = await HttpClient.GetAsync($"/purchase-requests/{pr1Id}");
        Assert.Equal(HttpStatusCode.OK, getPr1Resp.StatusCode);

        // Approver should NOT access dept2 PR (not theirs)
        var getPr2Resp = await HttpClient.GetAsync($"/purchase-requests/{pr2Id}");
        await getPr2Resp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Purchase request not found",
            "NotFound",
            $"GET /purchase-requests/{pr2Id}");

        // Approver should access own PR in dept2
        var getPr3Resp = await HttpClient.GetAsync($"/purchase-requests/{pr3Id}");
        Assert.Equal(HttpStatusCode.OK, getPr3Resp.StatusCode);
    }

    [Fact]
    public async Task Approver_without_department_can_only_see_own_requests()
    {
        await LoginAsAdminAsync();
        var categoryId = await CreateCategoryAsync();
        var deptId = await CreateDepartmentAsync();

        // Create approver WITHOUT department assignment, but with Requester role too
        var approverId = await CreateUserAsync("approver@example.com", ValidPassword, [RoleNames.Approver, RoleNames.Requester]);

        // Create another requester and PR
        var requesterId = await CreateUserAsync("requester@example.com", ValidPassword, [RoleNames.Requester], deptId);

        await LoginAsync("requester@example.com", ValidPassword);
        var otherUserPrId = await CreatePurchaseRequestAsync(categoryId, deptId, "Other user's request");

        // Login as approver without department and create own request
        await LoginAsync("approver@example.com", ValidPassword);
        var ownPrId = await CreatePurchaseRequestAsync(categoryId, deptId, "Approver's own request");

        // Query should return only own request, not other user's request
        var queryResp = await HttpClient.GetAsync("/purchase-requests")
            .ReadJsonAsync<PagedResponse<QueryPurchaseRequests.Response>>();
        Assert.Contains(queryResp.Data, pr => pr.Id == ownPrId);
        Assert.DoesNotContain(queryResp.Data, pr => pr.Id == otherUserPrId);

        // GetById should work for own request
        var getOwnResp = await HttpClient.GetAsync($"/purchase-requests/{ownPrId}");
        Assert.Equal(HttpStatusCode.OK, getOwnResp.StatusCode);

        // GetById should NOT work for other user's request
        var getOtherResp = await HttpClient.GetAsync($"/purchase-requests/{otherUserPrId}");
        await getOtherResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Purchase request not found",
            "NotFound",
            $"GET /purchase-requests/{otherUserPrId}");
    }

    #endregion
}
