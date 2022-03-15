# TradeHelper
Hi everyone, welcome to my project. You will learn to how to coding a TradeHelper project in here. Let's go we start!


## Platform
We are using `Console Application (.NET Framework)` in this tutorial. But you can use to you wish any platform that's supports .dll library.


## StrategyBuilder
The static `StrategyBuilder` class manages to your strategy classes. This class has the following methods:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| Append | Type strategyClass | IProcessResult | Appends your strategy class to its own list
| Remove | Type strategyClass | IProcessResult | Removes your strategy class from its own list if exists
| StartStrategies |  | IProcessResult | Starts all the strategies into its own list
| StopStrategies |  | IProcessResult | Stops all the strategies into its own list

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
| Initialize |  | void | Runs first before all
| RunAlways |  | Task&lt;bool> | Runs everytime by synchron. Waits for own tasks to finish and repeats (Task&lt;bool> is not important)
| RunTriggered | IProcessResult Graphic | void | Runs when it is triggered via value of `RunTriggeredInterval` property. Does not wait for own tasks to finish for trigger

| Property | Type | Description
|--|--|--|
| RunTriggeredInterval | KlineInterval? | Contains kline period for triggers `RunTriggered` method
| RunAlwaysDelay | int | Contains delay as microsecond for runs every betweens two `RunAlways` methods
| Symbols | string[] | Contains symbol names for positions (you can give in any format you want to ("Eth", "eTH ", "ETHUSdt", "ETHUSDT", ... etc.))
| GMTForGraph | int | Contains GMT period for kline data (if you give 3 then result is GMT+3, either if you give -2 then result is GMT-2)
| Binance | IBinancePosition | Contains Binance object for your processes about Binance positions
| Test | ITestPosition | Contains Test object for your processes about Test exchange positions (this exchange is not real)

#### Example
<pre><code>public class MyStrategy : Strategy
{
    public override KlineInterval? RunTriggeredInterval { get; set; }       = KlineInterval.FifteenMinutes; // fifteen minutes kline interval
    public override int RunAlwaysDelay { get; set; }                        = 100; // 100 ms delay for RunAlways()
    public override string[] Symbols { get; set; }                          = new string[] { "BTCUSDT" }; // only BTCUSDT for positions or analyses
    public override int GMTForGraph { get; set; }                           = +3; // GMT+3 for graphic data
    public override IBinancePosition Binance { get; set; }
    public override ITestPosition Test { get; set; }

    public override void Initialize()
    {
        Console.WriteLine("Initialized MyStrategy!");
    }

    public async override Task&lt;bool> RunAlways()
    {
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
| Status | ProcessStatus | Contains knowledge for finish status belong to any method
| Message | string | Contains error message if it is exist
| Data | object | Contains body data from the finished a method (as be an IKlineResult object, either as be an decimal value, ... etc.)

## TradeHelpers
The static `TradeHelpers` class helps to your strategy tips. This class has the following methods:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| GetLotSizeFilterAsync | string symbol | (decimal)IProcessResult.Data | Returns the lot size filter result for given symbol (for "BTCUSDT" result is 0.001, this meaning minimum entry cost is 0.001 for BTCUSDT)
| GetQuotes | List&lt;IBinanceKline> allKlines | List&lt;Quote> | Returns the converted kline data for indicators
| PercentChange | decimal firstPrice, decimal lastPrice | (decimal)IProcessResult.Data | Returns the difference as percentage for given betweens two prices (result is returning between -infinite to +infinite, not between -1 to +1)

#### Example
<pre><code>IProcessResult result = TradeHelpers.PercentChange(35000m, 39000m);
decimal percentChange = (decimal)result.Data;
// value of percentChange is 11.42857142857143
</code></pre>

## GraphicProcessor
The static `GraphicProcessor` class provides api for graphical data to your strategy. This class has the following methods:<br>

| Method | Parameters | Returns | Description
|--|--|--|--|
| GetAllSymbolsAsync |  | (List&lt;string>)IProcessResult.Data | Returns the symbol list on the Binance
| GetAssetFromUSDTAsync | string asset, string amountUSDT | (decimal)IProcessResult.Data | Returns the converted price data (USDT amount to Asset amount)
| GetCurrentPriceAsync | string symbol | (decimal)IProcessResult.Data | Returns the instant price belong to given symbol on the Binance
| GetKlinesAsync | string[] symbols, KlineInterval interval, [int gmt = 0] | (List&lt;IKlineResult>)IProcessResult.Data | Returns the historical candle data for given symbols and interval
| GetKlinesAsync | KlineInterval interval, [int gmt = 0] | (List&lt;IKlineResult>)IProcessResult.Data | Returns the historical candle data for given interval
| GetUSDTFromAssetAsync | string asset, string amountAsset | (decimal)IProcessResult.Data | Returns the converted price data (Asset amount to USDT amount)

#### Example
<pre><code>IProcessResult result = await GraphicProcessor.GetUSDTFromAssetAsync("BTC", 2);
decimal usdtAmount = (decimal)result.Data;
// value of usdtAmount is 80000 for now (if 1 BTC equals 40000 USDT)
</code></pre>
