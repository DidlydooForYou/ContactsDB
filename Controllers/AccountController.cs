using DAL;
using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using static Controllers.AccessControl;

namespace ContactsDB.Controllers
{
    public class AccountsController : Controller
    {
        public JsonResult EmailExist(string Email)
        {
            return Json(DB.Users.ToList().Where(u => u.Email == Email).Any(), JsonRequestBehavior.AllowGet);
        }

        public JsonResult EmailAvailable(string Email)
        {
            bool NotAvailable = false;
            int currentId = Models.User.ConnectedUser != null ? Models.User.ConnectedUser.Id : 0;

            User foundUser = DB.Users.ToList()
                .Where(u => u.Email == Email && u.Id != currentId)
                .FirstOrDefault();

            NotAvailable = foundUser != null;

            return Json(NotAvailable, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ExpiredSession()
        {
            return Redirect("/Accounts/Login?message=Session expirée, veuillez vous reconnecter.&success=false");
        }

        public ActionResult Logout()
        {
            return Redirect("/Accounts/Login");
        }

        public ActionResult Login(string message = "", bool success = true)
        {
            if (Models.User.ConnectedUser != null)
            {
                if (Session["spy"] == null)
                {
                    if (success)
                        DB.Events.Add("Logout");
                    else
                        DB.Events.Add("Expired/blocked");

                    DB.Logins.UpdateLogoutByUserId(Models.User.ConnectedUser.Id);

                    Models.User.ConnectedUser.Online = false;
                    Session["spy"] = null;
                }

                Models.User.ConnectedUser = null;
            }

            Session["LoginSuccess"] = success;
            Session["LoginMessage"] = message;

            if (Session["CurrentLoginEmail"] == null)
                Session["CurrentLoginEmail"] = "";

            LoginCredential credential = new LoginCredential
            {
                Email = (string)Session["CurrentLoginEmail"]
            };

            return View(credential);
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult Login(LoginCredential credential)
        {
            DateTime serverDate = DateTime.Now;
            int serverTimeZoneOffset = serverDate.Hour - serverDate.ToUniversalTime().Hour;

            Session["TimeZoneOffset"] = -(credential.TimeZoneOffset + serverTimeZoneOffset);

            credential.Email = credential.Email.Trim();
            credential.Password = credential.Password.Trim();

            Session["CurrentLoginEmail"] = credential.Email;

            User loginUser = DB.Users.GetUser(credential);

            if (loginUser == null)
            {
                Session["LoginSuccess"] = false;
                Session["LoginMessage"] = "Courriel ou mot de passe incorrect";
                return View(credential);
            }

            if (loginUser.Online)
            {
                return Redirect("/Accounts/Login?message=Il y a déjà une session ouverte avec cet usager!&success=false");
            }

            if (loginUser.Blocked)
            {
                return Redirect("/Accounts/Login?message=Votre compte a été bloqué!&success=false");
            }

            if (!loginUser.Verified)
            {
                return Redirect("/Accounts/Login?message=Votre compte n'a pas été vérifié!&success=false");
            }

            Models.User.ConnectedUser = loginUser;

            loginUser.Online = true;

            DB.Users.Update(loginUser);

            DB.Logins.Add(Models.User.ConnectedUser.Id);
            DB.Events.Add("Login");

            Session["LoginSuccess"] = true;
            Session["LoginMessage"] = "";

            return RedirectToAction("List", "Students");
        }

        [UserAccess(Access.Admin)]
        public ActionResult ChangeIdentity(int userid)
        {
            User newIdentity = DB.Users.Get(userid);

            if (newIdentity != null)
            {
                Models.User.ConnectedUser.Online = false;
                Models.User.ConnectedUser = newIdentity;
                Session["spy"] = true;

                return RedirectToAction("List", "Students");
            }

            return null;
        }

        public ActionResult RenewPasswordCommand()
        {
            ViewBag.EmailNotFound = false;
            return View(new EmailView());
        }

        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult RenewPasswordCommand(EmailView emailView)
        {
            User user = DB.Users.ToList()
                .Where(u => u.Email == emailView.Email)
                .FirstOrDefault();

            if (user != null)
            {
                return Redirect("/Accounts/Login?message=Un courriel de commande de changement de mot de passe vous a été envoyé si l'adresse fournie est valide.");
            }

            ViewBag.EmailNotFound = true;
            return View(emailView);
        }

        [UserAccess(Models.Access.View)]
        public ActionResult EditProfil()
        {
            User connectedUser = Models.User.ConnectedUser;

            if (connectedUser != null)
            {
                Session["CurrentEditingUserPassword"] = DateTime.Now.Ticks.ToString();
                return View(connectedUser);
            }

            return RedirectToAction("List", "Students");
        }

        [UserAccess(Models.Access.View)]
        [HttpPost]
        [ValidateAntiForgeryToken()]
        public ActionResult EditProfil(User user, string NotifyCB = "off")
        {
            DB.Events.Add("EditProfil");

            User connectedUser = Models.User.ConnectedUser;

            user.Id = connectedUser.Id;
            user.Blocked = connectedUser.Blocked;
            user.Access = connectedUser.Access;
            user.Verified = connectedUser.Verified;
            user.Online = connectedUser.Online;
            user.Notify = NotifyCB == "on";

            if (user.Password == (string)Session["CurrentEditingUserPassword"])
                user.Password = connectedUser.Password;

            DB.Users.Update(user);

            Models.User.ConnectedUser = DB.Users.Get(user.Id);

            return RedirectToAction("List", "Students");
        }

        [UserAccess(Models.Access.View)]
        public ActionResult DeleteProfil()
        {
            DB.Events.Add("DeleteProfil");

            User connectedUser = Models.User.ConnectedUser;

            if (connectedUser != null)
            {
                DB.Users.Delete(connectedUser.Id);
                Models.User.ConnectedUser = null;
            }

            return Redirect("/Accounts/Login?message=Votre compte a été effacé avec succès!");
        }

        [UserAccess(Access.Admin)]
        public ActionResult GetUsers(bool forceRefresh = false)
        {
            if (DB.Users.HasChanged || DB.Logins.HasChanged || forceRefresh)
            {
                return PartialView(
                    DB.Users.ToList()
                        .Where(u => u.Id != Models.User.ConnectedUser.Id)
                        .OrderBy(u => u.Name)
                        .ToList()
                );
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult ManageUsers()
        {
            DB.Events.Add("ManageUsers");
            return View();
        }

        [UserAccess(Access.Admin)]
        public ActionResult SetUserAccess(int userid, int access)
        {
            DB.Events.Add("SetUserAccess");

            if (userid != 1)
            {
                User user = DB.Users.Get(userid);

                if (user != null)
                {
                    user.Access = (Models.Access)access;
                    DB.Users.Update(user);
                }
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult ToggleBlockUser(int id)
        {
            if (id != 1)
            {
                User user = DB.Users.Get(id);

                if (user != null)
                {
                    user.Blocked = !user.Blocked;
                    user.Online = false;

                    DB.Users.Update(user);

                    if (user.Blocked)
                        DB.Events.Add("BlockUser", user.Name);
                    else
                        DB.Events.Add("UnBlockUser", user.Name);
                }
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult ForceVerifyUser(int id)
        {
            if (id != 1)
            {
                User user = DB.Users.Get(id);

                if (user != null)
                {
                    user.Verified = true;
                    DB.Users.Update(user);
                }
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult DeleteUser(int id)
        {
            if (id != 1)
            {
                User user = DB.Users.Get(id);

                if (user != null)
                {
                    DB.Events.Add("DeleteUser", user.Name);
                    DB.Users.Delete(id);
                }
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult LoginsJournal()
        {
            return View();
        }

        [UserAccess(Access.Admin)]
        public ActionResult GetLoginsList(bool forceRefresh = false)
        {
            if (DB.Logins.HasChanged || forceRefresh)
            {
                List<User> onlineUsers = DB.Users.ToList()
                    .Where(u => u.Online)
                    .ToList();

                ViewBag.LoggedUsersId = onlineUsers.Select(u => u.Id).ToList();

                List<Login> logins = DB.Logins.ToList()
                    .OrderByDescending(l => l.LoginDate)
                    .ToList();

                return PartialView(logins);
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult EventsJournal()
        {
            return View();
        }

        [UserAccess(Access.Admin)]
        public ActionResult GetEventsList(bool forceRefresh = false)
        {
            if (DB.Events.HasChanged || forceRefresh)
            {
                List<Event> events = DB.Events.ToList()
                    .OrderByDescending(e => e.CreationDate)
                    .ToList();

                return PartialView(events);
            }

            return null;
        }

        [UserAccess(Access.Admin)]
        public ActionResult DeleteLoginsDay(string day)
        {
            try
            {
                DateTime date = DateTime.Parse(day);
                DB.Logins.DeleteLoginsJournalDay(date);
            }
            catch (Exception) { }

            return RedirectToAction("LoginsJournal");
        }

        [UserAccess(Access.Admin)]
        public ActionResult DeleteEventsDay(string day)
        {
            try
            {
                DateTime date = DateTime.Parse(day);
                DB.Events.DeleteEventsJournalDay(date);
            }
            catch (Exception) { }

            return RedirectToAction("EventsJournal");
        }
    }
}