namespace ProcureHub.Infrastructure;

public class CustomLanguageManager : FluentValidation.Resources.LanguageManager
{
    public CustomLanguageManager() 
    {
        AddTranslation("en", "InclusiveBetweenValidator", "'{PropertyName}' must be between {From} and {To}. Received {PropertyValue}.");
        AddTranslation("en-US", "InclusiveBetweenValidator", "'{PropertyName}' must be between {From} and {To}. Received {PropertyValue}.");
        AddTranslation("en-GB", "InclusiveBetweenValidator", "'{PropertyName}' must be between {From} and {To}. Received {PropertyValue}.");
    }
}
