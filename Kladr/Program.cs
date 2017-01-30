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
            //    Regex rg = new Regex("Белинского");
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
            BackgroundJobClient bj;
            GlobalConfiguration.Configuration.UseSqlServerStorage("Data Source=.\\SQLEXPRESS;Initial Catalog=HangFireTest;Integrated Security=True;MultipleActiveResultSets=True");
            bj = new BackgroundJobClient();
            client.Encoding = Encoding.ASCII;
            string Begin = "";
            #region
            string[] alphabet = { "А", "а", "Б", "б", "В", "в", "Г", "г", "Д", "д", "Е", "е", "Ё", "ё", "Ж", "ж", "З", "з", "И", "и", "Й", "й", "К", "к", "Л", "л", "М", "м", "Н", "н", "О", "о", "П", "п", "Р", "р", "С", "с", "Т", "т", "У", "у", "Ф", "ф", "Х", "х", "Ц", "ц", "Ч", "ч", "Ш", "ш", "Щ", "щ", "Ъ", "ъ", "Ы", "ы", "Ь", "ь", "Э", "э", "Ю", "ю", "Я", "я","1","2","3","4","5","6","7","8","9","0" };
            #endregion
            foreach (var beg in alphabet)
            {
                Begin = beg;
                //string street = String.Format(CultureInfo.CurrentCulture, $"http://kladr-api.ru/api.php?query={Begin}&contentType=street&cityId=7400000900000");
                //string street = String.Format(CultureInfo.CurrentCulture, $"http://kladr-api.ru/api.php?query={Begin}&contentType=street&cityId=7400000100000");
                string street = String.Format(CultureInfo.CurrentCulture, $"http://kladr-api.ru/api.php?query={Begin}&contentType=street&cityId=7400000100000");
                Thread.Sleep(10000);
                var res = client.DownloadString(new Uri(street));
                Regex reg = new Regex("[u]{1}[0-9]{4}|[u]{1}([0-9]{3}){1}[a-z]{1}");
                MatchCollection matches = reg.Matches(res);
                for (int i = 0; i < matches.Count; i++)
                {
                    string str = matches[i].Value;
                    string ch = Normalize(str);
                    res = res.Replace("\\" + str, ch);
                }
                List<Element> streets = new List<Element>();
                string[] separator = { "result\":" };
                string result = res.Split(separator, StringSplitOptions.RemoveEmptyEntries)[1];
                result = result.Remove(result.Length - 1);
                streets = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Element>>(result);
                foreach (var item in streets)
                {
                    _task tsk = new _task();
                    bj.Enqueue(() => tsk.GO(item));
                }
            }                        
        }
        static string Normalize(string input)
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
