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
