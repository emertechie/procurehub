using System.Net;
using System.Net.Http.Json;
using ProcureHub.Common.Pagination;
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
    private const string ValidUserEmail = "user1@example.com";
    private const string ValidUserPassword = "Test1234!";

    public async ValueTask InitializeAsync()
    {
        await userSetupFixture.EnsureUserCreated(this, AdminEmail, AdminPassword, ValidUserEmail, ValidUserPassword);
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
            new EndpointInfo("/purchase-requests/{id}/approve", "POST", "ApprovePurchaseRequest", new EndpointTestOptions { RequiresAdmin = true }),
            new EndpointInfo("/purchase-requests/{id}/reject", "POST", "RejectPurchaseRequest", new EndpointTestOptions { RequiresAdmin = true }),
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
        await LoginAsync(ValidUserEmail, ValidUserPassword);

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
        await LoginAsync(ValidUserEmail, ValidUserPassword);

        // No title
        var reqNoTitle = new CreatePurchaseRequest.Request(
            Title: null!,
            Description: "Test description",
            EstimatedAmount: 1000,
            BusinessJustification: "Business need",
            CategoryId: Guid.NewGuid(),
            DepartmentId: Guid.NewGuid(),
            UserId: "user-id"
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
            UserId: "user-id"
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
        await LoginAsync(ValidUserEmail, ValidUserPassword);

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
        string title = "New Laptop",
        decimal amount = 1500,
        Guid? categoryId = null,
        Guid? departmentId = null)
    {
        var catId = categoryId ?? await CreateCategoryAsync();
        var deptId = departmentId ?? await CreateDepartmentAsync();

        await LoginAsAdminAsync();
        var createReq = new CreatePurchaseRequest.Request(
            Title: title,
            Description: "Need new laptop for development",
            EstimatedAmount: amount,
            BusinessJustification: "Current laptop is outdated",
            CategoryId: catId,
            DepartmentId: deptId,
            UserId: "will-be-replaced"
        );
        var createResp = await HttpClient.PostAsync("/purchase-requests", JsonContent.Create(createReq));
        Assert.Equal(HttpStatusCode.Created, createResp.StatusCode);
        var created = await createResp.ReadJsonAsync<EntityCreatedResponse<Guid>>();
        return created.Id;
    }

    [Fact]
    public async Task Can_create_and_fetch_purchase_request()
    {
        var categoryId = await CreateCategoryAsync("Software");
        var departmentId = await CreateDepartmentAsync("IT");

        await LoginAsAdminAsync();

        // Create purchase request
        var createReq = new CreatePurchaseRequest.Request(
            Title: "Microsoft Office License",
            Description: "Need license for new employee",
            EstimatedAmount: 500,
            BusinessJustification: "New hire requires productivity tools",
            CategoryId: categoryId,
            DepartmentId: departmentId,
            UserId: "will-be-replaced"
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
        Assert.Equal(Models.PurchaseRequestStatus.Draft, getPrResp.Data.Status);
        Assert.Null(getPrResp.Data.SubmittedAt);

        // Query - should appear in list
        var queryResp = await HttpClient.GetAsync("/purchase-requests")
            .ReadJsonAsync<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>();
        Assert.Contains(queryResp.Data.Data, pr => pr.Id == prId);
    }

    [Fact]
    public async Task Cannot_create_purchase_request_with_nonexistent_category()
    {
        var departmentId = await CreateDepartmentAsync();
        await LoginAsAdminAsync();

        var nonexistentCategoryId = Guid.NewGuid();
        var createReq = new CreatePurchaseRequest.Request(
            Title: "Test Request",
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: nonexistentCategoryId,
            DepartmentId: departmentId,
            UserId: "will-be-replaced"
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
        var categoryId = await CreateCategoryAsync();
        await LoginAsAdminAsync();

        var nonexistentDepartmentId = Guid.NewGuid();
        var createReq = new CreatePurchaseRequest.Request(
            Title: "Test Request",
            Description: "Test",
            EstimatedAmount: 1000,
            BusinessJustification: "Test",
            CategoryId: categoryId,
            DepartmentId: nonexistentDepartmentId,
            UserId: "will-be-replaced"
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
        var prId = await CreatePurchaseRequestAsync("Original Title", 1000);
        var categoryId = await CreateCategoryAsync("New Category");
        var departmentId = await CreateDepartmentAsync("New Department");

        await LoginAsAdminAsync();

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
        Assert.Equal(Models.PurchaseRequestStatus.Draft, getResp.Data.Status);
    }

    [Fact]
    public async Task Can_submit_draft_purchase_request()
    {
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Verify initial state is Draft
        var getDraft = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(Models.PurchaseRequestStatus.Draft, getDraft.Data.Status);
        Assert.Null(getDraft.Data.SubmittedAt);

        // Submit the request
        var submitResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);
        Assert.Equal(HttpStatusCode.NoContent, submitResp.StatusCode);

        // Verify new state is Pending with SubmittedAt set
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(Models.PurchaseRequestStatus.Pending, getPending.Data.Status);
        Assert.NotNull(getPending.Data.SubmittedAt);
    }

    [Fact]
    public async Task Cannot_submit_already_pending_purchase_request()
    {
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

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
        var prId = await CreatePurchaseRequestAsync();
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();

        await LoginAsAdminAsync();

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
    public async Task Admin_can_approve_pending_purchase_request()
    {
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Submit to make it Pending
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Verify Pending status
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(Models.PurchaseRequestStatus.Pending, getPending.Data.Status);
        Assert.Null(getPending.Data.ReviewedAt);
        Assert.Null(getPending.Data.ReviewedBy);

        // Approve
        var approveResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/approve", null);
        Assert.Equal(HttpStatusCode.NoContent, approveResp.StatusCode);

        // Verify Approved status with ReviewedAt and ReviewedBy set
        var getApproved = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(Models.PurchaseRequestStatus.Approved, getApproved.Data.Status);
        Assert.NotNull(getApproved.Data.ReviewedAt);
        Assert.NotNull(getApproved.Data.ReviewedBy);
    }

    [Fact]
    public async Task Admin_can_reject_pending_purchase_request()
    {
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Submit to make it Pending
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);

        // Verify Pending status
        var getPending = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(Models.PurchaseRequestStatus.Pending, getPending.Data.Status);

        // Reject
        var rejectResp = await HttpClient.PostAsync($"/purchase-requests/{prId}/reject", null);
        Assert.Equal(HttpStatusCode.NoContent, rejectResp.StatusCode);

        // Verify Rejected status with ReviewedAt and ReviewedBy set
        var getRejected = await HttpClient.GetAsync($"/purchase-requests/{prId}")
            .ReadJsonAsync<DataResponse<GetPurchaseRequestById.Response>>();
        Assert.Equal(Models.PurchaseRequestStatus.Rejected, getRejected.Data.Status);
        Assert.NotNull(getRejected.Data.ReviewedAt);
        Assert.NotNull(getRejected.Data.ReviewedBy);
    }

    [Fact]
    public async Task Cannot_approve_draft_purchase_request()
    {
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Try to approve without submitting first
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
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Try to reject without submitting first
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
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

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
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

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
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Submit and approve
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);
        await HttpClient.PostAsync($"/purchase-requests/{prId}/approve", null);

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
        var prId = await CreatePurchaseRequestAsync();

        await LoginAsAdminAsync();

        // Submit and reject
        await HttpClient.PostAsync($"/purchase-requests/{prId}/submit", null);
        await HttpClient.PostAsync($"/purchase-requests/{prId}/reject", null);

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
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();

        // Create multiple purchase requests in different states
        var draftId = await CreatePurchaseRequestAsync("Draft Request", 1000, categoryId, departmentId);
        var pendingId = await CreatePurchaseRequestAsync("Pending Request", 2000, categoryId, departmentId);
        var approvedId = await CreatePurchaseRequestAsync("Approved Request", 3000, categoryId, departmentId);

        await LoginAsAdminAsync();

        // Submit pending and approved requests
        await HttpClient.PostAsync($"/purchase-requests/{pendingId}/submit", null);
        await HttpClient.PostAsync($"/purchase-requests/{approvedId}/submit", null);
        await HttpClient.PostAsync($"/purchase-requests/{approvedId}/approve", null);

        // Query by Draft status
        var draftQuery = await HttpClient.GetAsync("/purchase-requests?status=Draft")
            .ReadJsonAsync<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>();
        Assert.Contains(draftQuery.Data.Data, pr => pr.Id == draftId);
        Assert.DoesNotContain(draftQuery.Data.Data, pr => pr.Id == pendingId);
        Assert.DoesNotContain(draftQuery.Data.Data, pr => pr.Id == approvedId);

        // Query by Pending status
        var pendingQuery = await HttpClient.GetAsync("/purchase-requests?status=Pending")
            .ReadJsonAsync<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>();
        Assert.DoesNotContain(pendingQuery.Data.Data, pr => pr.Id == draftId);
        Assert.Contains(pendingQuery.Data.Data, pr => pr.Id == pendingId);
        Assert.DoesNotContain(pendingQuery.Data.Data, pr => pr.Id == approvedId);

        // Query by Approved status
        var approvedQuery = await HttpClient.GetAsync("/purchase-requests?status=Approved")
            .ReadJsonAsync<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>();
        Assert.DoesNotContain(approvedQuery.Data.Data, pr => pr.Id == draftId);
        Assert.DoesNotContain(approvedQuery.Data.Data, pr => pr.Id == pendingId);
        Assert.Contains(approvedQuery.Data.Data, pr => pr.Id == approvedId);
    }

    [Fact]
    public async Task Can_search_purchase_requests_by_title()
    {
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();

        var laptopId = await CreatePurchaseRequestAsync("New Laptop Purchase", 1500, categoryId, departmentId);
        var softwareId = await CreatePurchaseRequestAsync("Software License", 500, categoryId, departmentId);

        await LoginAsAdminAsync();

        // Search for "laptop"
        var laptopSearch = await HttpClient.GetAsync("/purchase-requests?search=laptop")
            .ReadJsonAsync<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>();
        Assert.Contains(laptopSearch.Data.Data, pr => pr.Id == laptopId);
        Assert.DoesNotContain(laptopSearch.Data.Data, pr => pr.Id == softwareId);

        // Search for "software"
        var softwareSearch = await HttpClient.GetAsync("/purchase-requests?search=software")
            .ReadJsonAsync<DataResponse<PagedResult<QueryPurchaseRequests.Response>>>();
        Assert.DoesNotContain(softwareSearch.Data.Data, pr => pr.Id == laptopId);
        Assert.Contains(softwareSearch.Data.Data, pr => pr.Id == softwareId);
    }

    [Fact]
    public async Task Update_purchase_request_returns_not_found_for_nonexistent_request()
    {
        var categoryId = await CreateCategoryAsync();
        var departmentId = await CreateDepartmentAsync();

        await LoginAsAdminAsync();

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

        var nonexistentId = Guid.NewGuid();
        var deleteResp = await HttpClient.DeleteAsync($"/purchase-requests/{nonexistentId}");

        await deleteResp.AssertProblemDetailsAsync(
            HttpStatusCode.NotFound,
            "Purchase request not found",
            "NotFound",
            $"DELETE /purchase-requests/{nonexistentId}");
    }
}
