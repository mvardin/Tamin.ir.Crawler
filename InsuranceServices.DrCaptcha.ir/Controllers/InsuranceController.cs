using InsuranceServices.DrCaptcha.ir.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InsuranceServices.DrCaptcha.ir.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class InsuranceController : ControllerBase
    {
        private readonly IConfiguration Configuration;
        private readonly IMongoCollection<TPerson> personCollection;
        private readonly IMongoCollection<TUser> userCollection;
        private readonly IMongoCollection<TDastmozd> dastmozdCollection;
        private readonly IMongoCollection<TRequestLog> requestLogCollection;
        private static object Lock = new object();


        public InsuranceController(IConfiguration configuration)
        {
            Configuration = configuration;
            var mongoClient = new MongoClient(configuration.GetValue<string>("ConnectionString"));
            var mongoDatabase = mongoClient.GetDatabase(configuration.GetValue<string>("DatabaseName"));
            personCollection = mongoDatabase.GetCollection<TPerson>("TPerson");
            userCollection = mongoDatabase.GetCollection<TUser>("TUser");
            dastmozdCollection = mongoDatabase.GetCollection<TDastmozd>("TDastmozd");
            requestLogCollection = mongoDatabase.GetCollection<TRequestLog>("TRequestLog");
        }
        [HttpGet]
        public ActionResult Report(string username, string password)
        {
            TRequestLog requestLog = new TRequestLog();
            DateTime dtStart = DateTime.Now;
            try
            {
                lock (Lock)
                {

                    string passPhrase = Configuration.GetValue<string>("PassPhrase");
                    password = StringCipher.Decrypt(password, passPhrase);

                    ChromeOptions chromeOptions = new ChromeOptions();

                    string proxyString = Configuration.GetValue<string>("Proxy");
                    if (!string.IsNullOrEmpty(proxyString))
                    {
                        var proxy = new Proxy();
                        proxy.HttpProxy = proxy.SslProxy = proxyString;
                        chromeOptions.Proxy = proxy;
                    }
                    ChromeDriver chromeDriver = new ChromeDriver(chromeOptions);
                    chromeDriver.Navigate().GoToUrl("https://account.tamin.ir/auth/login");
                    Thread.Sleep(1000);

                    if (!waitFor(chromeDriver, "ورود به سامانه", 30))
                    {
                        chromeDriver.Quit();
                        return Ok(new { Status = false, Message = "زمان زیادی برای دریافت اطلاعات طول کشید، لطفا مجدد تلاش کنید" });
                    }

                    Thread.Sleep(500);
                    chromeDriver.FindElement(By.ClassName("username")).SendKeys(username);
                    Thread.Sleep(500);
                    chromeDriver.FindElement(By.ClassName("password")).SendKeys(password);
                    Thread.Sleep(500);
                    chromeDriver.FindElement(By.ClassName("login-button")).Submit();
                    Thread.Sleep(1000);

                    if (checkAuth(chromeDriver))
                    {
                        chromeDriver.Quit();
                        return Ok(new { Status = false, Message = "نام کاربری و یا رمز عبور اشتباه می باشد." });
                    }

                    chromeDriver.Navigate().GoToUrl("https://eservices.tamin.ir/view/#/salary");

                    var token = (string)chromeDriver.ExecuteScript("return localStorage.getItem('access_token')");

                    Thread.Sleep(1000);

                    var Result = SaveData(password, token);

                    chromeDriver.Quit();

                    requestLog.CreateLog = DateTime.Now;
                    requestLog.Message = string.Empty;
                    requestLog.Result = RequestLogResult.Completed;
                    requestLog.TimeElapsed = DateTime.Now.Subtract(dtStart).TotalSeconds;

                    requestLogCollection.InsertOne(requestLog);

                    return Ok(new { Status = true, Result });
                }
            }
            catch (Exception ex)
            {
                requestLog.CreateLog = DateTime.Now;
                requestLog.Message = ex.Message;
                requestLog.Result = RequestLogResult.Error;
                requestLog.TimeElapsed = DateTime.Now.Subtract(dtStart).TotalSeconds;
                requestLogCollection.InsertOne(requestLog);

                return Ok(new { Status = false, Message = ex.Message });
            }
        }
        [HttpGet]
        public ActionResult Encrypt(string plainText)
        {
            string passPhrase = Configuration.GetValue<string>("PassPhrase");
            return Ok(StringCipher.Encrypt(plainText, passPhrase));
        }
        private TResult SaveData(string password, string token)
        {
            #region Save data
            string userinfos = MakeRequests("https://eservices.tamin.ir/api/history-services/userinfos", token);
            JObject userinfosObject = JObject.Parse(userinfos);
            var userinfosObjectSelectToken = userinfosObject.SelectToken("data");
            var person = Newtonsoft.Json.JsonConvert.DeserializeObject<TPerson>(userinfosObjectSelectToken.ToString());
            person.Password = password;
            person.UpdateDate = DateTime.Now;
            person.NationalCode = person.NationalID;

            personCollection.DeleteOne(a => a.NationalCode == person.NationalID);

            personCollection.InsertOne(person);

            Thread.Sleep(1000);

            string currentuser = MakeRequests("https://eservices.tamin.ir/api/users/current-user", token);
            JObject userObject = JObject.Parse(currentuser);
            var userObjectSelectToken = userObject.SelectToken("data");
            var user = Newtonsoft.Json.JsonConvert.DeserializeObject<TUser>(userObjectSelectToken.ToString());

            userCollection.DeleteOne(a => a.NationalCode == person.NationalCode);

            userCollection.InsertOne(user);

            Thread.Sleep(1000);

            string dastmozdinfos = MakeRequests("https://eservices.tamin.ir/api/history-services/dastmozdinfos", token);
            JObject dastmozdinfoObject = JObject.Parse(dastmozdinfos);
            var dastmozdinfoObjectSelectToken = dastmozdinfoObject.SelectToken("data");
            dastmozdinfoObjectSelectToken = dastmozdinfoObjectSelectToken.SelectToken("list");
            var dastmozdList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<TDastmozd>>(dastmozdinfoObjectSelectToken.ToString());
            foreach (var dastmozd in dastmozdList)
                dastmozd.NationalCode = user.NationalCode;

            dastmozdCollection.DeleteMany(a => a.NationalCode == person.NationalCode);

            dastmozdCollection.InsertMany(dastmozdList);
            #endregion

            #region Generate Report
            TResult result = new TResult()
            {
                Name = user.FirstName,
                Family = user.LastName,
                Email = user.Email,
                Mobile = user.Mobile,
                IdentityNumber = person.IdentityNumber,
                NationalCode = user.NationalCode,
                LastSalary = 0,
                Last1YearAverageSalary = "",
                Last3MonthsAverageSalary = "",
                Last5YearInsurancePercent = "",
                TotalMonths = 0,
                TotalDays = 0
            };

            var lastYear = dastmozdList.OrderByDescending(a => a.hisyear).FirstOrDefault();
            int lastMonth = GetLastMonth(lastYear);

            result.LastSalary = Convert.ToInt64(GetSpecificField(lastYear, "hiswage" + lastMonth));
            List<Record> lastRecord = new List<Record>();

            int currentMonth = lastMonth;
            int currentYear = lastYear.hisyear;
            var currentYearItem = lastYear;

            Record record = new Record();
            record.KargahName = currentYearItem.rwshname;
            record.KargahNumber = currentYearItem.rwshid;
            record.Month = currentMonth;
            record.Salary = Convert.ToInt64(GetSpecificField(currentYearItem, "hiswage" + currentMonth));
            record.WorkDayCount = Convert.ToInt32(GetSpecificField(currentYearItem, "hismon" + currentMonth));
            record.Year = currentYear;
            lastRecord.Add(record);

            for (int i = 0; i < 70; i++)
            {
                currentMonth--;
                if (currentMonth == 0)
                {
                    currentMonth = 12;
                    currentYear--;
                    currentYearItem = dastmozdList.OrderByDescending(a => a.hisyear).Skip(1).FirstOrDefault();
                }

                record = new Record();
                record.KargahName = currentYearItem.rwshname;
                record.KargahNumber = currentYearItem.rwshid;
                record.Month = currentMonth;
                record.Salary = Convert.ToInt64(GetSpecificField(currentYearItem, "hiswage" + currentMonth));
                record.WorkDayCount = Convert.ToInt32(GetSpecificField(currentYearItem, "hismon" + currentMonth));
                record.Year = currentYear;
                lastRecord.Add(record);
            }
            result.Last3MonthsAverageSalary = lastRecord.Take(3).Select(a => a.Salary).Average().ToString("##.##");
            result.Last1YearAverageSalary = lastRecord.Take(12).Select(a => a.Salary).Average().ToString("##.##");

            long totalWorkDays = 0;
            foreach (var dastmozd in dastmozdList)
            {
                for (int month = 1; month <= 12; month++)
                {
                    totalWorkDays += Convert.ToInt64(GetSpecificField(dastmozd, "hismon" + month));
                }
            }
            result.TotalDays = totalWorkDays;
            result.TotalMonths = (totalWorkDays / 30);

            long last5MonthworkDays = lastRecord.Take(60).Select(a => a.WorkDayCount).Sum();
            long ordinaryLast5MonthWorkDays = 1830;
            var percent = ((last5MonthworkDays * 100) / ordinaryLast5MonthWorkDays);
            percent += 5;
            if (percent >= 100)
                percent = 100;
            result.Last5YearInsurancePercent = percent + " %";

            result.LastRecords = lastRecord.Take(6).ToList();
            #endregion

            return result;
        }
        private int GetLastMonth(TDastmozd lastYear)
        {
            for (int month = 12; month > 0; month--)
            {
                var monthSalary = Convert.ToInt64(GetSpecificField(lastYear, "hiswage" + month));
                if (monthSalary > 0)
                    return month;
            }
            return 0;
        }
        private string GetSpecificField(TDastmozd item, string field)
        {
            var prop = item.GetType().GetProperty(field);
            var value = prop.GetValue(item).ToString();
            return value;
        }

        private bool waitFor(ChromeDriver chromeDriver, string message, int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                if (chromeDriver.PageSource.Contains(message))
                    return true;
                Thread.Sleep(1000);
            }
            return true;
        }
        private bool checkAuth(ChromeDriver chromeDriver)
        {
            return chromeDriver.PageSource.Contains("به دلیل عدم تطابق نام کاربری با گذرواژه امکان ورود به سیستم وجود ندارد");
        }
        private string MakeRequests(string url, string token)
        {
            HttpWebResponse response;
            string responseText = string.Empty;

            if (Request_eservices_tamin_ir(out response, url, token))
            {
                responseText = ReadResponse(response);
                response.Close();
            }
            return responseText;
        }
        private static string ReadResponse(HttpWebResponse response)
        {
            using (Stream responseStream = response.GetResponseStream())
            {
                Stream streamToRead = responseStream;
                if (response.ContentEncoding == null)
                    using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                        return streamReader.ReadToEnd();

                if (response.ContentEncoding.ToLower().Contains("gzip"))
                {
                    streamToRead = new GZipStream(streamToRead, CompressionMode.Decompress);
                }
                else if (response.ContentEncoding.ToLower().Contains("deflate"))
                {
                    streamToRead = new DeflateStream(streamToRead, CompressionMode.Decompress);
                }
                else if (response.ContentEncoding.ToLower().Contains("br"))
                {

                    using (BrotliStream bs = new BrotliStream(streamToRead, CompressionMode.Decompress))
                    {
                        using (MemoryStream msOutput = new MemoryStream())
                        {
                            bs.CopyTo(msOutput);
                            msOutput.Seek(0, System.IO.SeekOrigin.Begin);
                            using (StreamReader reader = new StreamReader(msOutput, Encoding.UTF8))
                            {
                                return reader.ReadToEnd();
                            }
                        }
                    }
                }
                using (StreamReader streamReader = new StreamReader(streamToRead, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }

            }
        }
        private bool Request_eservices_tamin_ir(out HttpWebResponse response, string url, string token)
        {
            response = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

                string proxyString = Configuration.GetValue<string>("Proxy");
                if (!string.IsNullOrEmpty(proxyString))
                {
                    WebProxy proxy = new WebProxy(proxyString);
                    request.Proxy = proxy;
                }

                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/104.0.0.0 Safari/537.36";
                request.Headers.Set(HttpRequestHeader.Authorization, "Bearer " + token);
                request.Headers.Set(HttpRequestHeader.AcceptEncoding, "gzip, deflate, br");
                request.Accept = "application/json, text/plain, */*";

                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                if (e.Status == WebExceptionStatus.ProtocolError) response = (HttpWebResponse)e.Response;
                else return false;
            }
            catch (Exception)
            {
                if (response != null) response.Close();
                return false;
            }

            return true;
        }
    }
}
