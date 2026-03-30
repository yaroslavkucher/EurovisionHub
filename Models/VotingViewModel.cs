namespace EurovisionHub.Models
{
    public class VotingViewModel
    {
        public List<Event> Events { get; set; } = new List<Event>();
        public int? SelectedEventId { get; set; }
        public string SelectedEventName { get; set; }
        public List<Vote> Votes { get; set; } = new List<Vote>();
    }
}