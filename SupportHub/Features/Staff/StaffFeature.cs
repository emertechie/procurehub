using SupportHub.Data;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Staff;

public static class CreateStaff
{
    public record Request(string UserId);
    
    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Request, string>
    {
        public async Task<string> HandleAsync(Request request, CancellationToken token)
        {
            var now = DateTime.UtcNow;
            var staff = new Models.Staff
            {
                UserId = request.UserId,
                CreatedAt = now,
                UpdatedAt = DateTime.UtcNow,
                EnabledAt = DateTime.UtcNow
            };
            dbContext.Staff.Add(staff);
            await dbContext.SaveChangesAsync(token);
        
            return staff.UserId;
        }
    }
}

