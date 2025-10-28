namespace Traveler.Wasm.Client.Models
{
    public class BridgeWebsTournament
    {
        public string EventId { get; set; } = string.Empty;
        public string ClubId { get; set; } = string.Empty;
        public string ClubName { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public bool HasResults { get; set; }
        
        public string GetPbnUrl()
        {
            // Format: event should be like "20251027_1" (date_session)
            return $"https://www.bridgewebs.com/cgi-bin/bwor/bw.cgi?pid=display_hands&msec=1&event={EventId}&wd=1&club={ClubId}&deal_format=pbn";
        }

        public string GetEventUrl()
        {
            // URL to view the event rankings on BridgeWebs
            return $"https://www.bridgewebs.com/cgi-bin/bwor/bw.cgi?pid=display_rank&event={EventId}&club={ClubId}";
        }
    }
}