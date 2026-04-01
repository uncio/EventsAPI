using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI
{
    /// <summary>
    /// Custom validation attribute to compare timeline order of two DateTime properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        /// <summary>
        /// Attribute constructor
        /// </summary>
        /// <param name="dateToCompareToFieldName">Name of second DateTime attribute co compare</param>
        public DateGreaterThanAttribute(string dateToCompareToFieldName)
        {
            DateToCompareToFieldName = dateToCompareToFieldName;
        }

        private string DateToCompareToFieldName { get; set; }

        /// <summary>
        /// Method to compare timeline order of two DateTime properties
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult($"Date cann't be null");
            }
            DateTime earlierDate = (DateTime)value;

            var lDate = validationContext?.ObjectType.GetProperty(DateToCompareToFieldName)?.GetValue(validationContext.ObjectInstance, null);
            if (lDate == null)
            {
                return new ValidationResult($"Date {DateToCompareToFieldName} cann't be null");
            }
            DateTime laterDate = (DateTime)lDate;


            if (laterDate.IsStrictlyGreaterThan(earlierDate))
            {
                return ValidationResult.Success;
            }
            else
            {
                return new ValidationResult($"Date is not later {DateToCompareToFieldName}");
            }
        }
    }
}
