namespace PinPinServer.Models.DTO
{
    public class WishlistDetailDTO
    {
        public int Id { get; set; }
        public int WishlistId { get; set; }
        public decimal? LocationLng { get; set; }
        public decimal? LocationLat { get; set; }
        public string GooglePlaceId { get; set; }
        public string Name { get; set; }
        public int? LocationCategoryId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
