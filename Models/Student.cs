using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwimmingSchool_Implementation.Models
{
    public class Student
    {
        public int Id { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in your First Name")]
        [MaxLength(30, ErrorMessage = "Second name can't be less then 2 and more then 30 characters"), MinLength(2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z]*$", ErrorMessage = "Use valid symbols in your Second Name")]
        [MaxLength(40, ErrorMessage = "Second name can't be less then 2 and more then 40 characters"), MinLength(2)]
        [Display(Name = "Second Name")]
        public string SecondName { get; set; }

        [Required]
        public string Gender { get; set; }

        [Required(ErrorMessage = "Please enter valid Date of Birth.")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateOfBirth { get; set; }

        public string MedicalConditions { get; set; }

        public string Allergies { get; set; }

        public string Medications { get; set; }

        //[Required]
        public string AquaticGoals { get; set; }

        public string SwimExperience { get; set; }

        //// --- Foreign Key & Navigation properties ---

        public virtual ICollection<Booking> Bookings { get; set; }
    }
}