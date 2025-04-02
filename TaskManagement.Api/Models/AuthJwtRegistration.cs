using System.ComponentModel.DataAnnotations;

namespace TaskManagement.Api.Models
{
    public class AuthJwtRegistration
    {
        public string User { get; set; }
        public string Password { get; set; }
    }
}