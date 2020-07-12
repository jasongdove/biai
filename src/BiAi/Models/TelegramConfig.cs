namespace BiAi.Models
{
    public class TelegramConfig
    {
        private long[] _chatIds;
        public string Token { get; set; }

        public long[] ChatIds
        {
            // the config binder appears to use null rather than an empty array
            get => _chatIds ?? new long[0];
            set => _chatIds = value;
        }
    }
}