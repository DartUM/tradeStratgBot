using System;
using System.Drawing;
using System.Linq;
using PowerLanguage.Function;
using ATCenterProxy.interop;
using PowerLanguage.Indicator;
 
namespace PowerLanguage.Strategy
{
        public class AA_1 : SignalObject
    {
                public AA_1(object ctx):base(ctx)
                {
                        Lenght = 50;
                        TP = 3000;
                        SL = 1000;
                    StrongInx = 10;
                }
 
                private IOrderMarket _buyMarketOrder;
        private IOrderMarket _shortMarketOrder;
 
        private IOrderMarket _SessionCLBuy;
        private IOrderMarket _SessionClSell;
 
        private IOrderPriced _SL_long;
        private IOrderPriced _SL_short;
 
        private IOrderPriced _tp_long;
        private IOrderPriced _tp_short;
 
        private double AVG { get; set; }
        private int HHCounter { get; set; }
        private int LLCounter { get; set; }
               
            [Input]
            public int Lenght { get; set; }
        [Input]
        public double TP { get; set; }
        [Input]
        public double SL { get; set; }
        [Input]
        public int StrongInx { get; set; }
 
                protected override void Create()
        {
                        // create variable objects, function objects, order objects etc.
            _buyMarketOrder = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "_buyMarketOrder", EOrderAction.Buy));
            _shortMarketOrder = OrderCreator.MarketNextBar(new SOrderParameters(Contracts.Default, "_shortMarketOrder", EOrderAction.SellShort));
 
            _SessionCLBuy = OrderCreator.MarketThisBar(new SOrderParameters(Contracts.Default, "_SessionCLBuy", EOrderAction.BuyToCover));
            _SessionClSell = OrderCreator.MarketThisBar(new SOrderParameters(Contracts.Default, "_SessionClSell", EOrderAction.Sell));
 
            _SL_short = OrderCreator.Stop(new SOrderParameters(Contracts.Default, "_SL_short", EOrderAction.BuyToCover));
            _SL_long = OrderCreator.Stop(new SOrderParameters(Contracts.Default, "_SL_long", EOrderAction.Sell));
 
            _tp_short = OrderCreator.Limit(new SOrderParameters(Contracts.Default, "_tp_short", EOrderAction.BuyToCover));
            _tp_long = OrderCreator.Limit(new SOrderParameters(Contracts.Default, "_tp_long", EOrderAction.Sell));          
                }
 
                protected override void StartCalc()
        {
                        // assign inputs
                }
 
        protected override void CalcBar()
        {
            AVG = this.StrategyInfo.AvgEntryPrice;
 
            var hh = PublicFunctions.Highest(Bars.High, Lenght, 2);
            var ll = PublicFunctions.Lowest(Bars.Low, Lenght, 2);
 
            if (PublicFunctions.DoubleGreater(Bars.High[1], hh))
            {
                HHCounter++;
                LLCounter = 0;
            }
            else if (PublicFunctions.DoubleLess(Bars.Low[1], ll))
            {
                LLCounter++;
                HHCounter = 0;
            }
 
            if (this.StrategyInfo.MarketPosition == 0)
            {
                if (HHCounter >= StrongInx)
                {
                        if(PublicFunctions.DoubleGreater(Bars.High[0], hh))
                        _buyMarketOrder.Send();
                }
                else if (LLCounter >= StrongInx)
                {  
                                        if (PublicFunctions.DoubleLess(Bars.Low[0], ll))
                    _shortMarketOrder.Send();                  
                }
            }
            else if (this.StrategyInfo.MarketPosition >= 1)
            {
                _SL_long.Send(AVG - SL);
                _tp_long.Send(AVG + TP);
 
                if(Bars.LastBarInSession)
                {
                    _SessionClSell.Send();
                }
            }
            else if (this.StrategyInfo.MarketPosition < 0)
            {
                _SL_short.Send(AVG + SL);
                _tp_short.Send(AVG - TP);
 
                if(Bars.LastBarInSession)
                {
                    _SessionCLBuy.Send();
                }
            }
        }
    }
}
