

using System.ComponentModel.DataAnnotations;

namespace CraneFileManager.Application.Validations
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class AgeRestrictionAttribute : ValidationAttribute
    {
        private readonly int _minimumAge;

        public AgeRestrictionAttribute(int minimumAge)
        {
            _minimumAge = minimumAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime birthDate)
            {
                var today = DateTime.Today;
                var age = today.Year - birthDate.Year;
                if (birthDate.Date > today.AddYears(-age)) age--;

                if (age < _minimumAge)
                {
                    throw new ValidationException(GetErrorMessage());
                }
            }
            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return $"You must be at least {_minimumAge} years old.";
        }
    }
}
