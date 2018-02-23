using BitcoinTransactionManagement.Filters;
using BitcoinTransactionManagement.Models;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace BitcoinTransactionManagement.Controllers
{
    [CheckSession]
    public class SettingsController : BaseController
    {
        int pageSize = 10;

        /// <summary>
        /// 使用者管理
        /// </summary>
        /// <param name="Name">姓名</param>
        /// <param name="UserName">帳號</param>
        /// <param name="page"></param>
        /// <returns></returns>
        public ActionResult MainUsers(string Name, string UserName, int page = 1)
        {
            var Users = db.Users.Where(x => x.Status != -1);
            if (!string.IsNullOrWhiteSpace(Name))
            {
                Users = Users.Where(x => x.Name.Contains(Name));
            }
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                Users = Users.Where(x => x.UserName.Contains(UserName));
            }
            QueryString();
            return View(Users.OrderByDescending(x => x.Createdt).ToPagedList(page, pageSize));
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateUsers()
        {
            return View();
        }

        /// <summary>
        /// 新增(Post)
        /// </summary>
        /// <param name="Users"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult CreateUsers(Users Users)
        {
            if (ModelState.IsValid)
            {
                if (db.Users.Where(x => x.UserName == Users.UserName && x.Status==1).Any())
                {
                    ModelState.AddModelError("", "帳號重複！");
                    //Response.Write("<script>alert('帳號重複！'); </script>");
                    return View();
                }
                else
                {
                    var NowTime = DateTime.Now;

                    SHA256 sha256 = new SHA256CryptoServiceProvider();

                    Users.id = Guid.NewGuid();
                    Users.Password = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Users.Password)));
                    Users.CreateID = Session["UserID"].ToString();
                    Users.Createdt = NowTime;
                    Users.UpdateID = Session["UserID"].ToString();
                    Users.Updatedt = NowTime;
                    Users.Status = 1;

                    db.Users.Add(Users);
                    db.SaveChanges();

                    return RedirectToAction("MainUsers");
                }
            }
            else
            {
                return RedirectToAction("Error", "Base", new { msg = "新增使用者時發生錯誤" });
            }
        }

        /// <summary>
        /// 編輯
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EditUsers(Guid id)
        {
            var Users = db.Users.Find(id);
            return View(Users);
        }

        /// <summary>
        /// 編輯(Post)
        /// </summary>
        /// <param name="Users"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult EditUsers(Users Users)
        {
            try
            {
                if (db.Users.Where(x => x.UserName == Users.UserName && x.id != Users.id && x.Status==1).Any())
                {
                    ModelState.AddModelError("", "帳號重複！");
                    return View(Users);
                }
                else
                {
                    SHA256 sha256 = new SHA256CryptoServiceProvider();
                    var sql = db.Users.Find(Users.id);
                    sql.Name = Users.Name;
                    sql.UserName = Users.UserName;
                    sql.Password = Convert.ToBase64String(sha256.ComputeHash(Encoding.Default.GetBytes(Users.Password)));
                    sql.UpdateID = Session["UserID"].ToString();
                    sql.Updatedt = DateTime.Now;

                    db.SaveChanges();
                    return RedirectToAction("MainUsers");
                }
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
        public ActionResult DeleteUsers(Guid id)
        {
            if (Session["UserID"].ToString()!=id.ToString())
            {
                var Users = db.Users.Find(id);
                Users.DisableID = Session["UserID"].ToString();
                Users.Disabledt = DateTime.Now;
                Users.Status = -1;
                db.SaveChanges();
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "不得刪除本身帳號" });
            }
        }

    }
}