using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SwimmingSchool_Implementation.Models
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class ExternalLoginListViewModel
    {
        public string ReturnUrl { get; set; }
    }

    public class SendCodeViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
        public string ReturnUrl { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyCodeViewModel
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        [Display(Name = "Code")]
        public string Code { get; set; }
        public string ReturnUrl { get; set; }

        [Display(Name = "Remember this browser?")]
        public bool RememberBrowser { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class LoginViewModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class RegisterViewModel
    {
        [Required(ErrorMessage = "You must provide an emailr")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must provide your First Name")]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in your First Name")]
        [MaxLength(30, ErrorMessage = "Your First name  can't be less then 2 and more then 30 characters"), MinLength(2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in your Second Name")]
        [MaxLength(40, ErrorMessage = "Your Second name can't be less then 2 and more then 40 characters"), MinLength(2)]
        [Display(Name = "Second Name")]
        public string SecondName { get; set; }

        //[Required(ErrorMessage = "You must provide a mobile phone number")]
        [Display(Name = "Mobile Number")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{4})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid mobile number")]
        public virtual string PhoneNumber { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }


        public List<StudentViewModel> Students { get; set; }

        public int SelectedLessonId{ get; set; }

        // True if they check the 20% deposit box, False if they want to pay in full
        public bool PayDepositOnly { get; set; }

        // Will hold either "Stripe" or "PayPal" based on which button they click
        public string PaymentMethod { get; set; }

        public List<int> AcceptedPolicies { get; set; }

        public string Comments { get; set; }
    }

    public class StudentViewModel
    {
        public bool IsAccountHolder { get; set; }

        public List<int> AcceptedPolicyIds { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in student's First Name")]
        [MaxLength(30, ErrorMessage = "Student's first name can't be less then 2 and more then 30 characters"), MinLength(2)]
        [Display(Name = "Student's First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in student's Second Name")]
        [MaxLength(40, ErrorMessage = "Student's econd name can't be less then 2 and more then 40 characters"), MinLength(2)]
        [Display(Name = "Student's Second Name")]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime BirthDate { get; set; }

        // Specific to this student's booking
        [Required(ErrorMessage = "You must select a lesson for this student.")]
        public int SelectedLessonId { get; set; }
        public string SelectedLessonTitle { get; set; }
        public decimal LessonPrice { get; set; }

        [Required(ErrorMessage = "You must accept the policies.")]
        public List<int> AcceptedPolicies { get; set; } = new List<int>();

        public string MedicalConditions { get; set; }

        public string Allergies { get; set; }

        public string Medications { get; set; }

        public string ImmunizationNotes { get; set; }

        public string AquaticGoals { get; set; }

        public string SwimExperience { get; set; }
    }

    public class BookingSuccessViewModel
    {
        public string AccountHolderName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public string PaymentMethod { get; set; }
        public decimal TotalPaidToday { get; set; }
        public string PaymentType { get; set; } // "20% Deposit" or "Paid in Full"
        public DateTime TransactionDate { get; set; }

        public decimal OutstandingBalance { get; set; }
        public List<SuccessStudentDetail> Students { get; set; } = new List<SuccessStudentDetail>();
    }

    public class SuccessStudentDetail
    {
        public string StudentName { get; set; }
        public string ClassName { get; set; }
        public string DayOfWeek { get; set; }
        public string LessonTime { get; set; }
        public string Venue { get; set; }
        public decimal ClassPrice { get; set; }
        public List<DateTime> SessionDates { get; set; } = new List<DateTime>();
    }


    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Code { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
