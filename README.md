# tradebot
Bot trading coins

## Docker Build
```
docker build -t trumhemcut/tradebot .
```
## Commands
**Get help**
```
docker run trumhemcut/tradebot --help
Usage:  [options]

Options:
  -?|-h|--help                                Show help information
  -c|--coin <COIN>                            Trade Coin, e.g. ADA | XVG
  -d|--delta <DELTA>                          Expected Delta, e.g. 0.00000010
  -auto|--isautotrading                       Auto Trader Mode On/Off
  -f|--tradeflow <BuyAtBinanceSellAtBittrex>  BuyAtBinanceSellAtBittrex | SellAtBinanceBuyAtBittrex | AutoSwitch
  -q|--quantity <QUANTITY>                    Quantity to trade
  -w|--win <PlusPointToWin>                   Plus Point To Win e.g. 0.00000003
  -t|--testmode                               Test Mode On/Off
```

**Buy at Binance, Sell at Bittrex**
```
docker run -d --name xvgbotr trumhemcut/tradebot \
  -c XVG \
  -d 0.00000012 \
  -auto \
  -f BuyAtBinanceSellAtBittrex \
  -q 1000 \
  -w 0.00000001
```

**Sell at Binance, Buy at Bittrex**
```
docker run -d --name xvgbot trumhemcut/tradebot \
  -c XVG \
  -d 0.00000012 \
  -auto \
  -f SellAtBinanceBuyAtBittrex \
  -q 1000 \
  -w 0.00000001
```


## Buy me a coffee

My ADA wallet address:
```
DdzFFzCqrhseGFdRvddihfs61xgvqgBEgyExYQH7h3De18hEfFW8nhpgRC4zCiuRApYgNBrk1LiCp4EGcaiiffHZY4L2xU5BAZtEp41n
```

**Be carefull in release mode!!!**

All rights reserved by @trumhemcut.
