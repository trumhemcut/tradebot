namespace tradebot.core
{
    public enum TradeFlow
    {
        BuyAtBinanceSellAtBittrex,
        SellAtBinanceBuyAtBittrex,
        // If Price at binance is higher, then will be SellAtBinanceBuyAtBittrex
        // Otherwise, will be BuyAtBinanceSellAtBittrex
        AutoSwitch
    } 
}