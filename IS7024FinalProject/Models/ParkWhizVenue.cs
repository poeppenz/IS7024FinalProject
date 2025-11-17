namespace IS7024FinalProject.Models
{
    /// <summary>
    /// Represents a venue retrieved from the ParkWhiz API.
    /// </summary>
    public class ParkWhizVenue
    {
        public int VenueId { get; set; }
        public string VenueName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public int Capacity { get; set; }
    }
}
