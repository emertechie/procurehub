using ProcureHub.WebApi.Features.Auth;
using ProcureHub.WebApi.Features.Categories;
using ProcureHub.WebApi.Features.Departments;
using ProcureHub.WebApi.Features.PurchaseRequests;
using ProcureHub.WebApi.Features.Roles;
using ProcureHub.WebApi.Features.Users;

namespace ProcureHub.WebApi;

public static class ApiEndpoints
{
    public static void Configure(WebApplication app)
    {
        app.ConfigureAuthEndpoints();
        app.ConfigureDemoEndpoints();
        app.ConfigureUsersEndpoints();
        app.ConfigureDepartmentEndpoints();
        app.ConfigureCategoryEndpoints();
        app.ConfigurePurchaseRequestEndpoints();
        app.ConfigureRolesEndpoints();
    }
}
