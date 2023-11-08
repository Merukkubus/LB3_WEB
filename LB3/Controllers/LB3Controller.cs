using LB3.Models.Entities;
using LB3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Web.Security;
using System.Text;

namespace LB3.Controllers
{
    public class LB3Controller : Controller
    {
        [AllowAnonymous]
        public ActionResult Index()
        {
            List<Human> humans = new List<Human>();
            using (var db = new PersonEntities())
            {
                humans = db.Human.OrderByDescending(x => x.Age)
                    .ThenBy(x => x.LastName)
                    .ThenBy(x => x.FirstName).ToList();
            }
            return View(humans);
        }

        [HttpGet]
        [Authorize]
        public ActionResult PersonalDetails(Guid humanId)
        {
            Human model = new Human();
            using (var db = new PersonEntities())
            {
                model = db.Human.Find(humanId);
            }
            return View(model);
        }
        List<Tuple<string, string>> GetGenderList()
        {
            List<Tuple<string, string>> genders = new List<Tuple<string, string>>
            {
                new Tuple<string, string>("Ж", "Женский"),
                new Tuple<string, string>("М", "Мужской")
            };
            return genders;
        }
        [HttpGet]
        public ActionResult CreateHuman()
        {
            ViewBag.Genders = new SelectList(GetGenderList(), "Item1", "Item2");
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        [Authorize(Roles = "Admin")]
        public ActionResult CreateHuman(HumanVM newHuman)
        {
            if (ModelState.IsValid)
            {
                using (var db = new PersonEntities())
                {
                    Human human = new Human()
                    {
                        Id = Guid.NewGuid(),
                        LastName = newHuman.LastName,
                        FirstName = newHuman.FirstName,
                        Patronymic = newHuman.Patronymic,
                        Gender = newHuman.Gender,
                        Age = newHuman.Age,
                        HasJob = newHuman.HasJob,
                    };
                    db.Human.Add(human);
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.Genders = new SelectList(GetGenderList(), "Item1", "Item2");
            return View(newHuman);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult EditHuman(Guid humanId)
        {
            HumanVM model;
            using (var db = new PersonEntities())
            {
                Human hm = db.Human.Find(humanId);
                model = new HumanVM()
                {
                    Id = hm.Id,
                    LastName = hm.LastName,
                    FirstName = hm.FirstName,
                    Gender = hm.Gender,
                    Age = hm.Age,
                    HasJob = hm.HasJob,
                };
            }
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken()]
        [Authorize(Roles = "Admin")]
        public ActionResult EditHuman(HumanVM model) 
        {
            if(ModelState.IsValid)
            {
                using (var db = new PersonEntities())
                {
                    Human editedHuman = new Human
                    {
                        Id = model.Id,
                        LastName = model.LastName,
                        FirstName = model.FirstName,
                        Gender = model.Gender,
                        Age = model.Age,
                        HasJob = model.HasJob,
                    };
                    db.Human.Attach(editedHuman);
                    db.Entry(editedHuman).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            ViewBag.Genders = new SelectList(GetGenderList(), "Item1", "Item2");
            return View(model);
        }
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteHuman(Guid humanId)
        {
            Human humanToDelete;
            using(var db = new PersonEntities())
            {
                humanToDelete = db.Human.Find(humanId);
            }
            return View(humanToDelete);
        }
        [HttpPost, ActionName("DeleteHuman")]
        public ActionResult DeleteConfirmed(Guid humanId)
        {
            using(var db = new PersonEntities())
            {
                Human humanToDelete = db.Human.Find(humanId);
                db.Human.Remove(humanToDelete);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }
        [ChildActionOnly]
        public ActionResult QuestionAnswered(Guid humanId)
        {
            string message = "";
            using (var db = new PersonEntities())
            {
                int q_ans_num = db.Human.Find(humanId).AnswerW.Count();
                message = $"Вопросов отвечено: {q_ans_num}.";
            }
            return PartialView("QuestionAnswered", message);
        }
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login (UserVM webUser)
        {
            if (ModelState.IsValid)
            {
                using (var db = new PersonEntities())
                {
                    User user = null;
                    user = db.User.Where(u => u.Login == webUser.Login).FirstOrDefault();
                    if (user != null)
                    {
                        string passwordHash = ReturnHashCode(webUser.Password + user.Salt.ToString().ToUpper());
                        if(passwordHash == user.PasswordHash)
                        {
                            string userRole = "";
                            switch(user.UserRole)
                            {
                                case 1:
                                    userRole = "Admin";
                                    break;
                                case 2:
                                    userRole = "Participant";
                                    break;
                            }

                            FormsAuthenticationTicket authTicket = new FormsAuthenticationTicket(
                                1,
                                user.Login,
                                DateTime.Now,
                                DateTime.Now.AddDays(1),
                                false,
                                userRole
                            );
                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpContext.Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket));
                            return RedirectToAction("Index");
                        }
                    }
                }
            }
            ViewBag.Error = "Пользователь не найден";
            return View(webUser);
        }
        string ReturnHashCode(string str)
        {
            string hash = "";
            using(SHA1 sha1 = SHA1.Create())
            {
                byte[] data = sha1.ComputeHash(Encoding.UTF8.GetBytes(str));
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                {
                    sb.Append(data[i].ToString("x2"));
                }
                hash = sb.ToString().ToUpper();
            }
            return hash;
        }
        public ActionResult LogOut ()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }
    }
}