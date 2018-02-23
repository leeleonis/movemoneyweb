using BitcoinTransactionManagement.Filters;
using BitcoinTransactionManagement.HubModel;
using BitcoinTransactionManagement.Models;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using PagedList;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace BitcoinTransactionManagement.Controllers
{
    [CheckSession]
    public class ExecutionsController : BaseController
    {
        int pageSize = 10;

        /// <summary>
        /// 執行管理
        /// </summary>
        /// <param name="Name"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult MainExecutions(string Name, int page = 1)
        {
           
            var context = GlobalHost.ConnectionManager.GetHubContext<myhub>();

            var Executions = db.Executions.Where(x => x.Status != -1);
            if (!string.IsNullOrWhiteSpace(Name))
            {
                Executions = Executions.Where(x => x.Name.Contains(Name));
            }
            QueryString();
            return View(Executions.OrderBy(x => x.Createdt).ToPagedList(page, pageSize));
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateExecutions()
        {
            ViewBag.Statuslist = StatusSelect("");
            ViewBag.Currencylist = db.Currency.OrderBy(x => x.Value).Select(x => new SelectListItem { Text = x.Name, Value = x.Value }).ToList();
            ViewBag.ExchangeType = db.ExchangeTypes.GroupBy(x=>x.Text).Select(x => new SelectListItem { Text = x.Key, Value = x.Key }).ToList();

            return View();
        }

        /// <summary>
        /// 新增(Post)
        /// </summary>
        /// <param name="Exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateExecutions(Executions Executions, string[] AccountIdList)
        {
            if (ModelState.IsValid)
            {
                var NowTime = DateTime.Now;
                var UserID = Session["UserID"].ToString();

                Executions.id = Guid.NewGuid();
                Executions.CreateID = UserID;
                Executions.Createdt = NowTime;
                Executions.UpdateID = UserID;
                Executions.Updatedt = NowTime;

                db.Executions.Add(Executions);

                if (AccountIdList != null)
                {
                    foreach (var AccountId in AccountIdList)
                    {
                        if (!string.IsNullOrEmpty(AccountId))
                        {
                            ExecutionsAccount ExecutionsAccount = new ExecutionsAccount
                            {
                                ExecutionsId = Executions.id,
                                AccountId = Guid.Parse(AccountId),
                                CreateID = UserID,
                                Createdt = NowTime
                            };

                            db.ExecutionsAccount.Add(ExecutionsAccount);
                        }
                    }
                }
                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// 編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditExecutions(Guid id)
        {
            var Executions = db.Executions.Find(id);

            ViewBag.Statuslist = StatusSelect(Executions.Status.ToString());
            ViewBag.Currencylist = db.Currency.OrderBy(x => x.Value).Select(x => new SelectListItem { Text = x.Name, Value = x.Value, Selected = (x.Value == Executions.CurrencyValue) }).ToList();
            ViewBag.ExchangeTypelist = db.ExchangeTypes.GroupBy(x => x.Text).Select(x => new SelectListItem { Text = x.Key, Value = x.Key, Selected = (x.Key == Executions.ExchangeType) }).ToList();
            JsonResult Result = (JsonResult)GetAccountSelectList(Executions.CurrencyValue, Executions.id);
            Dictionary<string, object> Dictionary = (Dictionary<string, object>)Result.Data;
            ViewBag.SelectListHtml = Dictionary["message"].ToString();

            return View(Executions);
        }

        /// <summary>
        /// 編輯(Post)
        /// </summary>
        /// <param name="Exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditExecutions(Executions Executions, string[] AccountIdList)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var sql = db.Executions.Find(Executions.id);
                    sql.Name = Executions.Name;
                    sql.Status = Executions.Status;
                    sql.MinQuantity = Executions.MinQuantity;
                    sql.MinDifferencePrices = Executions.MinDifferencePrices;
                    sql.ExchangeType = Executions.ExchangeType;
                    sql.CurrencyValue = Executions.CurrencyValue;
                    sql.UpdateID = Session["UserID"].ToString();
                    sql.Updatedt = DateTime.Now;

                    db.ExecutionsAccount.RemoveRange(sql.ExecutionsAccount);
                    if (AccountIdList != null)
                    {
                        foreach (var AccountId in AccountIdList)
                        {
                            if (!string.IsNullOrEmpty(AccountId))
                            {
                                ExecutionsAccount ExecutionsAccount = new ExecutionsAccount
                                {
                                    ExecutionsId = Executions.id,
                                    AccountId = Guid.Parse(AccountId),
                                    CreateID = Session["UserID"].ToString(),
                                    Createdt = DateTime.Now
                                };

                                db.ExecutionsAccount.Add(ExecutionsAccount);
                            }
                        }
                    }

                    db.SaveChanges();

                    //開啟搬磚程式
                    if (Executions.Status == 1)
                    {
                        SetExecutions.KillProcess(Executions.id.ToString());
                        string ProcessName = Server.MapPath("~") + @"cmd\BitcoinDeveloper.exe";
                        var Url = Request.Url.Scheme + "://" + Request.Url.Authority+ "/api/api.asmx?op=ReturnMessage";
                        SetExecutions.StartProcess(ProcessName, Executions.id.ToString(), Url);
                    }
                    else if (Executions.Status == 0)
                    {
                        SetExecutions.KillProcess(Executions.id.ToString());
                    }

                    return Json(new { success = true });
                }
                catch (Exception)
                {
                    return Json(new { success = false });
                }
            }
            else
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// 假刪除
        /// </summary>
        /// <param name="id">UsersId</param>
        /// <returns></returns>
        public ActionResult DeleteExecutions(Guid id)
        {
            try
            {
                var Executions = db.Executions.Find(id);
                Executions.DisableID = Session["UserID"].ToString();
                Executions.Disabledt = DateTime.Now;
                Executions.Status = -1;

                foreach (var ExecutionsAccount in Executions.ExecutionsAccount)
                {
                    db.ExecutionsAccount.Remove(ExecutionsAccount);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除交易所時發生錯誤" + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult GetAccountSelectList(string CurrencyValue, Guid? ExecutionsId)
        {
            Dictionary<string, object> jo = new Dictionary<string, object>();
            try
            {
                StringBuilder sb = new StringBuilder();
                var ExchangeCurrency = db.ExchangeCurrency.Where(x => x.CurrencyVal == CurrencyValue).OrderBy(x => x.Exchange.Createdt);
                foreach (var item in ExchangeCurrency)
                {
                    var Exchange = item.Exchange;
                    List<SelectListItem> AccountSelectList = new List<SelectListItem>();

                    if (ExecutionsId.HasValue)
                    {
                        var ExecutionsAccount = db.ExecutionsAccount.Where(x => x.ExecutionsId == ExecutionsId).ToList();
                        AccountSelectList = Exchange.Account.Where(x => x.Status != -1).OrderBy(x => x.Createdt)
                        .Select(x => new SelectListItem { Text = x.Name, Value = x.id.ToString(), Selected = ExecutionsAccount.Where(y => y.AccountId == x.id).Any() }).ToList();
                    }
                    else
                    {
                        AccountSelectList = Exchange.Account.Where(x => x.Status != -1).OrderBy(x => x.Createdt)
                        .Select(x => new SelectListItem { Text = x.Name, Value = x.id.ToString() }).ToList();
                    }

                    TagBuilder Builder_select = new TagBuilder("select")
                    { InnerHtml = "<option value>請選擇</option>" };
                    Builder_select.MergeAttribute("class", "form-control AccountIdList");
                    foreach (var SelectList in AccountSelectList)
                    {
                        TagBuilder option = new TagBuilder("option");
                        option.MergeAttribute("value", SelectList.Value);
                        if (SelectList.Selected)
                        { option.MergeAttribute("selected", "selected"); }
                        option.InnerHtml = SelectList.Text;
                        Builder_select.InnerHtml += option.ToString();
                    }

                    TagBuilder Builder_div2 = new TagBuilder("div")
                    { InnerHtml = Builder_select.ToString(TagRenderMode.Normal) };
                    Builder_div2.MergeAttribute("class", "col-xs-12 col-sm-6");

                    TagBuilder Builder_label = new TagBuilder("label")
                    { InnerHtml = Exchange.Name };
                    Builder_label.MergeAttribute("class", "col-xs-12 col-sm-3 control-label");

                    TagBuilder Builder_div1 = new TagBuilder("div")
                    { InnerHtml = Builder_label.ToString(TagRenderMode.Normal) + Builder_div2.ToString(TagRenderMode.Normal) };
                    Builder_div1.MergeAttribute("class", "form-group AccountIdList_div");

                    sb.Append(Builder_div1.ToString(TagRenderMode.Normal));
                }

                jo.Add("success", true);
                jo.Add("message", sb.ToString());
            }
            catch (Exception ex)
            {
                jo.Add("success", false);
                jo.Add("message", "發生錯誤." + ex.Message);
            }
            return Json(jo);
        }

        public ActionResult MainTransactionConfirmed(Guid ExecutionsId, string ExchangeType, DateTime? StartingTime, DateTime? EndTime,int? TransactionType,int? Status, int page = 1)
        {
            var TransactionConfirmed = db.TransactionConfirmed.Where(x => x.TransactionRecords.ExecutionsId == ExecutionsId);
            ViewBag.ExecutionsId = ExecutionsId;
            ViewBag.ExchangeTypelist = db.ExchangeTypes.GroupBy(x => x.Text).Select(x => new SelectListItem { Text = x.Key, Value = x.Key }).ToList();
            ViewBag.Status = TransactionStatusSelect("");
            ViewBag.TransactionType = TransactionTypeSelect("");
            ViewBag.AccountList = db.Account.Where(x => x.Status != -1 && x.Exchange.Status != -1).Include(x=>x.Exchange).ToList();

            if (!string.IsNullOrWhiteSpace(ExchangeType))
            {
                TransactionConfirmed = TransactionConfirmed.Where(x => x.TransactionRecords.ExchangeType.Contains(ExchangeType));
            }
            if (StartingTime != null)
            {
                var cqqc = StartingTime.Value;
                TransactionConfirmed = TransactionConfirmed.Where(x => x.TransactionRecords.Createdt >= StartingTime.Value);
            }
            if (EndTime != null)
            {
                TransactionConfirmed = TransactionConfirmed.Where(x => x.TransactionRecords.Createdt <= EndTime.Value);
            }
            if (TransactionType != null)
            {
                TransactionConfirmed = TransactionConfirmed.Where(x => x.TransactionType == TransactionType);
            }
            if (Status != null)
            {
                TransactionConfirmed = TransactionConfirmed.Where(x => x.Status == Status);
            }

            QueryString();
            return View(TransactionConfirmed.OrderByDescending(x => x.TransactionRecords.Createdt).ThenBy(x=>x.TransactionType).ToPagedList(page, pageSize));
        }

    }
}