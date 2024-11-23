using System.ComponentModel.DataAnnotations;

namespace UserManagement.Areas.Admin.Models
{
    public class RoleEditViewModel
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "نام نقش الزامی است.")]
        [Display(Name = "نام نقش")]
        public string Name { get; set; }
    }

}
