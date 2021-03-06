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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Manager.Models;
using System.DirectoryServices;

namespace Manager.Controllers
{
    public class HomeController : Controller
    {
        BaseDbContext db = new BaseDbContext();
        static List<string> HostAddressList = new List<string> { "10.2.13.248" };

        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Index(string id)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var manager = db.Managers.Where(m => m.AccountName == id).SingleOrDefault();
                    if (manager == null)
                    {
                        TempData["Alert"] = "请检查学号是否输入正确";
                    }
                    else
                    {
                        var time = Models.Manager.FindCheckInTime();
                        if (string.IsNullOrWhiteSpace(time.TimeName))
                        {
                            TempData["Alert"] = "不在可签到时间内！";
                            db.Logs.Add(new Log(string.Format("系统：值班员[{0}]的失败签到，原因：不在可签到时间内，当前时间[{1}]。", manager.Name, DateTime.Now)));
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        if (HostAddressList.Contains(Request.UserHostAddress))
                        {
                            TempData["Alert"] = "不是值班机器！";
                            db.Logs.Add(new Log(string.Format("系统：值班员[{0}]的失败签到，原因：非值班机器签到，当前IP[{1}]。", manager.Name, Request.UserHostAddress)));
                            db.SaveChanges();
                            return RedirectToAction("Index");
                        }
                        var pre = db.Status.FirstOrDefault();
                        if (pre != null)
                        {
                            if (pre.TimeName == time.TimeName)
                            {
                                TempData["Alert"] = string.Format("请不要重复签到！值班员【{0}】已签到！", pre.Name);
                                db.Logs.Add(new Log(string.Format("系统：值班员[{0}]的失败签到，原因：已有值班员[{1}]的签到记录。", manager.Name, pre.Name)));
                                db.SaveChanges();
                                return RedirectToAction("Index");
                            }
                            var preManager = db.Managers.Where(m => m.Name == pre.Name).SingleOrDefault();
                            if (preManager != null)
                            {
                                preManager.CheckOut();
                            }
                        }
                        if (manager.CheckIn(time))
                        {
                            TempData["Alert"] = "签到成功！";
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.ReportException(e);
                TempData["Alert"] = string.Format("出现错误，请重试。如果不能解决，请联系技术支持人员，并提供以下信息：时间[{2}]发生在[{0}]上的[{1}]异常，错误已捕获并记录。", e.TargetSite, e.ToString(), DateTime.Now);
                return RedirectToAction("Index");
            }

            return View();
        }

        [CheckLogin]
        public ActionResult Data()
        {
            return View(db.Managers.ToList());
        }
    }
}