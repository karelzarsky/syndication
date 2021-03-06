﻿using Mongo.Common;
using MongoDB.Driver;
using System;
using System.Globalization;
using System.Linq;

namespace Mongo.Sim
{
    class Program
    {
        const double minPTPct = 1;
        const double maxPTPct = 20;
        const double stepPTPct = 1;
        const double minSLPct = 0.1;
        const double minSLValue = 0.02;
        const double maxSLPct = 2;
        const double stepSLPct = 0.1;
        const double usualCommissions = 1;
        const double maxRiskValue = 100;
        const double maxMargin = 5000;
        const double usualSlippage = 0.01;
        const double minimalUpPercent = 0;
        const double minGapDown = -0.5;
        private const string resultCollectionName = "TradesThroughZeroMinus05Plus0";

        static IMongoCollection<Trade> tradeCollection;

        static void Main(string[] args)
        {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var mongoUri = "mongodb://localhost:27017/?readPreference=primary&appname=MongoDB&ssl=false";
            var mongoClient = new MongoClient(mongoUri);
            var seerDatabase = mongoClient.GetDatabase("seer");
            IMongoCollection<EarningEvent> releaseCollection = seerDatabase.GetCollection<EarningEvent>("EarningsFromYahoo");
            tradeCollection = seerDatabase.GetCollection<Trade>(resultCollectionName);

            //var filterDefinition = Builders<EarningEvent>.Filter.Eq(x => x.Error, "Error processing request.-'bT' : cause - Duplicate ticker ID for API historical data query");
            //var updateDefinition = Builders<EarningEvent>.Update.Unset(x => x.Error);
            //var res = releaseCollection.UpdateMany(filterDefinition, updateDefinition);

            var ids = releaseCollection.Find(x => x.Candles != null && x.Candles.Count > 1200).Project(x => x.Id).ToList();
            foreach (var id in ids)
            {
                try
                {
                    var r = releaseCollection.Find(x => x.Id == id).FirstOrDefault();
                    GenerateTradesThroughZero(r);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
        }

        private static void GenerateTradesThroughZero(EarningEvent release)
        {
            Candle closingCandle;
            DateTime realReleaseTime = findRealReleaseTime(release, out closingCandle);
            if (closingCandle == null) return;
            double gap = calculateGap(release, realReleaseTime);
            if (gap > minGapDown) return;
            Candle startCandle = FindEntryThroughZero(release, realReleaseTime, closingCandle);
            if (startCandle == null) return; // Entry signal not triggered
            if (startCandle.Open < 1) return; // Price too low. I don't want to trade any penny stocks.

            for (double ptPct = minPTPct; ptPct <= maxPTPct; ptPct += stepPTPct)
            {
                if (ptPct == 0) continue;
                bool directionLong = ptPct > 0;
                double startPrice = directionLong
                    ? closingCandle.Close * (1 + minimalUpPercent / 100)
                    : closingCandle.Close * (1 - minimalUpPercent / 100);
                double ptValue = startPrice * (1 + (ptPct / 100));
                for (double slPct = minSLPct; slPct <= maxSLPct; slPct += stepSLPct)
                {
                    double slValue = directionLong ? startPrice * (1 - (slPct / 100)) : startPrice * (1 + (slPct / 100));
                    if (Math.Abs(slValue - startPrice) < minSLValue)
                        slValue = directionLong ? startPrice - minSLValue : startPrice + minSLValue;
                    //Console.WriteLine($"PT%:{ptPct} SL%:{slPct} start:{startPrice} PT:{ptValue} SL:{slValue}");

                    var trade = new Trade
                    {
                        Ticker = release.Symbol,
                        StartTime = startCandle.Time,
                        DirectionLong = directionLong,
                        StartPrice = startPrice,
                        TargetValue = ptValue,
                        TargerPct = ptPct,
                        StopLossValue = slValue,
                        StopLossPct = slPct,
                        Commissions = usualCommissions,
                        GapPercent = gap,
                        YahooId = release.Id
                    };
                    SetQuantity(trade);
                    //Console.WriteLine($"PT%:{ptPct} SL%:{slPct} price:{trade.StartPrice} shares:{trade.Shares} risk:{trade.RiskValue} reward:{trade.RewardValue} RRR:{trade.RRR}");
                    SimulateTrade(release, trade);
                    Console.ForegroundColor = trade.RealizedProfitPct > 0 ? ConsoleColor.White : ConsoleColor.Red;
                    Console.WriteLine($"{trade.EndReason} PT%:{ptPct:F0} SL%:{slPct:F1} price:{trade.StartPrice:F2} cl.price:{trade.ClosePrice:F2} end:{trade.EndTime} real%:{trade.RealizedProfitPct:F2} real:{trade.RealizedProfitValue:F2}");
                    tradeCollection.InsertOne(trade);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        private static Candle FindEntryThroughZero(EarningEvent release, DateTime realReleaseTime, Candle closingCandle)
        {
            foreach (var c in release.Candles)
            {
                if (c.Time < realReleaseTime) continue;
                if (c.High > closingCandle.Close * (1 + minimalUpPercent/100)) return c;
            }
            return null;
        }

        private static void GenerateTradesFirstCandle(EarningEvent release)
        {
            Candle closingCandle;
            DateTime realReleaseTime = findRealReleaseTime(release, out closingCandle);
            double gap = calculateGap(release, realReleaseTime);
            Candle startCandle = FindStartCandle(release, realReleaseTime);
            if (startCandle.Open < 1)
                throw new Exception("Price too low.");


            for (double ptPct = minPTPct; ptPct <= maxPTPct; ptPct += stepPTPct)
            {
                if (ptPct == 0) continue;
                bool directionLong = ptPct > 0;
                double startPrice = directionLong
                    ? startCandle.High
                    : startCandle.Low;
                double ptValue = startPrice * (1 + (ptPct / 100));
                for (double slPct = minSLPct; slPct <= maxSLPct; slPct += stepSLPct)
                {
                    double slValue = directionLong ? startPrice * (1 - (slPct / 100)) : startPrice * (1 + (slPct / 100));
                    //Console.WriteLine($"PT%:{ptPct} SL%:{slPct} start:{startPrice} PT:{ptValue} SL:{slValue}");

                    var trade = new Trade
                    {
                        Ticker = release.Symbol,
                        StartTime = startCandle.Time,
                        DirectionLong = directionLong,
                        StartPrice = startPrice,
                        TargetValue = ptValue,
                        TargerPct = ptPct,
                        StopLossValue = slValue,
                        StopLossPct = slPct,
                        Commissions = usualCommissions,
                        GapPercent = gap,
                        YahooId = release.Id
                    };
                    SetQuantity(trade);
                    //Console.WriteLine($"PT%:{ptPct} SL%:{slPct} price:{trade.StartPrice} shares:{trade.Shares} risk:{trade.RiskValue} reward:{trade.RewardValue} RRR:{trade.RRR}");
                    SimulateTrade(release, trade);
                    Console.ForegroundColor = trade.RealizedProfitPct > 0 ? ConsoleColor.White : ConsoleColor.Red;
                    Console.WriteLine($"{trade.EndReason} PT%:{ptPct:F0} SL%:{slPct:F0} price:{trade.StartPrice:F2} cl.price:{trade.ClosePrice:F2} end:{trade.EndTime} real%:{trade.RealizedProfitPct:F2} real:{trade.RealizedProfitValue:F2}");
                    tradeCollection.InsertOne(trade);
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }

        private static void SetQuantity(Trade t)
        {
            t.Shares = (int)Math.Truncate((maxRiskValue - usualCommissions) / Math.Abs(t.StartPrice - t.StopLossValue));
            t.UsedMargin = t.Shares * t.StartPrice;
            if (t.UsedMargin > maxMargin)
            {
                t.Shares = (int) (maxMargin / t.StartPrice);
                t.UsedMargin = t.Shares * t.StartPrice;
            }
            t.RiskValue = (t.Shares * Math.Abs(t.StartPrice - t.StopLossValue)) + usualCommissions;
            t.RewardValue = (t.Shares * Math.Abs(t.TargetValue - t.StartPrice)) - usualCommissions;
            t.RRR = t.RiskValue / t.RewardValue;
        }

        private static void SimulateTrade(EarningEvent release, Trade trade)
        {
            var candles = release.Candles.Where(x => x.Time > trade.StartTime).OrderBy(x => x.Time);
            if (candles.Count() == 0) throw new Exception("No data after release date.");
            foreach (var candle in candles)
            {
                if (trade.DirectionLong)
                {
                    if (trade.StopLossValue >= candle.Low)
                    {
                        trade.EndTime = candle.Time;
                        trade.Finished = true;
                        trade.ClosePrice = trade.StopLossValue - usualSlippage;
                        trade.EndReason = "SL+";
                        break;
                    }
                    if (trade.TargetValue <= candle.High)
                    {
                        trade.EndTime = candle.Time;
                        trade.Finished = true;
                        trade.ClosePrice = trade.TargetValue;
                        trade.EndReason = "PT+";
                        break;
                    }
                }
                else
                {
                    if (trade.StopLossValue <= candle.High)
                    {
                        trade.EndTime = candle.Time;
                        trade.Finished = true;
                        trade.ClosePrice = trade.StopLossValue + usualSlippage;
                        trade.EndReason = "SL-";
                        break;
                    }
                    if (trade.TargetValue >= candle.Low)
                    {
                        trade.EndTime = candle.Time;
                        trade.Finished = true;
                        trade.ClosePrice = trade.TargetValue;
                        trade.EndReason = "PT-";
                        break;
                    }
                }
            }
            if (!trade.Finished)
            {
                var lastCandle = candles.Last();
                trade.Finished = true;
                trade.EndTime = lastCandle.Time;
                trade.Finished = true;
                trade.ClosePrice = lastCandle.Close;
                trade.EndReason = "OUT";
            }
            if (trade.DirectionLong)
            {
                trade.RealizedProfitPct = 100 * (trade.ClosePrice - trade.StartPrice) / trade.StartPrice;
                trade.RealizedProfitValue = trade.Shares * (trade.ClosePrice - trade.StartPrice);
            }
            else
            {
                trade.RealizedProfitPct = 100 * (trade.StartPrice - trade.ClosePrice) / trade.StartPrice;
                trade.RealizedProfitValue = trade.Shares * (trade.StartPrice - trade.ClosePrice);
            }
        }

        private static Candle FindStartCandle(EarningEvent release, DateTime realReleaseTime)
        {
            return release.Candles.Where(x => x.Time >= realReleaseTime).OrderBy(x => x.Time).FirstOrDefault();
        }

        private static double calculateGap(EarningEvent release, DateTime realReleaseTime)
        {
            Candle lastCandleBefore = release.Candles.Where(x => x.Time < realReleaseTime).OrderBy(x => x.Time).LastOrDefault();
            double closingPrice = lastCandleBefore.Close;
            Candle firstCandleAfter = release.Candles.Where(x => x.Time >= realReleaseTime).OrderBy(x => x.Time).FirstOrDefault();
            double openingPrice = firstCandleAfter.Open;
            double gap = 100 * ((openingPrice / closingPrice) - 1);
            //Console.WriteLine($"{release.Symbol} {realReleaseTime} cl:{closingPrice} {lastCandleBefore.Time} op:{openingPrice} {firstCandleAfter.Time} gap:{gap}");
            return gap;
        }

        private static DateTime findRealReleaseTime(EarningEvent release, out Candle closingCandle)
        {
            closingCandle = null;
            if (release.EarningsCallTime != null && release.EarningsCallTime.Equals("Before Market Open")) return release.Date;
            if (release.EarningsCallTime != null && release.EarningsCallTime.Equals("After Market Close")) return release.Date.AddDays(1);

            closingCandle = release.Candles.Where(x => x.Time < release.Date).OrderBy(x => x.Time).LastOrDefault();
            if (closingCandle == null)
                throw new Exception("No candles before open");
            double closingPrice0 = closingCandle.Close;
            Candle firstCandleAfter0 = release.Candles.Where(x => x.Time >= release.Date).OrderBy(x => x.Time).FirstOrDefault();
            if (firstCandleAfter0 == null)
                throw new Exception("No candles after open");
            double openingPrice0 = firstCandleAfter0.Open;
            double gap0 = Math.Abs((openingPrice0 / closingPrice0) - 1);

            Candle lastCandleBefore1 = release.Candles.Where(x => x.Time < release.Date.AddDays(1)).OrderBy(x => x.Time).LastOrDefault();
            double closingPrice1 = lastCandleBefore1.Close;
            Candle firstCandleAfter1 = release.Candles.Where(x => x.Time >= release.Date.AddDays(1)).OrderBy(x => x.Time).FirstOrDefault();
            double openingPrice1 = firstCandleAfter1.Open;
            double gap1 = Math.Abs((openingPrice1 / closingPrice1) - 1);

            if (gap0 > 2 * gap1 && gap0 > 0.01)
                return release.Date;
            if (gap1 > 2 * gap0 && gap1 > 0.01)
                return release.Date.AddDays(1);

            throw new Exception("Not sure about exact time");
        }
    }
}
