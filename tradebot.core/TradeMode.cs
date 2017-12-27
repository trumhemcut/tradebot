namespace tradebot.core
{
    public enum TradeMode
    {
        FixedMode,
        NormalTrade, 
        // Finegrained mode:
        // Buy/sell follow the qty of current bid / ask quantity, 
        // not following the max budget we have
        FinegrainedTrade
    }
}