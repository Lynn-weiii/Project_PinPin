using System.ComponentModel.DataAnnotations;

namespace PinPinServer.Models.DTO
{
    public class CostCategoryDTO
    {
        public int Id { get; set; }

        [RegularExpression("^[A-Z]{3}$", ErrorMessage = "Invalid code format.")]
        public string? Code { get; set; }

        public string? Name { get; set; }
    }
}
