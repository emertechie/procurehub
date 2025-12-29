
# Models

- Define model mappings in an `IEntityTypeConfiguration<TModel>` type in the same file as the model
- **Always use PascalCase table names** (EF Core default convention)
  - Incorrect: `builder.ToTable("purchase_requests");`
  - Correct: `builder.ToTable("PurchaseRequests");`
- **Do NOT use `HasColumnName()` for column mappings** - use EF Core's default convention (property names)
  - Incorrect: `.HasColumnName("request_number")`
  - Correct: Omit `HasColumnName()` entirely; EF will use property name `RequestNumber` as column name
- **Do NOT use `HasKey()`** - EF Core convention automatically detects `Id` property as primary key
- Entity IDs should use UUID v7, generated on server. Follow `Department.Id` mapping example:
```cs
builder.Property(d => d.Id)
    .HasDefaultValueSql("uuidv7()");
```
- **Do NOT use `ValueGeneratedOnAdd()` or `ValueGeneratedOnAddOrUpdate()`** for timestamps
- For non-nullable value type props like `public DateTime CreatedAt { get; set; }`, don't bother creating an explicit `.IsRequired()` mapping unless semantically important
- Ensure model property validation like max length is validated in the corresponding request handler(s)
  - Use shared static constants in the configuration class to avoid hard coding max length in multiple places
  - Good example: usage of `CategoryConfiguration.NameMaxLength`
  - Define constants like: `public const int NameMaxLength = 100;`

# Request Handlers

- Request handlers implement the `ProcureHub.Infrastructure.IRequestHandler<TRequest, TResponse>`
- Place request handlers in the appropriate `ProcureHub/Features/{FeatureName}` folder
- `GET`-related handlers should return a `PagedResult<Response>` for pageable queries, or just a plan `Response` DTO (or `Response[]`) for others
- Mutation-related handlers should return a `ProcureHub.Common.Result<T>` type to indicate either success or failure
  - Failure results use the `ProcureHub.Common.Error` type, which is not concerned with transport-specific error handling
  - `Error`s are converted to a problem details response in the API with the `.ToProblemDetails()` extension method
- Custom Error helpers or validation related types should go in a `Validation` folder within the feature folder
  - Example: `ProcureHub/Features/Departments/Validation/DepartmentErrors.cs`
