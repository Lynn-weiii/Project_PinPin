using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace PinPinServer
{
    public class MinCountAttributes : ValidationAttribute
    {
        private int _minCount;

        /// <summary>
        /// 限制接收的集合最小長度為minCount
        /// </summary>
        /// <param name="minCount"></param>
        public MinCountAttributes(int minCount)
        {
            _minCount = minCount;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is ICollection collection && collection.Count >= _minCount)
            {
                return ValidationResult.Success;
            }
            return new ValidationResult($"The field {validationContext.DisplayName} must contain at least {_minCount} items.");
        }
    }
}
