﻿using ISPCore.Models.Base.WhiteList;
using System.Collections.Generic;
using System.Linq;

namespace ISPCore.Engine.Base.SqlAndCache
{
    public class WhiteList
    {
        static WhiteListCache cache = null;
        
        /// <summary>
        /// Кеш настроек WhiteList
        /// </summary>
        /// <param name="conf">Оригинальные настройки WhiteList</param>
        public static WhiteListCache GetCache(Models.Databases.json.WhiteList conf)
        {
            // Кеш настроек
            if (cache != null && cache.LastUpdateToConf == conf.LastUpdateToConf)
                return cache;

            #region Локальный метод - "JoinMass"
            string JoinMass(List<string> mass, bool IsUserAgent = false)
            {
                if (mass == null || mass.Count == 0)
                    return "^$";

                if (IsUserAgent)
                    return $"({string.Join("|", mass)})";

                return $"^({string.Join("|", mass)})$";
            }
            #endregion

            // Базовый список PTR
            List<string> PTRs = new List<string>(conf.Values.Where(i => i.Type == WhiteListType.PTR).Select(i => i.Value).ToArray());
            PTRs.Add(@".*\.(yandex.(ru|net|com)|googlebot.com|google.com|mail.ru|search.msn.com)");

            // Базовый список IP
            List<string> IPs = new List<string>(conf.Values.Where(i => i.Type == WhiteListType.IPv4Or6).Select(i => i.Value).ToArray());
            IPs.Add(@"(127.0.0.1|::1|8.8.4.4|8.8.8.8|192.168.0.[0-9]+)");

            // Создаем кеш
            cache = new WhiteListCache();
            cache.LastUpdateToConf = conf.LastUpdateToConf;
            cache.IpRegex = JoinMass(IPs);
            cache.PtrRegex = JoinMass(PTRs);
            cache.UserAgentRegex = JoinMass(conf.Values.Where(i => i.Type == WhiteListType.UserAgent).Select(i => i.Value).ToList(), IsUserAgent: true);

            // Успех
            return cache;
        }
    }
}
