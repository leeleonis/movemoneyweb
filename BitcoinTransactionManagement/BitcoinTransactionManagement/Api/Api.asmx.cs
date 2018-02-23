using BitcoinTransactionManagement.HubModel;
using BitcoinTransactionManagement.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Services;

namespace BitcoinTransactionManagement.Api
{
    /// <summary>
    ///Api1 的摘要描述
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允許使用 ASP.NET AJAX 從指令碼呼叫此 Web 服務，請取消註解下列一行。
    // [System.Web.Script.Services.ScriptService]
    public class Api1 : System.Web.Services.WebService
    {
        [WebMethod]
        public string GetExecutions()
        {
            BitcoinTransactionEntities db = new BitcoinTransactionEntities();
            return JsonConvert.SerializeObject(db.Executions.AsQueryable().ToList(), new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.Indented
            });
        }

        [WebMethod]
        public void ReturnMessage(string Message,string ExecutionsId,int Status,int ProcessId)
        {
            //檢查應關但未關程式
            //你應該被關掉啦!?
            //還傳東西給我幹嘛(((ﾟДﾟ;)))
            SetExecutions.ShouldKillProcess(ExecutionsId, ProcessId);

            //如果要秀東西
            if (true)
            {
                var context = GlobalHost.ConnectionManager.GetHubContext<myhub>();
                ProcessMessage ProcessMessage = new ProcessMessage
                {
                    ExecutionsId = ExecutionsId,
                    Message = Message
                };
                context.Clients.All.Message(ProcessMessage);
            }
            
        }

        public class ProcessMessage
        {
            public string ExecutionsId { get; set; }

            public string Message { get; set; }
        }

    }
}
