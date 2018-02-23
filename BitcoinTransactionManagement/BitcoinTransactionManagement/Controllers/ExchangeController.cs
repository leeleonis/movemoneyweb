using BitcoinTransactionManagement.Filters;
using BitcoinTransactionManagement.Helpers;
using BitcoinTransactionManagement.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BitcoinTransactionManagement.Controllers
{
    [CheckSession]
    public class ExchangeController : BaseController
    {
        int pageSize = 10;

        /// <summary>
        /// 交易所管理
        /// </summary>
        /// <param name="Name">交易所名稱</param>
        /// <returns></returns>
        public ActionResult MainExchange(string Name, int page = 1)
        {
            var Exchange = db.Exchange.Where(x => x.Status != -1);
            ViewBag.CurrencyList = db.Currency.AsQueryable().ToList();
            if (!string.IsNullOrWhiteSpace(Name))
            {
                Exchange = Exchange.Where(x => x.Name.Contains(Name));
            }
            QueryString();
            return View(Exchange.OrderBy(x => x.Createdt).ToPagedList(page, pageSize));
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateExchange()
        {
            List<CheckBoxListInfo> CurrencyCheckBox = new List<CheckBoxListInfo>();
            var Currency = db.Currency.OrderBy(x => x.Value).ToList();
            foreach (var item in Currency)
            {
                CurrencyCheckBox.Add(new CheckBoxListInfo(item.Value, item.Name, false));
            }
            ViewBag.CurrencyCheckBox = CurrencyCheckBox;

            return View();
        }

        /// <summary>
        /// 新增(Post)
        /// </summary>
        /// <param name="Exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateExchange(Exchange Exchange, string[] CurrencyCheckBox)
        {
            if (ModelState.IsValid)
            {
                var UserID = Session["UserID"].ToString();
                var NowTime = DateTime.Now;
                var NewGuid = Guid.NewGuid();

                Exchange.id = NewGuid;
                Exchange.CreateID = UserID;
                Exchange.Createdt = NowTime;
                Exchange.UpdateID = UserID;
                Exchange.Updatedt = NowTime;
                Exchange.Status = 1;
                db.Exchange.Add(Exchange);

                if (CurrencyCheckBox != null)
                {
                    foreach (var item in CurrencyCheckBox)
                    {
                        ExchangeCurrency AddExchangeCurrency = new ExchangeCurrency
                        {
                            ExchangeId = NewGuid,
                            CurrencyVal = item,
                            CreateID = UserID,
                            Createdt = NowTime
                        };
                        db.ExchangeCurrency.Add(AddExchangeCurrency);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("MainExchange");
            }
            else
            {
                return RedirectToAction("Error", "Base", new { msg = "新增交易所時發生錯誤" });
            }
        }

        /// <summary>
        /// 編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditExchange(Guid id)
        {
            var Exchange = db.Exchange.Find(id);

            List<CheckBoxListInfo> CurrencyCheckBox = new List<CheckBoxListInfo>();
            var Currency = db.Currency.OrderBy(x => x.Value).ToList();
            foreach (var item in Currency)
            {
                CurrencyCheckBox.Add(new CheckBoxListInfo(item.Value, item.Name, item.ExchangeCurrency.Where(x => x.ExchangeId == id).Any()));
            }
            ViewBag.CurrencyCheckBox = CurrencyCheckBox;

            return View(Exchange);
        }

        /// <summary>
        /// 編輯(Post)
        /// </summary>
        /// <param name="Exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditExchange(Exchange Exchange, string[] CurrencyCheckBox)
        {
            try
            {
                var UserID = Session["UserID"].ToString();
                var DateTimeNow = DateTime.Now;

                //編輯交易所
                var sql = db.Exchange.Find(Exchange.id);
                sql.Name = Exchange.Name;
                sql.ExchangeUrl = Exchange.ExchangeUrl;
                sql.ProcessingFee = Exchange.ProcessingFee;
                sql.UpdateID = UserID;
                sql.Updatedt = DateTimeNow;

                //找出刪除的幣別選項
                var WantDeletdCurrency = sql.ExchangeCurrency.ToList();
                if (CurrencyCheckBox != null)
                {
                    foreach (var item in CurrencyCheckBox)
                    {
                        WantDeletdCurrency = WantDeletdCurrency.Where(x => x.CurrencyVal != item).ToList();
                    }
                }

                //交易所帳號內的幣別一同刪除
                var FundsBalance = db.FundsBalance;
                foreach (var Account in sql.Account)
                {
                    var WantDeleteFundsBalance = Account.FundsBalance;
                    foreach (var item in WantDeletdCurrency)
                    {
                        WantDeleteFundsBalance.Where(x => x.CurrencyVal == item.CurrencyVal);
                    }
                    FundsBalance.RemoveRange(WantDeleteFundsBalance);
                }

                //刪除幣別關聯
                var ExchangeCurrency = db.ExchangeCurrency;
                ExchangeCurrency.RemoveRange(sql.ExchangeCurrency);

                //重新新增幣別關聯
                if (CurrencyCheckBox != null)
                {
                    foreach (var item in CurrencyCheckBox)
                    {
                        ExchangeCurrency AddExchangeCurrency = new ExchangeCurrency
                        {
                            ExchangeId = Exchange.id,
                            CurrencyVal = item,
                            CreateID = UserID,
                            Createdt = DateTimeNow
                        };
                        db.ExchangeCurrency.Add(AddExchangeCurrency);
                    }
                }

                //關閉有關聯之執行程式
                var ExecutionsAccount = db.ExecutionsAccount.Where(x => x.Account.ExchangeId == Exchange.id);
                foreach (var item in ExecutionsAccount)
                {
                    item.Executions.Status = 0;
                    SetExecutions.KillProcess(item.ExecutionsId.ToString());
                }

                db.SaveChanges();
                
                return RedirectToAction("MainExchange");
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Base", new { msg = "編輯時發生錯誤" + ex.Message });
            }
        }

        /// <summary>
        /// 假刪除
        /// </summary>
        /// <param name="id">UsersId</param>
        /// <returns></returns>
        public ActionResult DeleteExchange(Guid id)
        {
            try
            {
                //此交易所狀態改-1
                var Exchange = db.Exchange.Find(id);
                Exchange.DisableID = Session["UserID"].ToString();
                Exchange.Disabledt = DateTime.Now;
                Exchange.Status = -1;

                //底下的帳號狀態改-1
                var AccountList = db.Account.Where(x => x.ExchangeId == id && x.Status == 1);
                foreach (var Account in AccountList)
                {
                    Account.DisableID = Session["UserID"].ToString();
                    Account.Disabledt = DateTime.Now;
                    Account.Status = -1;

                    //關閉有關聯之執行程式
                    foreach (var ExecutionsAccount in Account.ExecutionsAccount)
                    {
                        ExecutionsAccount.Executions.Status = 0;
                    }

                    //刪除帳號關聯
                    db.ExecutionsAccount.RemoveRange(Account.ExecutionsAccount);
                }

                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除交易所時發生錯誤" + ex.Message });
            }
        }

        /// <summary>
        /// 帳戶設定主頁
        /// </summary>
        /// <param name="ExchangeId"></param>
        /// <param name="Name"></param>
        /// <param name="APIKey"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult MainAccount(Guid ExchangeId, string Name, string APIKey, int page = 1)
        {
            var Exchange = db.Exchange.Find(ExchangeId);
            ViewBag.Exchange = Exchange;
            ViewBag.CurrencyList = db.Currency.AsQueryable().ToList();

            var Account = Exchange.Account.Where(x => x.Status != -1);
            if (!string.IsNullOrWhiteSpace(Name))
            {
                Account = Account.Where(x => x.Name.Contains(Name));
            }
            if (!string.IsNullOrWhiteSpace(APIKey))
            {
                Account = Account.Where(x => x.Name.Contains(APIKey));
            }

            QueryString();
            return View(Account.OrderBy(x => x.Createdt).ToPagedList(page, pageSize));
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="ExchangeId"></param>
        /// <returns></returns>
        public ActionResult CreateAccount(Guid ExchangeId)
        {
            ViewBag.ExchangeId = ExchangeId;
            return View();
        }

        //新增(POST)
        [HttpPost]
        public ActionResult CreateAccount(Guid ExchangeId, Account Account)
        {
            if (ModelState.IsValid)
            {
                var NowTime = DateTime.Now;

                Account.id = Guid.NewGuid();
                Account.ExchangeId = ExchangeId;
                Account.CreateID = Session["UserID"].ToString();
                Account.Createdt = NowTime;
                Account.UpdateID = Session["UserID"].ToString();
                Account.Updatedt = NowTime;
                Account.Status = 1;

                db.Account.Add(Account);
                db.SaveChanges();

                return RedirectToAction("MainAccount", new { ExchangeId = ExchangeId });
            }
            else
            {
                return RedirectToAction("Error", "Base", new { msg = "新增帳戶時發生錯誤" });
            }
        }

        /// <summary>
        /// 編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditAccount(Guid id)
        {
            var Account = db.Account.Find(id);
            return View(Account);
        }

        /// <summary>
        /// 編輯(Post)
        /// </summary>
        /// <param name="Exchange"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditAccount(Account Account)
        {
            try
            {
                var sql = db.Account.Find(Account.id);
                sql.Name = Account.Name;
                sql.APIKey = Account.APIKey;
                sql.Secret = Account.Secret;
                sql.UpdateID = Session["UserID"].ToString();
                sql.Updatedt = DateTime.Now;

                //關閉有關聯之執行程式
                var ExecutionsAccount = db.ExecutionsAccount.Where(x => x.AccountId == sql.id);
                foreach (var item in ExecutionsAccount)
                {
                    item.Executions.Status = 0;
                    SetExecutions.KillProcess(item.ExecutionsId.ToString());
                }

                db.SaveChanges();
                return RedirectToAction("MainAccount", new { ExchangeId = sql.ExchangeId });
            }
            catch (Exception ex)
            {
                return RedirectToAction("Error", "Base", new { msg = "編輯時發生錯誤" + ex.Message });
            }
        }

        /// <summary>
        /// 假刪除
        /// </summary>
        /// <param name="id">UsersId</param>
        /// <returns></returns>
        public ActionResult DeleteAccount(Guid id)
        {
            try
            {
                var Account = db.Account.Find(id);
                Account.DisableID = Session["UserID"].ToString();
                Account.Disabledt = DateTime.Now;
                Account.Status = -1;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "刪除交易所時發生錯誤" + ex.Message });
            }
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        public ActionResult CreateFundsBalance(Guid AccountId)
        {
            var Account = db.Account.Find(AccountId);
            List<SelectListItem> CurrencySelectList = new List<SelectListItem>();
            foreach (var Currency in db.Currency.OrderBy(x => x.Value).ToList())
            {
                if (Account.Exchange.ExchangeCurrency.Where(x=>x.CurrencyVal==Currency.Value).Any())
                {
                    SelectListItem SelectListItem = new SelectListItem
                    {
                        Value = Currency.Value,
                        Text = Currency.Name,
                        Selected = false
                    };
                    CurrencySelectList.Add(SelectListItem);
                }
            }
            ViewBag.CurrencySelectList = CurrencySelectList;
            ViewBag.Account = Account;
            return View();
        }

        /// <summary>
        /// 新增(POST)
        /// </summary>
        /// <param name="AccountId"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateFundsBalance(Guid AccountId, FundsBalance FundsBalance)
        {
            if (ModelState.IsValid)
            {
                var NowTime = DateTime.Now;

                var sql = db.FundsBalance.Where(x => x.AccountId == AccountId && x.CurrencyVal == FundsBalance.CurrencyVal);
                if (sql.Any())
                {
                    sql.First().Quantity = sql.First().Quantity + FundsBalance.Quantity;
                }
                else
                {
                    FundsBalance.AccountId = AccountId;
                    FundsBalance.CreateID = Session["UserID"].ToString();
                    FundsBalance.Createdt = NowTime;

                    db.FundsBalance.Add(FundsBalance);
                }
                
                db.SaveChanges();

                var Account = db.Account.Find(AccountId);
                return RedirectToAction("MainAccount", new { ExchangeId = Account.ExchangeId });
            }
            else
            {
                return RedirectToAction("Error", "Base", new { msg = "新增帳戶時發生錯誤" });
            }
        }

    }
}