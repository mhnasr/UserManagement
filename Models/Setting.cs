using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class Setting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "نحوه ورود به سایت")]
        public string LoginMethod { get; set; } // UserAndPassword یا MobileCode
    }
}