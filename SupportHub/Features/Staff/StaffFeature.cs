using SupportHub.Data;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Staff;

public static class CreateStaff
{
    public record Command(string UserId);
    
    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : IRequestHandler<Command, string>
    {
        public async Task<string> HandleAsync(Command command, CancellationToken token)
        {
            var now = DateTime.UtcNow;
            var staff = new Models.Staff
            {
                UserId = command.UserId,
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

