namespace GameAggregator
{
    public class ResponseId
    {
        public string Steamid { get; set; }
        public int Success { get; set; }
        public string Message { get; set; }
    }

    public class Steam_GetId
    {
        public ResponseId Response { get; set; }
    }
}
