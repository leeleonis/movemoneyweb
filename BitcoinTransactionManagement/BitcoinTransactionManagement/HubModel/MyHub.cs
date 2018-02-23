using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using System.Threading;
namespace BitcoinTransactionManagement.HubModel
{
    //[HubName("MyHub")]
    public class myhub : Hub
    {
        public myhub()
        {
            var taskTimer = Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    string timeNow = DateTime.Now.ToString();
                    Clients.All.SendServerTime1(timeNow);
                    await Task.Delay(1000);
                }
            }, TaskCreationOptions.LongRunning);
        }
        public void myhubtime()
        {
            while (true)
            {
                string timeNow = DateTime.Now.ToString();
                Clients.All.SendServerTime2(timeNow);
                Thread.Sleep(1000);
            }
        }
    }
}