namespace PinPinServer.Models.DTO
{
    public class WishlistDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string Name { get; set; }
        public IEnumerable<LocationCategoryDTO> LocationCategories { get; set; }
        public List<WishlistDetailDTO> WishlistDetails { get; set; } // 加入這一行
    }
}
