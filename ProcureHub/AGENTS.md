
# Models

- Define model mappings in an `IEntityTypeConfiguration<TModel>` type in the same file as the model
- Always use uppercase names for tables
- Entity IDs should use UUID v7, generated on server. Follow `Department.Id` mapping example:
```cs
builder.Property(d => d.Id)
    .HasDefaultValueSql("uuidv7()");
```
- Ensure model property validation like max length is validated in the corresponding request handler(s)
  - Use a shared static variable to avoid hard coding max length in multiple places
  - Good example: usage of `Department.NameMaxLength`
- For non-nullable value type props like `public DateTime CreatedAt { get; set; }`, don't bother creating an explicit `.Required()` mapping in the `IEntityTypeConfiguration`.
