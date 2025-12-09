using SupportHub.Data;
using SupportHub.Infrastructure;

namespace SupportHub.Features.Staff;

public static class CreateStaff
{
    public record Command(string UserId);
    
    // TODO: validator

    public class Handler(ApplicationDbContext dbContext)
        : ICommandHandler<Command, string>
    {
        public async Task<string> HandleAsync(Command command)
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
            await dbContext.SaveChangesAsync();
        
            return staff.UserId;
        }
    }
}

