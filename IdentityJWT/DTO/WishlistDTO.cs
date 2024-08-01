using PinPinServer.Models;

namespace PinPinServer.DTO
{
    public class WishlistDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; }
        public IEnumerable<LocationCategoryDTO> LocationCategories { get; set; }
    }
}
