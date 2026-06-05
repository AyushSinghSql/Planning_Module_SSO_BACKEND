namespace WebApi.Models
{
    public class NonWorkingDay
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public bool IsPublicHoliday { get; set; }
        public string State { get; set; }
        public string Remarks { get; set; }
    }
}
