using System;
using System.ComponentModel.DataAnnotations;

namespace EWP_API_WEB_APP.Utilities.Validations
{
    public class FutureDateValidation : ValidationAttribute
    {
        private readonly string _datePropertyName;

        public FutureDateValidation(string datePropertyName)
        {
            _datePropertyName = datePropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var dateProperty = validationContext.ObjectType.GetProperty(_datePropertyName);

            var dateValue = (DateTime)dateProperty.GetValue(validationContext.ObjectInstance);

            if (dateValue < DateTime.Now)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
