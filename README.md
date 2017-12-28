# tradebot
Bot trading coins

## Commands
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

## Get helps
```
docker run --it testbot trumhemcut/tradebot -h
```

**Be carefull in release mode!!!**

* Ăn cơm với muối mà nói chuyện trên núi
* Dân chơi mà, bi nhiêu bi, còn nhiêu ghi (nợ)

All rights reserved by @trumhemcut.
