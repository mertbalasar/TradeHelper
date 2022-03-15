# TradeHelper
Hi everyone, welcome to my project. You will learn to how to coding a TradeHelper project in here. Let's go we start!

## Contents
[1 - Platform](#platform)<br>
[2 - StrategyBuilder](#strategybuilder)<br>
[3 - Strategy](#strategy)<br>
[4 - IProcessResult](#iprocessresult)<br>
[5 - TradeHelpers](#tradehelpers)<br>
[6 - GraphicProcessor](#graphicprocessor)<br>
[7 - IBinancePosition](#ibinanceposition)<br>
[8 - Example Strategy](#example-strategy)<br>

## Platform
We are using `Console Application (.NET Framework)` in this tutorial. But you can use to you wish any platform that's supports .dll library.


## StrategyBuilder
The static `StrategyBuilder` class manages to your strategy classes. This class has the following methods:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| Append | `Type` strategyClass | `IProcessResult` | Appends your strategy class to its own list
| Remove | `Type` strategyClass | `IProcessResult` | Removes your strategy class from its own list if exists
| StartStrategies |  | `IProcessResult` | Starts all the strategies into its own list
| StopStrategies |  | `IProcessResult` | Stops all the strategies into its own list

#### Example
<pre><code>// NOTE: MyStrategy is a your class that is inherited by Strategy class

IProcessResult resultAppend = StrategyBuilder.Append(typeof(MyStrategy));
if (resultAppend.Status == ProcessStatus.Success)
{
    Console.WriteLine("Appends Succeed");
}

IProcessResult resultStart = StrategyBuilder.StartStrategies();
if (resultStart.Status == ProcessStatus.Success)
{
    Console.WriteLine("Starts Succeed");
}

IProcessResult resultStop = StrategyBuilder.StopStrategies();
if (resultStop.Status == ProcessStatus.Success)
{
    Console.WriteLine("Stops Succeed");
}

IProcessResult resultRemove = StrategyBuilder.Remove(typeof(MyStrategy));
if (resultRemove.Status == ProcessStatus.Success)
{
    Console.WriteLine("Removes Succeed");
}
</code></pre>

## Strategy
The abstract `Strategy` class helps to create to your strategy. Have to override to following methods and properties:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| Initialize |  | `Task<bool>` | Runs first before all (Task&lt;bool> is not important)
| RunAlways |  | `Task<bool>` | Runs everytime by synchron. Waits for own tasks to finish and repeats (Task&lt;bool> is not important)
| RunTriggered | `IProcessResult` Graphic | `void` | Runs when it is triggered via value of `RunTriggeredInterval` property. Does not wait for own tasks to finish for trigger

| Property | Type | Description
|--|--|--|
| RunTriggeredInterval | `KlineInterval?` | Contains kline period for triggers `RunTriggered` method
| RunAlwaysDelay | `int` | Contains delay as microsecond for runs every betweens two `RunAlways` methods
| Symbols | `string[]` | Contains symbol names for positions (you can give in any format you want to ("Eth", "eTH ", "ETHUSdt", "ETHUSDT", ... etc.))
| GMTForGraph | `int` | Contains GMT period for kline data (if you give 3 then result is GMT+3, either if you give -2 then result is GMT-2)
| Binance | `IBinancePosition` | Contains Binance object for your processes about Binance positions
| Test | `ITestPosition` | Contains Test object for your processes about Test exchange positions (this exchange is not real)

#### Example
<pre><code>public class MyStrategy : Strategy
{
    public override KlineInterval? RunTriggeredInterval { get; set; }       = KlineInterval.FifteenMinutes; // fifteen minutes kline interval
    public override int RunAlwaysDelay { get; set; }                        = 100; // 100 ms delay for RunAlways()
    public override string[] Symbols { get; set; }                          = new string[] { "BTCUSDT" }; // only BTCUSDT for positions or analyses
    public override int GMTForGraph { get; set; }                           = +3; // GMT+3 for graphic data
    public override IBinancePosition Binance { get; set; }
    public override ITestPosition Test { get; set; }

    public async override Task&lt;bool> Initialize()
    {
        Console.WriteLine("Initialized MyStrategy!");

        await Task.Delay(0);
        return true;
    }

    public async override Task&lt;bool> RunAlways()
    {
        Console.WriteLine("Triggered Run Always, And Will Be Trigger After 100 Milisecond");

        await Task.Delay(0);
        return true;
    }

    public override void RunTriggered(IProcessResult Graphic)
    {
        List&lt;IKlineResult> result = (List&lt;IKlineResult>)Graphic.Data; // contains BTCUSDT graphic data (with fifteen minutes interval) and indicators as an element of List
    }
}
</code></pre>

## IProcessResult
The interface `IProcessResult` provides to results after any calling method. This interface has the following fields:<br>

| Property | Type | Description
|--|--|--|
| Status | `ProcessStatus` | Contains knowledge for finish status belong to any method
| Message | `string` | Contains error message if it is exist
| Data | `object` | Contains body data from the finished a method (as be an IKlineResult object, either as be an decimal value, ... etc.)

## TradeHelpers
The static `TradeHelpers` class helps to your strategy tips. This class has the following methods:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| GetLotSizeFilterAsync | `string` symbol | `(decimal)IProcessResult.Data` | Returns the lot size filter result for given symbol (for "BTCUSDT" result is 0.001, this meaning minimum entry cost is 0.001 for BTCUSDT)
| GetQuotes | `List<IBinanceKline>` allKlines | `List<Quote>` | Returns the converted kline data for indicators
| PercentChange | `decimal` firstPrice, `decimal` lastPrice | `(decimal)IProcessResult.Data` | Returns the difference as percentage for given betweens two prices (result is returning between -infinite to +infinite, not between -1 to +1)

#### Example
<pre><code>IProcessResult result = TradeHelpers.PercentChange(35000m, 39000m);
decimal percentChange = (decimal)result.Data;
// value of percentChange is 11.42857142857143
</code></pre>

## GraphicProcessor
The static `GraphicProcessor` class provides api for graphical data to your strategy. This class has the following methods:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| GetAllSymbolsAsync |  | `(List<string>)IProcessResult.Data` | Returns the symbol list on the Binance
| GetAssetFromUSDTAsync | `string` asset, `string` amountUSDT | `(decimal)IProcessResult.Data` | Returns the converted price data (USDT amount to Asset amount)
| GetCurrentPriceAsync | `string` symbol | `(decimal)IProcessResult.Data` | Returns the instant price belong to given symbol on the Binance
| GetKlinesAsync | `string[]` symbols, `KlineInterval` interval, [`int` gmt = 0] | `(List<IKlineResult>)IProcessResult.Data` | Returns the historical candle data for given symbols and interval
| GetKlinesAsync | `KlineInterval` interval, [`int` gmt = 0] | `(List<IKlineResult>)IProcessResult.Data` | Returns the historical candle data for given interval
| GetUSDTFromAssetAsync | `string` asset, `string` amountAsset | `(decimal)IProcessResult.Data` | Returns the converted price data (Asset amount to USDT amount)

#### Example
<pre><code>IProcessResult result = await GraphicProcessor.GetUSDTFromAssetAsync("BTC", 2);
decimal usdtAmount = (decimal)result.Data;
// value of usdtAmount is 80000 for now (if 1 BTC equals 40000 USDT)
</code></pre>

## IBinancePosition
The interface `IBinancePosition` provides to operations about the Binance API. This interface has the following fields:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| AddCredential | `string` key, `string` secret | `IProcessResult` | Sets the given API Key and returns the finish flag result
| ClosePositionAsync | `IPositionResult` openedPosition | `(ITradeResult)IProcessResult.Data` | Closes the given opened position object and returns trade results
| GetBalanceAsync |  | `(decimal)IProcessResult.Data` | Returns the available USDT balance on the your Binance account
| GetPositionDataAsync | `string` symbol | `(IPositionResult)IProcessResult.Data` | Returns the opened position data as instantaneously (that is includes entry time, leverage, mark price, pnl, roe, etc.)
| OpenPositionAsync | `string` symbol, `decimal` costAmount, `int` leverage, `PositionType` positionType, [`FuturesMarginType` marginType = `FuturesMarginType.Isolated`] | `(IPositionResult)IProcessResult.Data` | Opens to position according to given parameters and returns position results (that is includes entry time, leverage, mark price, pnl, roe, etc.)

#### Example
<pre><code>IProcessResult result = await Binance.GetBalanceAsync();
if (result.Status == ProcessStatus.Fail)
{
    Console.WriteLine(result.Message);
    return;
}

decimal balance = (decimal)result.Data;
// the value of balance is will be your available USDT balance
</code></pre>

## ITestPosition
The interface `ITestPosition` provides to operations about the Test Exchange API (do not worry, that is just a fake exchange). This interface has the following fields:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| ClosePositionAsync | `IPositionResult` openedPosition | `(ITradeResult)IProcessResult.Data` | Closes the given opened position object and returns trade results
| GetPositionDataAsync | `IPositionResult` openedPosition | `(IPositionResult)IProcessResult.Data` | Returns the opened position data as instantaneously (that is includes entry time, leverage, mark price, pnl, roe, etc.)
| OpenPositionAsync | `string` symbol, `decimal` costAmount, `int` leverage, `PositionType` positionType | `(IPositionResult)IProcessResult.Data` | Opens to position according to given parameters and returns position results (that is includes entry time, leverage, mark price, pnl, roe, etc.)

#### Example
<pre><code>IProcessResult result = await Test.OpenPositionAsync("BTCUSDT", 1, 5, PositionType.Long);
if (result.Status == ProcessStatus.Fail)
{
    Console.WriteLine(result.Message);
    return;
}

IPositionResult positionResult = (IPositionResult)result.Data;
Console.WriteLine("You entered in: " + positionResult.EntryPrice.ToString());
</code></pre>

## Example Strategy
<pre><code>public class EmaStrategy : Strategy
{
    public override KlineInterval? RunTriggeredInterval { get; set; }       = KlineInterval.OneMinute;
    public override int RunAlwaysDelay { get; set; }                        = 60000;
    public override string[] Symbols { get; set; }                          = new string[] { "avAX", "ada ", "BTc", "ETHUSDT" };
    public override int GMTForGraph { get; set; }                           = +3;
    public override IBinancePosition Binance { get; set; }
    public override ITestPosition Test { get; set; }

    private List<IPositionResult> openedPositions;
    private IPositionResult openedPosition;
    private IProcessResult positionResult;
    private List<EmaResult> emaResult;
    private IProcessResult lotSizeResult;

    public async override Task<bool> Initialize()
    {
        openedPositions = new List<IPositionResult>();

        Console.WriteLine("Initialized EMA Strategy!");

        await Task.Delay(0);
        return true;
    }

    public async override Task<bool> RunAlways()
    {
        await Task.Delay(0);
        return true;
    }

    public async override void RunTriggered(IProcessResult Graphic)
    {
        Console.WriteLine("#### TRIGGERED RunTriggered");

        // if pulling graphical data result is fail then will be return (maybe disconnected from ethernet)
        if (Graphic.Status == ProcessStatus.Fail) return;

        // gets all graphical data (that is includes indicators and klines knowledges of the AVAX, ADA, BTC and ETH
        List<IKlineResult> allGraphicResult = (List<IKlineResult>)Graphic.Data;

        // builds a loop for look at the all graphical data
        foreach (IKlineResult graphicResult in allGraphicResult)
        {
            // searching into openedPositions to find to opened position belong to current symbol (its can be null)
            openedPosition = openedPositions.Where((position) => position.Symbol.Equals(graphicResult.Symbol)).FirstOrDefault();
            // gets EMA(50) results
            emaResult = graphicResult.Indicators.GetEma(50).ToList();
            // gets lot size filter belong to current symbol (for pass as amount parameter)
            lotSizeResult = await TradeHelpers.GetLotSizeFilterAsync(graphicResult.Symbol);

            // if pulling lot size data result is fail then will be continue from next symbol
            if (lotSizeResult.Status == ProcessStatus.Fail) continue;

            // if not exist opened position for current symbol
            if (openedPosition == null)
            {
                // if previous ema value is betweens previous candle low price and previous candle high price (so if its cross) 
                // and if current ema value is lower than current candle close price then opens long position
                if (
                    emaResult[emaResult.Count - 2].Ema != null && emaResult.Last().Ema != null &&
                    emaResult[emaResult.Count - 2].Ema >= graphicResult.Klines[graphicResult.Klines.Count - 2].LowPrice &&
                    emaResult[emaResult.Count - 2].Ema <= graphicResult.Klines[graphicResult.Klines.Count - 2].HighPrice &&
                    emaResult.Last().Ema <= graphicResult.Klines.Last().ClosePrice
                    )
                {
                    positionResult = await Test.OpenPositionAsync(graphicResult.Symbol, (decimal)lotSizeResult.Data, 5, PositionType.Long);
                    if (positionResult.Status == ProcessStatus.Success)
                    {
                        openedPositions.Add((IPositionResult)positionResult.Data);
                        Console.WriteLine("Opened Position For " + graphicResult.Symbol);
                    }
                }
            }
            // if exist opened position for current symbol
            else
            {
                // if previous ema value is betweens previous candle low price and previous candle high price (so if its cross) 
                // and if current ema value is upper than current candle close price then closes opened position and shows trade result (just for PNL)
                if (
                    emaResult[emaResult.Count - 2].Ema != null && emaResult.Last().Ema != null &&
                    emaResult[emaResult.Count - 2].Ema >= graphicResult.Klines[graphicResult.Klines.Count - 2].LowPrice &&
                    emaResult[emaResult.Count - 2].Ema <= graphicResult.Klines[graphicResult.Klines.Count - 2].HighPrice &&
                    emaResult.Last().Ema > graphicResult.Klines.Last().ClosePrice
                    )
                {
                    positionResult = await Test.ClosePositionAsync(openedPosition);
                    if (positionResult.Status == ProcessStatus.Success)
                    {
                        openedPositions.Remove(openedPosition);
                        ITradeResult tradeResult = (ITradeResult)positionResult.Data;
                        Console.WriteLine("Closed Position For " + graphicResult.Symbol + " With PNL: " + string.Format("{0:0.00}", tradeResult.PNL) + " USDT");
                    }
                }
            }
        }
    }
}
</code></pre>
