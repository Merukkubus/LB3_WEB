using LB3.Models.Entities;
using LB3.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace LB3.Controllers
{
    public class LB3Controller : Controller
    {
        // GET: LB3
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
    }
}