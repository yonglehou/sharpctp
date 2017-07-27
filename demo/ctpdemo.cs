using CTP;
using System;
using System.Collections.Generic;

namespace demo
{
    public class ctpdemo
    {
        static FtdcMdAdapter DataApi;
        static FtdcTdAdapter TraderApi;

        static int iRequestID = 0;

        static string TD_FRONT_ADDR = "tcp://180.168.146.187:10000";
        static string MD_FRONT_ADDR = "tcp://180.168.146.187:10010";
        static string BrokerID = "9999";

        private string user = "18868407239";
        private string pass = "850215";
        
        void DataApi_OnRtnEvent(object sender, OnRtnEventArgs e)
        {
            //Console.WriteLine("DataApi_OnRtnEvent " + e.EventType.ToString());

            var fld = Conv.P2S<ThostFtdcDepthMarketDataField>(e.Param);

            Console.WriteLine("{0}.{1:D3} {2} {3}", fld.UpdateTime, fld.UpdateMillisec, fld.InstrumentID, fld.LastPrice);
        }

        void DataApi_OnRspEvent(object sender, OnRspEventArgs e)
        {
            Console.WriteLine("DataApi_OnRspEvent " + e.EventType.ToString());
            bool err = IsError(e.RspInfo, e.EventType.ToString());

            switch (e.EventType)
            {
                case EnumOnRspType.OnRspUserLogin:
                    if (!err)
                        Console.WriteLine("登录成功");
                    break;
                case EnumOnRspType.OnRspSubMarketData:
                {
                    var f = Conv.P2S<ThostFtdcSpecificInstrumentField>(e.Param);
                    Console.WriteLine("订阅成功:" + f.InstrumentID);
                }
                    break;
                case EnumOnRspType.OnRspUnSubMarketData:
                {
                    var f = Conv.P2S<ThostFtdcSpecificInstrumentField>(e.Param);
                    Console.WriteLine("退订成功:" + f.InstrumentID);
                }
                    break;
            }
        }

        void DataApi_OnFrontEvent(object sender, OnFrontEventArgs e)
        {
            switch (e.EventType)
            {
                case EnumOnFrontType.OnFrontConnected:
                {
                    var req = new ThostFtdcReqUserLoginField();
                    req.BrokerID = BrokerID;
                    req.UserID = user;
                    req.Password = pass;
                    int iResult = DataApi.ReqUserLogin(req, ++iRequestID);
                }
                    break;
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            if (DataApi == null)
            {
                if (CheckInput() == false)
                    return;

                DataApi = new FtdcMdAdapter("", false, false);
                DataApi.OnFrontEvent += DataApi_OnFrontEvent;
                DataApi.OnRspEvent += DataApi_OnRspEvent;
                DataApi.OnRtnEvent += DataApi_OnRtnEvent;

                DataApi.RegisterFront(MD_FRONT_ADDR);
                DataApi.Init();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            List<string> inst = new List<string>();
            inst.Add("ag1612");
            inst.Add("au1612");
            inst.Add("SR801");
            if (DataApi != null)
                DataApi.SubscribeMarketData(inst.ToArray());
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (DataApi != null)
            {
                DataApi.Dispose();
                DataApi = null;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (DataApi != null)
            {
                DataApi.UnSubscribeMarketData(new string[] { "ag1612" });
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (TraderApi == null)
            {
                if (CheckInput() == false)
                    return;

                TraderApi = new FtdcTdAdapter("");
                TraderApi.OnFrontEvent += TraderApi_OnFrontEvent;
                TraderApi.OnRspEvent += TraderApi_OnRspEvent;
                TraderApi.OnRtnEvent += TraderApi_OnRtnEvent;
                TraderApi.OnErrRtnEvent += TraderApi_OnErrRtnEvent;
                TraderApi.SubscribePublicTopic(EnumTeResumeType.THOST_TERT_QUICK);
                TraderApi.SubscribePrivateTopic(EnumTeResumeType.THOST_TERT_QUICK);
                TraderApi.RegisterFront(TD_FRONT_ADDR);
                TraderApi.Init();
            }
        }

        bool IsError(ThostFtdcRspInfoField rspinfo, string source)
        {
            if (rspinfo != null && rspinfo.ErrorID != 0)
            {
                Console.WriteLine(rspinfo.ErrorMsg + ", 来源 " + source);
                return true;
            }
            return false;
        }

        void TraderApi_OnErrRtnEvent(object sender, OnErrRtnEventArgs e)
        {
            Console.WriteLine("=====> " + e.EventType);
        }

        void TraderApi_OnRtnEvent(object sender, OnRtnEventArgs e)
        {
            Console.WriteLine("=====> " + e.EventType);
        }

        void TraderApi_OnRspEvent(object sender, OnRspEventArgs e)
        {
            bool err = IsError(e.RspInfo, e.EventType.ToString());

            switch (e.EventType)
            {
                case EnumOnRspType.OnRspUserLogin:
                    if (!err)
                    {
                        Console.WriteLine("登录成功");
                        var fld = Conv.P2S<ThostFtdcRspUserLoginField>(e.Param);
                        Console.WriteLine("TradingDay is " + fld.TradingDay);

                        ThostFtdcSettlementInfoConfirmField req = new ThostFtdcSettlementInfoConfirmField();
                        req.BrokerID = BrokerID;
                        req.InvestorID = user;
                        TraderApi.ReqSettlementInfoConfirm(req, ++iRequestID);
                    }
                    break;
                case EnumOnRspType.OnRspQryInstrument:
                    if (e.Param != IntPtr.Zero)
                    {
                        var fld = Conv.P2S<ThostFtdcInstrumentField>(e.Param);
                        Console.WriteLine("=====> {0}, {1},  isLast {2}", e.EventType, fld.InstrumentID, e.IsLast);
                    }
                    break;
            }
        }

        void TraderApi_OnFrontEvent(object sender, OnFrontEventArgs e)
        {
            switch (e.EventType)
            {
                case EnumOnFrontType.OnFrontConnected:
                    {
                        var req = new ThostFtdcReqUserLoginField();
                        req.BrokerID = BrokerID;
                        req.UserID = user;
                        req.Password = pass;
                        int iResult = TraderApi.ReqUserLogin(req, ++iRequestID);
                    }
                    break;
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            if (TraderApi != null)
            {
                TraderApi.Dispose();
                TraderApi = null;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (TraderApi != null)
            {
                var req = new ThostFtdcQryInstrumentField();
                req.InstrumentID = "";
                req.ExchangeID = "";
                TraderApi.ReqQryInstrument(req, ++iRequestID);
            }
        }

        private bool CheckInput()
        {
            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                Console.WriteLine("请输入用户名和密码");
                return false;
            }
            return true;
        }
    }
}