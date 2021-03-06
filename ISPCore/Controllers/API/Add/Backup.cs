﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ISPCore.Models.Databases;
using ISPCore.Models.SyncBackup.Tasks;
using Microsoft.EntityFrameworkCore;
using ISPCore.Models.Response;

namespace ISPCore.Controllers
{
    public class ApiAddBackup : ControllerToDB
    {
        public JsonResult Task(Task tk, FTP ftp, Models.SyncBackup.Tasks.WebDav webDav, OneDrive oneDrive)
        {
            return new SyncBackupToTaskController().Save(tk, ftp, webDav, oneDrive);
        }


        public JsonResult Ignore(int TaskId, IDictionary<string, IgnoreFileOrFolders> ignr)
        {
            // Записываем новые данные
            coreDB.SyncBackup_Task_IgnoreFileOrFolders.AddRange(TaskId, ignr, out var NewIgnore);

            // Сохраняем базу
            coreDB.SaveChanges();

            // Отдаем сообщение и Id новых шаблонов
            return Json(new UpdateToIds("accepted", TaskId, NewIgnore));
        }
    }
}
