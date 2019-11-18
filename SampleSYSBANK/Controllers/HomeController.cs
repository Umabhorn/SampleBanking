using SampleSYSBANK.BankingService;
using SampleSYSBANK.ComponentUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SampleSYSBANK.Controllers
{
    public class HomeController : Controller
    {


        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        #region User
        public ActionResult Index()
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var data = client.DataUser();

            return View(data);
        }
        public ActionResult User(string id)
        {
            UserDTO DataUser = new UserDTO();
            if (id != null)
            {
                ViewBag.inAction = "Edit";
                WcfBankingServiceClient client = new WcfBankingServiceClient();
                DataUser = client.GetUserBankRecord(Convert.ToInt32(id));
            }
            else
            {
                ViewBag.inAction = "Create";
            }
            return View(DataUser);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UserCancel")]
        public ActionResult UserCancel()
        {
            return RedirectToAction("Index", "Home");

        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UserDelete")]
        public ActionResult UserDelete(UserDTO UserData)
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            UserData.IsActive = false;
            var User = client.UpDateUser(UserData);
            return RedirectToAction("Index", "Home");

        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UserEdit")]
        public ActionResult UserEdit(UserDTO UserData)
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var User = client.UpDateUser(UserData);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "UserSave")]
        public ActionResult UserSave(UserDTO UserData)
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            UserData.IsActive = true;
            var User = client.AddUser(UserData);
            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Account

        public ActionResult AccountList(int id)
        {
            if (id != null )
            {
 ViewBag.UserID = id;
         
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var data = client.DataAccountBYUser(id);
                Session["UserID"] = id;
                return View(data);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
           
        }

        public ActionResult Account(string usrid, string id)
        {
            ViewBag.UserID = usrid;
            if (id != null)
            {
                ViewBag.inAction = "Edit";
                Session["UserID"] = id;
            }
            else
            {
                ViewBag.inAction = "Create";
            }
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var data = client.GetAccountBankRecord(Convert.ToInt32(id));
            return View(data);
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AccountCancel")]
        public ActionResult AccountCancel(AccountDTO AccountData)
        {
            return RedirectToAction("AccountList", "Home", new { id = AccountData.User_ID });

        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AccountDelete")]
        public ActionResult AccountDelete(AccountDTO AccountData)
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            AccountData.IsActive = false;
            AccountData.UpdateBy =Convert.ToInt16( Session["UserID"].ToString());
            AccountData.UpdateDate = DateTime.Now;
            var Account = client.UpDateAccount(AccountData);
            return RedirectToAction("Index", "Home");

        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AccountEdit")]
        public ActionResult AccountEdit(AccountDTO AccountData)
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            AccountData.UpdateBy = Convert.ToInt16(Session["UserID"].ToString());
            AccountData.UpdateDate = DateTime.Now;
            var Account = client.UpDateAccount(AccountData);
            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        [MultipleButton(Name = "action", Argument = "AccountSave")]
        public ActionResult AccountSave(AccountDTO AccountData)
        {
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var dataCheck = client.GetRecordByIBAN(AccountData.IBAN);
            if (dataCheck == null)
            {
                if (AccountData.TotalBalance == null ) AccountData.TotalBalance = 0;
                AccountData.IsActive = true;
                AccountData.UpdateBy = Convert.ToInt16(Session["UserID"].ToString());
                AccountData.UpdateDate = DateTime.Now;
                var Account = client.AddAccount(AccountData);
            }
            return RedirectToAction("Index", "Home");
        }

        #endregion


        public ActionResult Statement(string iban, string usrid)
        {
            ViewBag.IBAN = iban;
            ViewBag.UserID = usrid;
            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var data = client.GetAllByIBAN(iban).OrderByDescending(r=>r.CreateDate);
            return View(data);
        }

        #region Withdraw
        public ActionResult Withdraw(string iban, string usrid)
        {
            if (iban != null )
            {
                ViewBag.IBAN = iban;
                WcfBankingServiceClient client = new WcfBankingServiceClient();
                var dataAccount = client.GetRecordByIBAN(iban);
                if (dataAccount.TotalBalance != null)
                {
                    ViewBag.Balance = dataAccount.TotalBalance;
                }
                else
                {
                    ViewBag.Balance = 0;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WithdrawSave")]
        public ActionResult WithdrawSave(StatementDTO data)
        {

            WcfBankingServiceClient client = new WcfBankingServiceClient();
            var dataAccount = client.GetRecordByIBAN(data.IBANTo);
            data.PrevBalanceTo = dataAccount.TotalBalance;
            data.Channel = "Website";
            data.CreateDate = DateTime.Now;
            data.CreateBy = Convert.ToInt16(Session["UserID"].ToString());

            var ra = client.AddStatement(data);
            return RedirectToAction("Statement", "Home", new { iban = data.IBANFrom , usrid = Session["UserID"].ToString() });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "WithdrawCancel")]
        public ActionResult WithdrawCancel(StatementDTO data)
        {
            return RedirectToAction("AccountList", "Home", new { id = Session["UserID"].ToString() });
           
        }
        #endregion
        #region Deposit
        public ActionResult Deposit(string iban)
        {
            if (iban != null)
            {
                ViewBag.IBAN = iban;
                WcfBankingServiceClient client = new WcfBankingServiceClient();
                var dataAccount = client.GetRecordByIBAN(iban);
                if (dataAccount.TotalBalance != null)
                {
                    ViewBag.Balance = dataAccount.TotalBalance;
                }
                else
                {
                    ViewBag.Balance = 0;
                }
                return View();
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DepositSave")]
        public ActionResult DepositSave(StatementDTO data)
        {

            WcfBankingServiceClient client = new WcfBankingServiceClient();
            //var dataAccount = client.GetRecordByIBAN(data.IBANTo);
            //data.PrevBalanceTo = dataAccount.TotalBalance;
            data.Channel = "Website";
            data.CreateDate = DateTime.Now;
            data.CreateBy = Convert.ToInt16(Session["UserID"].ToString());
            var ra = client.AddStatement(data);
            return RedirectToAction("Statement", "Home", new { iban = data.IBANTo, usrid = Session["UserID"].ToString() });
        }

        [HttpPost]
        [MultipleButton(Name = "action", Argument = "DepositCancel")]
        public ActionResult DepositCancel(StatementDTO data)
        {
            return RedirectToAction("AccountList", "Home", new { id = Session["UserID"].ToString() });
          
        }
        #endregion
    }
}