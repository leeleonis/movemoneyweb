using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BitcoinTransactionManagement.Models;

namespace BitcoinTransactionManagement.Controllers
{
    public class HomeController : Controller
    {
        /// <summary>
        /// 登入
        /// </summary>
        /// <returns></returns>
        public ActionResult Login()
        {
            if (Session["IsLogin"] != null && (bool)Session["IsLogin"])
            {
                return RedirectToAction("Index");
            }
            else
            {
                Session.Add("IsLogin", false);
                return View();
            }
        }

        /// <summary>
        /// 登入(POST)
        /// </summary>
        /// <param name="username">帳號</param>
        /// <param name="password">密碼</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            SHA256 sha256 = new SHA256CryptoServiceProvider();
            password = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(password)));
            BitcoinTransactionEntities db = new BitcoinTransactionEntities();
            var Users = db.Users.Where(x => x.UserName == username && x.Password == password && x.Status == 1);
            //登入成功
            if (Users.Any())
            {
                Session.Add("IsLogin", true);
                Session.Add("UserID", Users.First().id);
                Session.Add("UserName", Users.First().Name);

                if (username != "weypro1")
                {
                    //應該開但沒開
                    string ProcessName = Server.MapPath("~") + @"cmd\BitcoinDeveloper.exe";
                    var Url = Request.Url.Scheme + "://" + Request.Url.Authority + "/api/api.asmx?op=ReturnMessage";
                    var ExecutionsList = db.Executions.Where(x => x.Status != -1).ToList();
                    SetExecutions.ShouldStartProcess(ProcessName, ExecutionsList, Url);
                }

                return RedirectToAction("MainExchange", "Exchange");
            }
            else
            {
                ModelState.AddModelError("", "登入失敗，請確認輸入帳密");
                return View();
            }
        }

        /// <summary>
        /// 登出
        /// </summary>
        /// <returns></returns>
        public ActionResult Logout()
        {
            Session.RemoveAll();
            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login");
        }
    }
}