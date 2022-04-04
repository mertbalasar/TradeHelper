# TradeHelper

TradeHelper is a project that helps about trade strategy. You firstly create a class that's inherited by our `Strategy` class. Then write it! Finally add your strategy class into `StrategyBuilder` and run it!

## Benefits

* Flexibility (no need knowledge of Binance API or Tradingview API or helper methods (convertion usdt <-> asset or calculate price change etc.))
* Accessible (access to TA indicators easily)
* Supports 80+ TA Indicators
* Everythings Are Asynchron
* Provides A Report (includes historical positions and strategy metrics as `.json` file)

## Disadvantages

* Does Not Support A Server (supports on the local machine yet now)

## Tutorial

Please look at the `Tutorial/TUTORIAL.md` file for details.

## Tanks For

* https://github.com/JKorf/Binance.Net (For Supports Binance API)
* https://daveskender.github.io/Stock.Indicators/ (For Supports TA Indicators)

## Releases

* v1.0.0
- Created project
* v1.1.0
- Fixed and refactored `TradeResult` for order partition result bug
- Fixed GMT bug for `Binance` into reports
- Fixed TotalFee bug for `Binance` into reports
- Fixed schedules bugs for `Strategy` class methods
- Added `SetTakeProfitAsync`, `SetStopLossAsync`, `OpenOrderAsync` instead of `OpenPositionAsync`, `CancelOrderAsync`, `GetOrderLocationAsync`, `GetPositionDataAsync` for position data, `GetTradeDataAsync` for adding reports into `Binance`
- Added `GetConnectionStatusAsync`, `PriceChange`, `FilterPriceByPrecisionAsync`, `FilterAmountByPrecisionAsync` instead of `GetLotSizeFilterAsync`, `GetLotSizeAmountAsync` into `TradeHelpers`
- Added `INotificationProcessor` into `Strategy` class for e-mail notifications
- Added `IReportProcessor` into `Strategy` class for report processes user control based
- Added `StrategySettings` into `Strategy` that contains `RunTriggeredInterval`, `RunAlwaysDelay`, `Symbols`, `GMTForGraph` and `MailList`
- Added `StrategyTools` into `Strategy` that contains `Binance`, `Test`, `Notification` and `Report`
- Changed `IPositionResult`, `ITradeResult` and added `IOrderResult` new
* v1.2.0
- Changed `IProcessResult` struct with `IProcessResult<T>`
