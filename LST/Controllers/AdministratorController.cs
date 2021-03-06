﻿/*
Copyright [2016] [puyang.c@foxmail.com]

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using LST.Models;
using System.Data.Entity.Infrastructure;

namespace LST.Controllers
{
    [Authorize(Users = "admin@uibesea.org")]
    public class AdministratorController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Administrator
        public ActionResult Index(MessageType message = MessageType.None)
        {
            ViewBag.ErrorInfo =
                message == MessageType.DbUpdateFailed ? "更新失败：并发控制生效，请核对该场次新记录后重新修改" :
                message == MessageType.Failed ? "失败：操作未完成。" :
                message == MessageType.Success ? "成功：已完成操作" : "";

            return View(
                db.TestMatches
                    .Where(t => t.EndTime > DateTime.Now)
                    .OrderBy(t => t.StartTime)
                    .ThenBy(t => t.Name)
                    .ToList()
                );
        }

        public ActionResult History()
        {
            return View(db.TestMatches.Where(t => t.EndTime < DateTime.Now).OrderBy(t => t.Name));
        }

        // GET: Administrator/Details/5
        public ActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TestMatch testMatch = db.TestMatches.Find(id);
            if (testMatch == null)
            {
                return HttpNotFound();
            }
            return View(testMatch);
        }

        // GET: Administrator/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Administrator/Create
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Limit,StartTime,EndTime,Visible")] TestMatch testMatch)
        {
            if (ModelState.IsValid)
            {
                testMatch.Id = Guid.NewGuid();
                db.TestMatches.Add(testMatch);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(testMatch);
        }

        // GET: Administrator/Edit/5
        public ActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TestMatch testMatch = db.TestMatches.Find(id);
            if (testMatch == null)
            {
                return HttpNotFound();
            }
            return View(testMatch);
        }

        // POST: Administrator/Edit/5
        // 为了防止“过多发布”攻击，请启用要绑定到的特定属性，有关 
        // 详细信息，请参阅 http://go.microsoft.com/fwlink/?LinkId=317598。
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Limit,StartTime,EndTime,Visible,RowVersion")] TestMatch testMatch)
        {
            if (ModelState.IsValid)
            {
                db.Entry(testMatch).State = EntityState.Modified;
                try
                {
                    db.SaveChanges();
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    ex.Entries.Single().Reload();

                    return RedirectToAction("Index", new { message = MessageType.DbUpdateFailed });
                }
                return RedirectToAction("Index");
            }
            return View(testMatch);
        }

        // GET: Administrator/Delete/5
        public ActionResult Delete(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TestMatch testMatch = db.TestMatches.Find(id);
            if (testMatch == null)
            {
                return HttpNotFound();
            }
            return View(testMatch);
        }

        // POST: Administrator/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(Guid id)
        {
            TestMatch testMatch = db.TestMatches.Find(id);
            foreach (var item in testMatch.RecordsCollection)
            {
                db.TestRecords.Remove(item);
            }
            db.SaveChanges();
            db.TestMatches.Remove(testMatch);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult GenerateMatches()
        {
            return View(new GenerateMatchesViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateMatches(GenerateMatchesViewModel model)
        {
            if (ModelState.IsValid)
            {
                TestHelper helper = new TestHelper();
                bool result = helper.GenerateMatches(
                    model.Days.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries),
                    model.Lessons.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries),
                    model.Limit,
                    model.StartTime,
                    model.EndTime);
                if (result)
                {
                    helper.ResetAppliedState();
                    return RedirectToAction("Index", new { message = MessageType.Success });
                }
                else
                {
                    ViewBag.Error = "创建失败";
                    return View(model);
                }
            }

            return View(model);
        }

        public ActionResult GetAllData()
        {
            System.IO.MemoryStream output = new System.IO.MemoryStream();
            System.IO.StreamWriter writer = new System.IO.StreamWriter(output, System.Text.Encoding.UTF8);
            var matches = db.TestMatches.ToList();
            writer.Write("Match,MatchCount,StartTime,EndTime,User,StudentNumber,PhoneNumber,Email,UserCount");
            writer.WriteLine();
            foreach (var item in matches)
            {
                foreach (var record in item.RecordsCollection)
                {
                    writer.Write(item.Name + ",");
                    writer.Write(item.Count + ",");
                    writer.Write(item.StartTime + ",");
                    writer.Write(item.EndTime + ",");
                    writer.Write(record.User.StudentName + ",");
                    writer.Write(record.User.StudentNumber + ",");
                    writer.Write(record.User.PhoneNumber + ",");
                    writer.Write(record.User.Email + ",");
                    writer.Write(record.User.RecordsCollection.Count + ",");
                    writer.WriteLine();
                }
            }
            writer.Flush();
            output.Position = 0;
            return File(output, "text/comma-separated-values", "demo1.csv");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        public enum MessageType
        {
            Success,
            Failed,
            DbUpdateFailed,
            None
        }
    }
}
