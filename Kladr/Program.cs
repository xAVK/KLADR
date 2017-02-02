using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft;
using System.IO;
using System.Globalization;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Threading;
using CreteElementAndWriteDB;
using Hangfire;
using Hangfire.SqlServer;

namespace Kladr
{
    class Program
    {
        static void Main(string[] args)
        {
            //using (DbElementContext db = new DbElementContext())
            //{
            //    Regex rg = new Regex("Курская");
            //    List<DbElement> list = new List<DbElement>();
            //    foreach (var item in db.Buldings)
            //    {
            //        if (rg.IsMatch(item.Street))
            //        {
            //            list.Add(item);
            //        }
            //    }
            //}
            WebClient client = new WebClient();
            client.Encoding = Encoding.ASCII;
            BackgroundJobClient bj;
            GlobalConfiguration.Configuration.UseSqlServerStorage("Data Source=.\\SQLEXPRESS;Initial Catalog=HangFireTest;Integrated Security=True;MultipleActiveResultSets=True");
            bj = new BackgroundJobClient();
            string Begin = "";
            #region
            //string[] alphabet = { "А", "а", "Б", "б", "В", "в", "Г", "г", "Д", "д", "Е", "е", "Ё", "ё", "Ж", "ж", "З", "з", "И", "и", "Й", "й", "К", "к", "Л", "л", "М", "м", "Н", "н", "О", "о", "П", "п", "Р", "р", "С", "с", "Т", "т", "У", "у", "Ф", "ф", "Х", "х", "Ц", "ц", "Ч", "ч", "Ш", "ш", "Щ", "щ", "Ъ", "ъ", "Ы", "ы", "Ь", "ь", "Э", "э", "Ю", "ю", "Я", "я","1","2","3","4","5","6","7","8","9","0" };
            string[] alphabet = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", " ", "." };
            #endregion
            string RegionQuery = String.Format(CultureInfo.CurrentCulture, $"http://kladr-api.ru/api.php?query=Московская&contentType=region");
            string RegionResponse = "";
            try
            {
                RegionResponse = client.DownloadStringTaskAsync(new Uri(RegionQuery)).Result;
            }
            catch (Exception)
            {
                Thread.Sleep(10000);
                RegionResponse = client.DownloadStringTaskAsync(new Uri(RegionQuery)).Result;
            }
            List<Element> regions = new List<Element>();
            Get getElement = new Get();
            regions = getElement.GetListElement(RegionResponse);
            List<Thread> threads = new List<Thread>();
            foreach (var region in regions)
            {
                List<Element> citys = new List<Element>();
                Get getCity = new Get();
                citys = getCity.GetAllCity(0, region.ID);
                foreach (var city in citys)
                {
                    Console.WriteLine($"{city.TypeShort}. {city.Name} start");
                    threads.Add(new Thread(() =>
                    {
                        Thread.CurrentThread.Name = $"{city.TypeShort}. {city.Name}";
                        foreach (var beg in alphabet)
                        {
                            Begin = beg;
                            int offset = 0;
                            List<Element> streets = new List<Element>();
                            Get getStreet = new Get();
                            streets = getStreet.GetAllStreets(Begin, offset, city.ID);
                            foreach (var item in streets)
                            {
                                _task tsk = new _task();
                                try
                                {
                                    bj.Enqueue(() => tsk.GO(item, city, region));
                                }
                                catch (Exception)
                                {
                                    Thread.Sleep(10000);
                                    bj.Enqueue(() => tsk.GO(item, city, region));
                                }
                            }
                        }
                    }));
                    foreach (var thread in threads)
                    {
                        if (thread.ThreadState == ThreadState.Unstarted)
                        {
                            thread.Start();
                        }
                    }
                    if (threads.Count() > 20)
                    {
                        while (!threads.Any((t) => t.ThreadState == ThreadState.Running))
                        {

                        }
                        List<Thread> tmp = new List<Thread>();
                        foreach (var thread in threads)
                        {
                            if (thread.ThreadState == ThreadState.Running)
                            {
                                tmp.Add(thread);
                            }
                            else
                            {
                                Console.WriteLine($"{thread.Name} Done");
                                thread.Abort();
                                thread.Join(1000);
                            }
                        }
                        threads.Clear();
                        foreach (var item in tmp)
                        {
                            threads.Add(item);
                        }
                    }
                }
            }
        }
        static string[] GetBegin()
        {
            List<string> result = new List<string>();
            string[] alphabet = { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", " ", "." };

            foreach (var item in alphabet)
            {
                result.Add(item);
                foreach (var str in alphabet)
                {
                    if (str != item)
                    {
                        result.Add(item + str);
                    }
                    if (str != ".")
                    {
                        foreach (var ch in alphabet)
                        {
                            result.Add(item + str + ch);
                        }
                    }
                    else
                    {
                        result.Add(item + str + " ");
                    }
                }
            }

            return result.ToArray();
        }

    }
    public class Get
    {
        public List<Element> GetListElement(string input)
        {
            Regex reg = new Regex("[u]{1}[0-9]{4}|[u]{1}([0-9]{3}){1}[a-z]{1}");
            MatchCollection matches;
            matches = reg.Matches(input);
            for (int i = 0; i < matches.Count; i++)
            {
                string str = matches[i].Value;
                string ch = Normalize(str);
                input = input.Replace("\\" + str, ch);
            }
            string[] separator = { "result\":" };
            string result = input.Split(separator, StringSplitOptions.RemoveEmptyEntries)[1];
            result = result.Remove(result.Length - 1);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Element>>(result);
        }
        public List<Element> GetAllStreets(string Begin, int offset, string cityID)
        {
            string street = String.Format(CultureInfo.CurrentCulture, $"http://kladr-api.ru/api.php?query={Begin}&contentType=street&cityId={cityID}&limit=100&offset={offset.ToString()}");
            string res = "";
            WebClient clientStreet = new WebClient();
            clientStreet.Encoding = Encoding.ASCII;
            try
            {
                while (clientStreet.IsBusy)
                {

                }
                res = clientStreet.DownloadStringTaskAsync(new Uri(street)).Result;
            }
            catch (Exception)
            {
                Thread.Sleep(10000);
                while (clientStreet.IsBusy)
                {

                }
                res = clientStreet.DownloadStringTaskAsync(new Uri(street)).Result;
            }
            List<Element> streets = new List<Element>();
            streets = GetListElement(res);
            if (streets != null && streets.Count() > 0)
            {
                offset += 100;
                var list = GetAllStreets(Begin, offset, cityID);
                foreach (var item in list)
                {
                    streets.Add(item);
                }
            }
            return streets;
        }
        public List<Element> GetAllCity(int offset, string regionID)
        {
            WebClient clientCity = new WebClient();
            clientCity.Encoding = Encoding.ASCII;
            string CityQuery = String.Format(CultureInfo.CurrentCulture, $"http://kladr-api.ru/api.php?query=&contentType=city&regionId={regionID}&limit=100&offset={offset.ToString()}");
            string Response = "";
            try
            {
                while (clientCity.IsBusy)
                {

                }
                Response = clientCity.DownloadStringTaskAsync(new Uri(CityQuery)).Result;
            }
            catch (Exception)
            {
                Thread.Sleep(10000);
                while (clientCity.IsBusy)
                {

                }
                Response = clientCity.DownloadStringTaskAsync(new Uri(CityQuery)).Result;
            }
            List<Element> citys = new List<Element>();
            citys = GetListElement(Response);
            if (citys != null && citys.Count() > 0)
            {
                offset += 100;
                var list = GetAllCity(offset, regionID);
                foreach (var item in list)
                {
                    citys.Add(item);
                }
            }
            return citys;
        }
        public string Normalize(string input)
        {

            switch (input)
            {
                case "u0410": return "А";
                case "u0430": return "а";
                case "u0411": return "Б";
                case "u0431": return "б";
                case "u0412": return "В";
                case "u0432": return "в";
                case "u0413": return "Г";
                case "u0433": return "г";
                case "u0414": return "Д";
                case "u0434": return "д";
                case "u0415": return "Е";
                case "u0435": return "е";
                case "u0401": return "Ё";
                case "u0451": return "ё";
                case "u0416": return "Ж";
                case "u0436": return "ж";
                case "u0417": return "З";
                case "u0437": return "з";
                case "u0418": return "И";
                case "u0438": return "и";
                case "u0419": return "Й";
                case "u0439": return "й";
                case "u041a": return "К";
                case "u043a": return "к";
                case "u041b": return "Л";
                case "u043b": return "л";
                case "u041c": return "М";
                case "u043c": return "м";
                case "u041d": return "Н";
                case "u043d": return "н";
                case "u041e": return "О";
                case "u043e": return "о";
                case "u041f": return "П";
                case "u043f": return "п";
                case "u0420": return "Р";
                case "u0440": return "р";
                case "u0421": return "С";
                case "u0441": return "с";
                case "u0422": return "Т";
                case "u0442": return "т";
                case "u0423": return "У";
                case "u0443": return "у";
                case "u0424": return "Ф";
                case "u0444": return "ф";
                case "u0425": return "Х";
                case "u0445": return "х";
                case "u0426": return "Ц";
                case "u0446": return "ц";
                case "u0427": return "Ч";
                case "u0447": return "ч";
                case "u0428": return "Ш";
                case "u0448": return "ш";
                case "u0429": return "Щ";
                case "u0449": return "щ";
                case "u042a": return "Ъ";
                case "u044a": return "ъ";
                case "u042b": return "Ы";
                case "u044b": return "ы";
                case "u042c": return "Ь";
                case "u044c": return "ь";
                case "u042d": return "Э";
                case "u044d": return "э";
                case "u042e": return "Ю";
                case "u044e": return "ю";
                case "u042f": return "Я";
                case "u044f": return "я";
                default: return null;
            }
        }
    }
}
