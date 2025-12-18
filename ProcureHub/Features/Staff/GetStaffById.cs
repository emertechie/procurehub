using FluentValidation;

using Microsoft.EntityFrameworkCore;

using ProcureHub.Infrastructure;

namespace ProcureHub.Features.Staff;

public static class GetStaffById
{
    public record Request(string Id);

    public class RequestValidator : AbstractValidator<Request>
    {
        public RequestValidator()
        {
            RuleFor(r => r.Id).NotEmpty();
        }
    }

    public record Response(
        string Id,
        string Email,
        int? DepartmentId,
        string? DepartmentName);

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, Response?>
    {
        public Task<Response?> HandleAsync(Request request, CancellationToken token)
        {
            return dbContext.Staff
                .AsNoTracking()
                .Where(s => s.UserId == request.Id)
                .Include(s => s.User)
                .Include(s => s.Department)
                .Select(s => new Response(
                    s.UserId,
                    s.User.Email!,
                    s.DepartmentId,
                    s.Department != null ? s.Department.Name : null))
                .FirstOrDefaultAsync(token);
        }
    }
}