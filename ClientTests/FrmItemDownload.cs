using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FlyingFive;

namespace ClientTests
{

    public static class Util
    {
        private static bool RemoteCertificateValidate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors error)
        {
            return true;
        }

        public static void SetCertificatePolicy()
        {
            ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, new RemoteCertificateValidationCallback(Util.RemoteCertificateValidate));
        }
    }

    public class Downloader
    {
        public int WorkerId { get; set; }
        public int StartIndex { get; set; }

        public Action<string> DisplayMsg { get; set; }
        public void GetString()
        {
            //if (!string.IsNullOrEmpty(this.textBox1.Text))
            //{
            //    response = this.GetResponse(this.textBox1.Text);
            //}
            //Thread.Sleep(1000);
            ip_address = CreateRandomIP();
            while (true)
            {
                var key = string.Format("69{0}{1}", WorkerId.ToString(), StartIndex.ToString().PadLeft(13 - 2 - WorkerId.ToString().Length, '0'));
                var response = GetResponse(key);
                if (!string.IsNullOrEmpty(response))
                {
                    response = NoHTML(response);
                    if (DisplayMsg != null) { DisplayMsg(response); }
                }
                else
                {
                    StartNewThread();
                    break;
                }
                Thread.Sleep(100);
                StartIndex++;
            }
        }

        private void StartNewThread()
        {
            var x = new Downloader() { WorkerId = this.WorkerId, StartIndex = this.StartIndex, DisplayMsg = this.DisplayMsg };
            var t = new Thread(x.GetString);
            t.IsBackground = true;
            t.Start();
        }

        public static string NoHTML(string Htmlstring)
        {
            Htmlstring = Regex.Replace(Htmlstring, "<script[^>]*?>.*?</script>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"(\<script(.+?)\</script\>)|(\<style(.+?)\</style\>)", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"(\<li(.+?)\</li\>)|(\<p(.+?)\</p\>)|(\<title(.+?)\</title\>)|(\<button(.+?)\</button\>)", "", RegexOptions.IgnoreCase);
            Htmlstring = new Regex("<.+?>", RegexOptions.IgnoreCase).Replace(Htmlstring, "");
            Htmlstring = Regex.Replace(Htmlstring, "<(.[^>]*)>", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"([\r\n])[\s]+", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "-->", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "<!--.*", "", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(quot|#34);", "\"", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(amp|#38);", "&", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(lt|#60);", "<", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(gt|#62);", ">", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(nbsp|#160);", "   ", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(iexcl|#161);", "\x00a1", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(cent|#162);", "\x00a2", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(pound|#163);", "\x00a3", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, "&(copy|#169);", "\x00a9", RegexOptions.IgnoreCase);
            Htmlstring = Regex.Replace(Htmlstring, @"&#(\d+);", "", RegexOptions.IgnoreCase);
            Htmlstring.Replace("<", "");
            Htmlstring.Replace(">", "");
            return Htmlstring;
        }

        private string ip_address = "";
        public string GetResponse(string key)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.TryAddWithoutValidation("x-forward-for", ip_address);
            //check
            httpClient.DefaultRequestHeaders.Referrer = new Uri("http://www.gds.org.cn/");
            string urlCheck = "http://search.anccnet.com/writeSession.aspx?responseResult=check_ok&rn=" + DateTime.Now.ToString("yyyyMMddHHmmssms").MD5();
            HttpResponseMessage response = httpClient.GetAsync(urlCheck).Result;
            var statusCode = response.StatusCode.ToString();
            if (statusCode == "OK")
            {
                //查找东西
                sleepSeconds = 1;
                string strUrl = String.Format("http://search.anccnet.com/searchResult2.aspx?keyword={0}", key, DateTime.Now.ToString("yyyyMMddHHmmssms").MD5());
                var response2 = httpClient.GetAsync(strUrl).Result;
                string strTemp = response2.Content.ReadAsStringAsync().Result;
                return strTemp;//= strTemp).Replace("\r\n","");
            }
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                DisplayMsg("拒绝请求。");
                ip_address = CreateRandomIP();
                Thread.Sleep(65 * 1000);
                sleepSeconds++;
                return "";
            }
            return "";
        }

        private string CreateRandomIP()
        {
            var rndA = new Random().Next(1, int.MaxValue);
            Thread.Sleep(10);
            var rndB = new Random().Next(1, int.MaxValue);
            Thread.Sleep(20);
            var rndC = new Random().Next(1, int.MaxValue);
            Thread.Sleep(30);
            var rndD = new Random().Next(1, int.MaxValue);
            Thread.Sleep(40);
            var a = new Random(rndA).Next(1, 254);
            Thread.Sleep(50);
            var b = new Random(rndB).Next(2, 254);
            Thread.Sleep(60);
            var c = new Random(rndC).Next(4, 254);
            Thread.Sleep(70);
            var d = new Random(rndD).Next(5, 254);
            var ip = string.Format("{0}.{1}.{2}.{3}", a, b, c, d);
            Console.WriteLine(string.Format("random ip address:{0}", ip));
            return ip;
        }

        private int sleepSeconds = 1;

        public string GetResponse2(string key = "06944127305584")
        {
            HttpClient client = new HttpClient
            {
                DefaultRequestHeaders = { Referrer = new Uri("http://www.gds.org.cn/") }
            };
            //client.DefaultRequestHeaders.TryAddWithoutValidation()
            string requestUri = "http://search.anccnet.com/writeSession.aspx?responseResult=check_ok";
            if (client.GetAsync(requestUri).Result.StatusCode.ToString() == "OK")
            {
                string str3 = string.Format("http://search.anccnet.com/searchResult2.aspx?keyword={0}", key);
                return client.GetAsync(str3).Result.Content.ReadAsStringAsync().Result;
            }
            return "";
        }
    }

    public partial class FrmItemDownload : Form
    {
        private List<Downloader> downloaders = new List<Downloader>();
        public FrmItemDownload()
        {
            InitializeComponent();
            //var dir = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);
            Util.SetCertificatePolicy();
            this.Load += FrmItemDownload_Load;
            for (int i = 0; i < 1; i++)
            {
                downloaders.Add(new Downloader() { WorkerId = i, StartIndex = 0, DisplayMsg = new Action<string>(this.DisplayMsg) });
            }
        }

        private void DisplayMsg(string msg)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(this.DisplayMsg), new object[] { msg });
            }
            else
            {
                txtContent.AppendText(string.Format("[{0}]{1}{2}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), msg, Environment.NewLine));
                txtContent.ScrollToCaret();
            }
        }

        private void FrmItemDownload_Load(object sender, EventArgs e)
        {

            foreach (var item in downloaders)
            {
                var t = new Thread(item.GetString);
                t.Start();
            }
        }


    }

}
