# tradebot
Bot trading coins

## Docker Build
```
docker build -t trumhemcut/tradebot .
```

## Get help
```docker run trumhemcut/tradebot --help```

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

## Configure for production environment
Please email me at trumhemcut@hotmail.com, I'll charge 80ADA/h or 2000ADA (Cardano) for one bot configuration.

My ADA wallet address:
```
DdzFFzCqrhseGFdRvddihfs61xgvqgBEgyExYQH7h3De18hEfFW8nhpgRC4zCiuRApYgNBrk1LiCp4EGcaiiffHZY4L2xU5BAZtEp41n
```

**Be carefull in release mode!!!**

* Ăn cơm với muối mà nói chuyện trên núi
* Dân chơi mà, bi nhiêu bi, còn nhiêu ghi (nợ)

All rights reserved by @trumhemcut.
