namespace MODELS.ProcureAccess.Entities.Mapping;

public class EnumToLanguageConverter : IValueConverter<Language, string>
{
    public string Convert(Language sourceMember, ResolutionContext context)
    {
        return sourceMember switch
        {
            Language.English => Languages.En,
            Language.German => Languages.De,
            Language.Spanish => Languages.Es,
            Language.French => Languages.Fr,
            _ => Languages.De
        };
    }
}
