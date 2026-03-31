using System.ComponentModel.DataAnnotations;

namespace RU.Uncio.EventsAPI
{
    [AttributeUsage(AttributeTargets.Property)]
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        public DateGreaterThanAttribute(string dateToCompareToFieldName)
        {
            DateToCompareToFieldName = dateToCompareToFieldName;
        }

        private string DateToCompareToFieldName { get; set; }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            DateTime earlierDate = (DateTime)value;

            DateTime laterDate = (DateTime)validationContext.ObjectType.GetProperty(DateToCompareToFieldName).GetValue(validationContext.ObjectInstance, null);

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
