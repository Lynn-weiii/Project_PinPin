namespace PinPinServer.Models.DTO
{
    public class ScheduleDetailDTO
    {


        public int? Sort { get; set; }

        public int? Id { get; set; }

        public List<TransportationDTO>? Transportations { get; set; }

        public int ScheduleDayId { get; set; }

        public string LocationName { get; set; }

        public string Location { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public decimal? Lat { get; set; }

        public decimal? Lng { get; set; }

        //transportation 
        public int ScheduleDetailsId { get; set; }

        public int TransportationCategoryId { get; set; }

        public DateTime? Time { get; set; }

        public int? CurrencyId { get; set; }

        public decimal? Cost { get; set; }

        public string? Remark { get; set; }

        public string? TicketImageUrl { get; set; }

        public int TransportationId { get; set; }

    }
}
