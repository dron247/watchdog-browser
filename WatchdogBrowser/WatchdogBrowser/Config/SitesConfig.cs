using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using WatchdogBrowser.CustomEventArgs;
using WatchdogBrowser.Models;

namespace WatchdogBrowser.Config {
    public class SitesConfig {
        //Событие вызывается когда конфиг прочитан
        public event EventHandler<ConfigReadyEventArgs> Ready;

        //Доступ к списку сайтов, обычно это не нужно, так как он отдаётся в событии
        public List<SiteModel> Sites {
            get {
                return sites;
            }
        }

        List<SiteModel> sites = new List<SiteModel>();

        string configPath;//путь к файлу с настройками

        public SitesConfig() {
            //читаем путь из настроек приложения
            configPath = Properties.Settings.Default.ConfigFilename;
        }

        /// <summary>
        /// Запускает чтение конфига
        /// </summary>
        public void Initialize() {
            var configText = ReadConfig();
            try {
                sites = ParseConfig(configText);
                Ready?.Invoke(this, new ConfigReadyEventArgs(sites));
            } catch (Exception e) {
                throw e;
            }
        }

        /// <summary>
        /// Считывает текст из файла конфига
        /// </summary>
        /// <returns>строку с xml</returns>
        string ReadConfig() {
            if (!File.Exists(configPath)) {
                return string.Empty;
            }
            using (var reader = File.OpenText(configPath)) {
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Выполняет разбор строки с xml конфигом
        /// </summary>
        /// <param name="xmlString">строка с xml конфигом</param>
        /// <returns>список с моделями настроек сайтов</returns>
        List<SiteModel> ParseConfig(string xmlString) {
            List<SiteModel> retval = new List<SiteModel>();
            if (xmlString.Equals(string.Empty)) {
                var siteModel = new SiteModel {
                    Name = "Файл конфигупации не найден",
                    UpdateInterval = 0,
                    SwitchMirrorTimeout = 0,
                    UpdateTimeout = 0,
                    Username = string.Empty,
                    Password = string.Empty,
                    Watched = false,
                    Message = "Обратитесь в отдел IT",
                    Mirrors = new List<string> { $"http://yandex.ru" },
                    Whitelist = new List<string>()
                };

                retval.Add(siteModel);
                return retval;
            }

            try {
                var xdoc = XDocument.Parse(xmlString);
                var xSites = xdoc.Descendants("sites");
                if (xSites.Count() > 0) {
                    //берём только первый элемент, по ТЗ он один, но такой код даёт задел на доработку
                    var xSite = xSites.Elements().First();
                    var siteName = string.Empty;
                    int updateOk = 10, updateFail = 60, updateTimeout = 20;
                    string username = "", password = "", message = "";
                    bool watched = false;
                    List<string> mirrors = new List<string>();
                    List<string> whitelist = new List<string>();


                    foreach (var attr in xSite.Attributes()) {
                        if (attr.Name == "name") {
                            try {
                                siteName = attr.Value;
                            } catch {
                                siteName = "Неизвестно";
                            }
                            continue;
                        }

                        if (attr.Name == "heartbeatTimeout") {
                            try {
                                updateOk = int.Parse(attr.Value);
                            } catch {
                                updateOk = 10;
                            }
                            continue;
                        }

                        if (attr.Name == "mirrorTimeout") {
                            try {
                                updateFail = int.Parse(attr.Value);
                            } catch {
                                updateFail = 60;
                            }
                            continue;
                        }

                        if (attr.Name == "pageLoadTimeout") {
                            try {
                                updateTimeout = int.Parse(attr.Value);
                            } catch {
                                updateTimeout = 20;
                            }
                            continue;
                        }

                        if (attr.Name == "username") {
                            try {
                                username = attr.Value;
                            } catch {
                                username = "";
                            }
                            continue;
                        }

                        if (attr.Name == "password") {
                            try {
                                password = attr.Value;
                            } catch {
                                password = "";
                            }
                            continue;
                        }

                        if (attr.Name == "watched") {
                            try {
                                watched = bool.Parse(attr.Value);
                            } catch {
                                watched = false;
                            }
                            continue;
                        }

                        if (attr.Name == "message") {
                            try {
                                message = attr.Value;
                            } catch {
                                message = "";
                            }
                            continue;
                        }
                    }

                    var xMirrors = xSite.Descendants("mirrors");
                    foreach (var xMirror in xMirrors.Elements()) {
                        var protocol = string.Empty;
                        var domain = string.Empty;
                        try {
                            protocol = xMirror.Attribute("protocol").Value;
                        } catch {
                            protocol = "http";
                        }

                        try {
                            domain = xMirror.Attribute("domain").Value;
                        } catch {
                            domain = "github.com";
                        }

                        mirrors.Add($"{protocol}://{domain}/");
                    }//end foreach xMirrors

                    var xWhitelist = xSite.Descendants("whitelist");
                    foreach (var xPath in xWhitelist.Elements()) {
                        var path = string.Empty;
                        try {
                            path = xPath.Attribute("uri").Value;
                        } catch { }
                        whitelist.Add(path);
                    }//end foreach whitelist

                    var siteModel = new SiteModel {
                        Name = siteName,
                        UpdateInterval = updateOk,
                        SwitchMirrorTimeout = updateFail,
                        UpdateTimeout = updateTimeout,
                        Username = username,
                        Password = password,
                        Watched = watched,
                        Mirrors = mirrors,
                        Message = message,
                        Whitelist = whitelist
                    };

                    retval.Add(siteModel);

                }//end if count > 0
            } catch (Exception ex) {
                throw new Exception($"Ошибка чтения файла конфигурации: {ex.Message}");
            }
            return retval;
        }//end PardeConfig



    }

}
/*
 * Пример xml
 * <?xml version="1.1" encoding="UTF-8" ?>
 * <sites>
 *  <site name="Кобра Гарант" updateOk="10" updateFail="60" updateTimeout="20">
 *      <mirrors>
 *          <mirror domain="ex1.example.com" protocol="https"/>
 *          <mirror domain="ex2.example.com" protocol="https"/>
 *      </mirrors>
 *      <whitelist>
 *          <path uri="index.php?r=site%2Findex"/>
 *          <path uri="index.php"/>
 *          <path uri="index.php?r=site%2Fobject-list"/>
 *      </whitelist>
 *  </site>
 * </sites>
 */
