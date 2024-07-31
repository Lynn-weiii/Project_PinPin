using PinPinServer.Models;

namespace PinPinServer.DTO
{
    public class WishlistDTO
    {
        public Wishlist Wishlists { get; set; }
        public IEnumerable<LocationCategory> LocationCategories { get; set; }
    }
}
