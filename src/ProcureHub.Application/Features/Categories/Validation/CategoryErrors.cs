using ProcureHub.Domain.Common;

namespace ProcureHub.Application.Features.Categories.Validation;

public static class CategoryErrors
{
    public static Error DuplicateName(string name)
    {
        var errors = new Dictionary<string, string[]>
        {
            ["Name"] = [$"Category '{name}' already exists."]
        };

        return Error.Validation("Category.DuplicateName", "Category name must be unique.", errors);
    }
}
