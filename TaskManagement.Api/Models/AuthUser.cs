using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Models
{
    public class AuthUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}