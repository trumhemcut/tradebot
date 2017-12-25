namespace tradebot.core
{
    public class TradeBotApiResult
    {
        public bool Success { get; set; }
        // TODO: Should be generic here in the future
        // public T Result { get; set; }
        public string Message { get; set; }
    }
}