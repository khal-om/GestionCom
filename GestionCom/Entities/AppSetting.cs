namespace GestionCom.Entities
{
    public class AppSetting : BaseEntity
    {
        public string Key { get; set; } = null!;
        public string Value { get; set; } = null!;
    }
}
