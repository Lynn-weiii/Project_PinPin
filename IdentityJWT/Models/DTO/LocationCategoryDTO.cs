using System.ComponentModel.DataAnnotations;

namespace PinPinServer.Models.DTO
{
    public class LocationCategoryDTO
    {
        public int Id { get; set; }
        public int WishlistId { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }
    }
}
