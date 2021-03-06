﻿using ISPCore.Engine;
using ISPCore.Engine.Common.Views;
using ISPCore.Engine.Base.SqlAndCache;
using ISPCore.Models.Base;
using ISPCore.Models.Databases.json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using ISPCore.Models.Common.Views;

namespace ISPCore.Models.Databases
{
    public class ControllerToDB : Controller, IDisposable
    {
        dynamic navPage;
        public JsonDB jsonDB { get; }
        public CoreDB coreDB { get; }
        public IMemoryCache memoryCache { get; }

        public ControllerToDB()
        {
            SqlToMode.SetMode(SqlMode.Read);
            jsonDB = Service.Get<JsonDB>();
            memoryCache = Service.Get<IMemoryCache>();
            coreDB = Service.Get<CoreDB>();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ViewResult View<T>(NavPage<T> _navPage, bool _ajax)
        {
            return View<T>(null, _navPage, _ajax);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ViewResult View<T>(string viewName, NavPage<T> _navPage, bool _ajax)
        {
            navPage = _navPage;
            var page = new PageToView<T>(_navPage, _ajax, jsonDB, coreDB, memoryCache);
            return viewName == null ? base.View(page) : base.View(viewName, page);
        }


        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        public new void Dispose()
        {
            navPage?.Dispose();
            coreDB.Dispose();
            SqlToMode.SetMode(SqlMode.ReadOrWrite);
            base.Dispose();
        }
    }
}
