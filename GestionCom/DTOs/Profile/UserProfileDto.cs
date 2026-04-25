namespace GestionCom.DTOs.Profile
{
    public class UserProfileDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = null!;
        public string? Email { get; set; }
        public string RoleName { get; set; } = null!;
    }
}
