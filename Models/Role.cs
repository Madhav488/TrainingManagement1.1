

namespace Tms.Api.Models
{

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = null!;

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}