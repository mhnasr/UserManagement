﻿namespace UserManagement.Areas.Account.Models.ViewModels
{
    public class ChangePasswordWithSmsViewModel
    {
        public string PhoneNumber { get; set; }
        public string VerificationCode { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

}
