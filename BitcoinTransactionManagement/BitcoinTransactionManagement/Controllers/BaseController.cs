using BitcoinTransactionManagement.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BitcoinTransactionManagement.Controllers
{
    public class BaseController : Controller
    {
        public BitcoinTransactionEntities db = new BitcoinTransactionEntities();

        public ActionResult Error(string msg)
        {
            ViewBag.msg = msg;
            return View();
        }

        //排序
        protected void QueryString()
        {
            var nvc = HttpUtility.ParseQueryString(Request.Url.Query);
            var RemoveList = new List<string>
            {
                "page"
            };
            foreach (string item in nvc)
            {
                if (string.IsNullOrWhiteSpace(nvc[item]))
                {
                    RemoveList.Add(item);
                }
            }
            foreach (var item in RemoveList)
            {
                nvc.Remove(item);
            }

            TempData["QueryString"] = nvc;
        }

        //選擇狀態
        public List<SelectListItem> StatusSelect(string Value)
        {
            List<SelectListItem> SelectItemList = new List<SelectListItem>
            {
                new SelectListItem() { Text = "開啟", Value = "1", Selected = false },
                new SelectListItem() { Text = "關閉", Value = "0", Selected = false }
            };
            if (!string.IsNullOrEmpty(Value))
            {
                SelectItemList.Where(x => x.Value == Value).First().Selected = true;
            }

            return SelectItemList;
        }

        //選擇交易狀態
        public List<SelectListItem> TransactionStatusSelect(string Value)
        {
            List<SelectListItem> SelectItemList = new List<SelectListItem>
            {
                new SelectListItem() { Text = "成功", Value = "1", Selected = false },
                new SelectListItem() { Text = "失敗", Value = "-1", Selected = false }
            };
            if (!string.IsNullOrEmpty(Value))
            {
                SelectItemList.Where(x => x.Value == Value).First().Selected = true;
            }

            return SelectItemList;
        }

        //選擇交易狀態
        public List<SelectListItem> TransactionTypeSelect(string Value)
        {
            List<SelectListItem> SelectItemList = new List<SelectListItem>
            {
                new SelectListItem() { Text = "BID", Value = "1", Selected = false },
                new SelectListItem() { Text = "ASK", Value = "0", Selected = false }
            };
            if (!string.IsNullOrEmpty(Value))
            {
                SelectItemList.Where(x => x.Value == Value).First().Selected = true;
            }

            return SelectItemList;
        }

    }
}