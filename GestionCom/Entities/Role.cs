namespace GestionCom.Entities
{
    public class Role : SoftDeletableEntity
    {
        public string Name { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
