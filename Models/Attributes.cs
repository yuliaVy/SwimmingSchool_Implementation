using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace SwimmingSchool_Implementation.Models
{
    //checking if the date of birth is valid
    public class ValidDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                // Ensures the date is strictly before today
                if (date.Date >= DateTime.Now.Date)
                {
                    return new ValidationResult("Date of Birth must be in the past.");
                }

                //ensures the user is at least 18 years old
                if (date.Date > DateTime.Now.AddYears(-18).Date)
                {
                    return new ValidationResult("You must be at least 18 years old.");
                }
            }
            return ValidationResult.Success;
        }
    }
}