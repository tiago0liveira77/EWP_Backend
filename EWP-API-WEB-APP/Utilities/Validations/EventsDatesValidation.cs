using System.ComponentModel.DataAnnotations;

namespace EWP_API_WEB_APP.Utilities.Validations
{
    public class EventsDatesValidation : ValidationAttribute
    { 

        private readonly string _startDatePropertyName;
        private readonly string _endDatePropertyName;

        public EventsDatesValidation(string startDatePropertyName, string endDatePropertyName)
        {
            _startDatePropertyName = startDatePropertyName;
            _endDatePropertyName = endDatePropertyName;
        }

        /// <summary>
        /// Validação customizada para as datas serem coerentes (Data de inicio de evento "<" data de fim de evento)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns> Resultado da validação, sucesso ou insucesso (com mensagem de erro) </returns>
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var startDateProperty = validationContext.ObjectType.GetProperty(_startDatePropertyName);
            var endDateProperty = validationContext.ObjectType.GetProperty(_endDatePropertyName);

            var startDateValue = (DateTime)startDateProperty.GetValue(validationContext.ObjectInstance);
            var endDateValue = (DateTime)endDateProperty.GetValue(validationContext.ObjectInstance);

            if (startDateValue > endDateValue)
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}
