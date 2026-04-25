namespace GestionCom.Entities
{
    public abstract class SoftDeletableEntity : BaseEntity
    {
        public bool IsDeleted { get; set; } = false;
    }
}
