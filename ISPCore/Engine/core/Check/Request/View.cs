﻿using ISPCore.Engine.Auth;
using ISPCore.Engine.Base;
using ISPCore.Engine.Base.SqlAndCache;
using ISPCore.Engine.core.Cache.CheckLink;
using ISPCore.Engine.Hash;
using ISPCore.Engine.Middleware;
using ISPCore.Models.core;
using ISPCore.Models.core.Cache.CheckLink.Common;
using ISPCore.Models.Databases;
using ISPCore.Models.Databases.json;
using ISPCore.Models.RequestsFilter.Base.Enums;
using ISPCore.Models.RequestsFilter.Domains;
using ISPCore.Models.RequestsFilter.Domains.Log;
using ISPCore.Models.RequestsFilter.Domains.Types;
using ISPCore.Models.RequestsFilter.Monitoring;
using ISPCore.Models.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ModelCache = ISPCore.Models.core.Cache.CheckLink;

namespace ISPCore.Engine.core.Check
{
    public partial class Request
    {
        #region ViewDomainNotFound
        public static Task ViewDomainNotFound(HttpContext context)
        {
            context.Response.StatusCode = 500;
            return context.Response.WriteAsync(@"<!DOCTYPE html>
<html lang='ru-RU'>
<head>
    <title>Ошибка</title>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <meta name='viewport' content='width=device-width, initial-scale=1, user-scalable=no'>
    <link rel='stylesheet' href='/statics/style.css'>
</head>
<body>
    <div class='error'>
        <div class='error-block'>

            <div class='code'>500</div>
            <div class='title'>Домен не найден</div>
            <pre>Добавьте домен в ISPCore и настройте фильтр запросов</pre>

            <div class='copyright'>
                <div>
                    &copy; 2018 <strong>ISPCore</strong>. All rights reserved.
                </div>
                <div>
                    <a href='/'>Главная сайта</a> / <a href='http://core-system.org/' target='_blank'>Core System</a>
                </div>
            </div>

        </div>
    </div>
</body>
</html>", context.RequestAborted);
        }
        #endregion

        #region View
        public static Task View(HttpContext context, ViewBag viewBag, ActionCheckLink Model)
        {
            #region Код ответа
            if (viewBag.IsErrorRule)
            {
                context.Response.StatusCode = 500;
            }
            else
            {
                switch (Model)
                {
                    case ActionCheckLink.allow:
                        {
                            context.Response.StatusCode = 303;
                            break;

                        }
                    case ActionCheckLink.Is2FA:
                        {
                            context.Response.StatusCode = 200;
                            break;

                        }
                    case ActionCheckLink.deny:
                        {
                            context.Response.StatusCode = 403;
                            break;

                        }
                    default:
                        {
                            context.Response.StatusCode = 500;
                            break;
                        }
                }
            }
            #endregion

            #region Локальный метод - "RenderTitle"
            string RenderTitle()
            {
                if (viewBag.IsErrorRule)
                {
                    return "Ошибка";
                }
                else
                {
                    switch (Model)
                    {
                        case ActionCheckLink.allow:
                            return "303";
                        case ActionCheckLink.deny:
                            return "Доступ запрещен";
                        case ActionCheckLink.Is2FA:
                            return "Aвторизация 2FA";
                        default:
                            return "Неизвестная ошибка";
                    }
                }
            }
            #endregion

            #region Локальный метод - "RenderScript"
            string RenderScript()
            {
                if (Model == ActionCheckLink.Is2FA)
                {
                    string hash = SHA256.Text($"{viewBag.host}:{viewBag.method}:{viewBag.uri}:{viewBag.Referer}:{PasswdTo.salt}");
                    return @"
<script>
    function unlock(e)
    {
        e.preventDefault();
        document.getElementById('unlockError').style.display = 'none';

        var password = document.getElementById('unlockPassword').value;

        $.post('" + viewBag.CoreAPI + "/unlock/2fa', { password: password, host: '" + viewBag.host + "', method: '" + viewBag.method + "', uri: '" + WebUtility.UrlEncode(viewBag.uri) + "', referer: '" + WebUtility.UrlEncode(viewBag.Referer) + "', hash: '" + hash + @"' }, function (data)
        {
            var json = JSON.parse(JSON.stringify(data));

            if (json.msg) {
                document.getElementById('unlockError').style.display = 'block';
                document.getElementById('unlockError').innerText = json.msg;
            }
            else if (json.result) {
                window.location.reload();
            }
            else {
                document.getElementById('unlockError').style.display = 'block';
                document.getElementById('unlockError').innerText = 'Неизвестная ошибка';
            }
        })
    }
</script>";
                }

                return string.Empty;
            }
            #endregion

            #region Локальный метод - "RenderBody"
            string RenderBody()
            {
                if (viewBag.IsErrorRule)
                {
                    return @"<div class='code'>500</div>
                    <div class='title'>"+ viewBag.ErrorTitleException + @"</div>
                    <pre>" + viewBag.ErrorRuleException + @"</pre>";
                }
                else if (Model == ActionCheckLink.Is2FA)
                {
                    return @"<div class='code'>2FA</div>
                    <div class='title'>Aвторизация</div>
                    <pre>Введите пароль безопасности для 2FA</pre>

                    <form method='post' action='/' onsubmit='unlock(event)'>
                        <div class='form-group'>
                            <div class='input-group form'>
                                <span class='input-group-addon'><i class='fa fa-lock'></i></span>
                                <input class='form-control' id='unlockPassword' type='password' name='password'>
                            </div>

                            <button type='submit' class='btn-unlock'>Unlock</button>

                            <div id='unlockError' class='errorMsg'>eroror</div>
                        </div>
                    </form>";
                }
                else if (Model == ActionCheckLink.allow)
                {
                    return @"<div class='code'>303</div>
                    <div class='title'>Отправить в backend</div>";
                }

                else if (Model == ActionCheckLink.deny)
                {
                    return @"<div class='code'>403</div>
                    <div class='title'>Доступ запрещен</div>";
                }

                else
                {
                    return @"<div class='code'>500</div>
                    <div class='title'>Неизвестная ошибка</div>";
                }
            }
            #endregion

            #region Локальный метод - "RenderDebug"
            string RenderDebug()
            {
                if (viewBag.DebugEnabled)
                {
                    #region jsonDomain
                    string jsonDomain()
                    {
                        if (viewBag.jsonDomain == null)
                            return string.Empty;

                        return Regex.Replace(viewBag.jsonDomain.Replace("\\\\", "\\"), "(<!--|-->|\"Auth2faToPasswd\": \"[^\"]+\")", r => 
                        {
                            switch (r.Groups[1].Value)
                            {
                                case "<!--":
                                    return "&lt;!--";

                                case "-->":
                                    return "--&gt;";

                                default:
                                    {
                                        if (r.Groups[1].Value.StartsWith("\"Auth2faToPasswd\":"))
                                            return "\"Auth2faToPasswd\": \"Используется\"";
                                    }
                                    break;
                            }

                            return r.Groups[1].Value;
                        });
                    }
                    #endregion

                    return @"
<!--
IP:         " + viewBag?.IP + @"
UserAgent:  " + viewBag?.UserAgent + @"
method:     " + viewBag?.method + @"
host:       " + viewBag?.host + @"
uri:        " + viewBag?.uri + @"
FormData:   " + viewBag?.FormData + @"
Referer:    " + viewBag?.Referer + @"

" + viewBag.antiBotToGlobalConf?.Replace("\\\\", "\\")?.Replace("<!--", "&lt;!--")?.Replace("-->", "--&gt;") + @"


" + jsonDomain() + @"
-->
";
                }

                return string.Empty;
            }
            #endregion

            // Html ответ
            return context.Response.WriteAsync(@"<!DOCTYPE html>
<html lang='ru-RU'>
<head>
    <title>" + RenderTitle() + @"</title>
    <meta http-equiv='Content-Type' content='text/html; charset=utf-8'>
    <meta http-equiv='X-UA-Compatible' content='IE=edge'>
    <meta name='viewport' content='width=device-width, initial-scale=1, user-scalable=no'>
    <link rel='stylesheet' href='/statics/style.css'>
    " + (Model == ActionCheckLink.Is2FA ? "<script type='text/javascript' src='/statics/jquery.min.js'></script>" : string.Empty) + @"
</head>
<body>

" + RenderScript() + @"

    <div class='error'>
        <div class='error-block'>

            " + RenderBody() + @"

            <div class='copyright'>
                <div>
                    &copy; 2018 <strong>ISPCore</strong>. All rights reserved.
                </div>
                <div>
                    <a href='/'>Главная сайта</a> / <a href='http://core-system.org/' target='_blank'>Core System</a>
                </div>
            </div>
        </div>
    </div>
</body>
</html>

" + RenderDebug() + @"
", context.RequestAborted);
        }
        #endregion
    }
}
