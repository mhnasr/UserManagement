using System.ComponentModel.DataAnnotations;

namespace UserManagement.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "نام الزامی است")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "نام خانوادگی الزامی است")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "شماره موبایل الزامی است")]
        [Phone(ErrorMessage = "شماره موبایل معتبر نیست")]
        [Display(Name = "شماره موبایل")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "کلمه عبور الزامی است")]
        [MinLength(6, ErrorMessage = "کلمه عبور باید حداقل ۶ کاراکتر باشد")]
        [Display(Name = "کلمه عبور")]
        public string Password { get; set; }
    }
}