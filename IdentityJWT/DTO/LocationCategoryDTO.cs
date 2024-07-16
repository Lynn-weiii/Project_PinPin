using System.ComponentModel.DataAnnotations;

namespace PinPinServer.DTO
{
    public class LocationCategoryDTO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Invalid color format.")]
        public string? Color { get; set; }
    }
}
