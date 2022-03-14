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
| RunAlways |  | Task&lt;bool> | Runs everytime by synchron. Waits for own tasks to finish and repeats
| RunTriggered | IProcessResult Graphic | void | Runs when it is triggered via value of `RunTriggeredInterval` property. Does not wait for own tasks to finish for trigger

| Property | Type | Description
|--|--|--|
| RunTriggeredInterval | KlineInterval? | Contains kline period for triggers `RunTriggered` method
| RunAlwaysDelay | int | Contains delay as microsecond for runs every betweens two `RunAlways` methods
| Symbols | string[] | Contains symbol names for positions (you can give in any format you want to ("Eth", "eTH ", "ETHUSdt", "ETHUSDT", ... etc.))
| GMTForGraph | int | Contains GMT period for kline data (if you give 3 then result is GMT+3, either if you give -2 then result is GMT-2)
| Binance | IBinancePosition | Contains Binance object for your processes about Binance positions
| Test | ITestPosition | Contains Test object for your processes about Test exchange positions (this exchange is not real)
