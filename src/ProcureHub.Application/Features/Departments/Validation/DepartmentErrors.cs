using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.Departments.Validation;

public static class DepartmentErrors
{
    public static Error DuplicateName(string name)
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = [$"Department '{name}' already exists."]
        };

        return Error.Validation("Department.DuplicateName", "Department name must be unique.", errors);
    }
}
