using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;
using System.Threading;

namespace Wechat
{
    public class Wechat
    {
        private CookieContainer cookieContainer;

        private string uuid;

        private PictureBox qrcode_img;

        private string statusString;

        private String base_url;

        private String redirect_url;

        private String skey;

        private String wxsid;

        private String wxuin;

        private String pass_ticket;

        private string synckey;

        private Dictionary<string, dynamic> synckeyObj;

        private String deviceId = "e000000000000021";

        private Dictionary<string, dynamic> self;

        private Thread m_syncThread;

        private string user_agnet = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36";

        private static char[] constant =
             {
        '0','1','2','3','4','5','6','7','8','9',
        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
        'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'
      };
        public static string GenerateRandomNumber(int Length)
        {
            System.Text.StringBuilder newRandom = new System.Text.StringBuilder(62);
            Random rd = new Random();
            for (int i = 0; i < Length; i++)
            {
                newRandom.Append(constant[rd.Next(62)]);
            }
            return newRandom.ToString();
        }


        public Wechat(PictureBox codeShow) {
            cookieContainer = new CookieContainer();
            qrcode_img = codeShow;
            deviceId = "e" + GenerateRandomNumber(16);
        }

        private string getTimeStamp()
        {
            return getTimeStampInt32().ToString();
        }

        private Int32 getTimeStampInt32() {
            return Convert.ToInt32((DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds);
        }

        private bool GetUUID() {
            var url = String.Format("https://login.weixin.qq.com/jslogin?appid=wx782c26e4c19acffb&fun=new&lang=zh_CN&_={0}", getTimeStamp());
            var responseText = "";
            Get(url, ref responseText);
            Console.WriteLine(responseText);
            var regex = @"window.QRLogin.code = (\d+); window.QRLogin.uuid = ""(\S+?)""";
            var r = new Regex(regex, RegexOptions.IgnoreCase);
            Match m = r.Match(responseText);
            if (m.Success) {
                var code = m.Groups[1].Value;
                uuid = m.Groups[2].Value;
                Console.WriteLine(code);
                Console.WriteLine(uuid);

                if (code == "200")
                {
                    return true;
                }
            }
            return false;
        }

        private HttpWebResponse Get(string url, ref string responseText,int timeout = 10) {
            try
            {
                var http = WebRequest.CreateHttp(url) as HttpWebRequest;
                http.Method = "GET";
                http.Timeout = timeout * 1000;
                http.CookieContainer = cookieContainer;
                http.UserAgent = user_agnet;
                http.Proxy = null;
                HttpWebResponse response = http.GetResponse() as HttpWebResponse;
                using (var reader = new StreamReader(response.GetResponseStream(), new UTF8Encoding(true, true)))
                {
                    responseText = reader.ReadToEnd();
                }
                http.Abort();
                return response;
            }
            catch (WebException e) {
                throw e;
            }
            return null;
        }


        private HttpWebResponse GetImg(string url)
        {
            var http = WebRequest.Create(url) as HttpWebRequest;
            http.Method = "Get";
            http.CookieContainer = cookieContainer;
            HttpWebResponse response = http.GetResponse() as HttpWebResponse;
            return response;
        }

        private HttpWebResponse Post(string url,string data, ref Dictionary<string,dynamic> json,int timeout = 10)
        {
            try
            {
                byte[] byteArray = Encoding.UTF8.GetBytes(data);
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.CookieContainer = cookieContainer;
                request.ContentType = "application/json; charset=UTF-8";
                request.Method = "POST";
                request.ContentLength = byteArray.Length;
                request.UserAgent = user_agnet;
                request.Timeout = timeout * 10000;
                request.ServicePoint.Expect100Continue = false;
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                string serverResponse = "";
                using (var responseStream = response.GetResponseStream())
                {
                    if (responseStream != null)
                    {
                        using (var responseStreamReader = new StreamReader(responseStream))
                        {
                            serverResponse = responseStreamReader.ReadToEnd();
                        }
                    }
                }
                request.Abort();
                var deserializer = new JavaScriptSerializer();
                json = deserializer.Deserialize<Dictionary<string, dynamic>>(serverResponse);
                return response;
            }
            catch (WebException e) {
                throw e;
            }
 
        }

        //显示微信登录二维码
        private void showQRImage()
        {
            var url = String.Format("https://login.weixin.qq.com/qrcode/" + uuid + "?t=webwx&_={0}", getTimeStamp());

            var response = GetImg(url);
         
            using (Stream stream = response.GetResponseStream())
            {
                Image image = Image.FromStream(stream); // should catch timeout exception                   
                qrcode_img.Invoke(new Action(() =>
                {
                    qrcode_img.Image = image;
                    qrcode_img.Height = image.Height;
                    qrcode_img.Width = image.Width;
                }));
            }
        }

        //加载登录
        private String waitForLogin()
        {
            var url = String.Format("https://login.weixin.qq.com/cgi-bin/mmwebwx-bin/login?tip={0}&uuid={1}&_={2}", 0, uuid, getTimeStamp());
            string responseText = "";
            HttpWebResponse response = Get(url, ref responseText);
            Console.WriteLine(responseText);
            var regex = @"window.code=(\d+);";
            var r = new Regex(regex, RegexOptions.None);
            Match m = r.Match(responseText);
            var code = m.Groups[1].Value;

            foreach (Cookie cookie in response.Cookies)
            {
                Console.WriteLine(String.Format("Cookie_in_wait_login: {0}, {1}", cookie.Name, cookie.Value));
            }

            if (code == "201")
            {
                statusString = "扫描成功,请在手机上点击确认以登录";
            }
            else if (code == "200")
            {
                statusString = "正在登录";

                regex = @"window.redirect_uri=""(\S+?)"";";
                r = new Regex(regex, RegexOptions.None);
                m = r.Match(responseText);
                redirect_url = m.Groups[1] + "&fun=new";
                for (int i = redirect_url.Length - 1; i >= 0; i--)
                {
                    if (redirect_url[i] == '/')
                    {
                        base_url = redirect_url.Substring(0, i + 1);
                        break;
                    }
                }
            }
            else if (code == "408")
            {
                // do nothing
            }
            return code;
        }

        //登录微信
        private bool login()
        {
            try {
                string responseText = "";
                HttpWebResponse response = Get(redirect_url, ref responseText);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(responseText);
                XmlElement root = doc.DocumentElement;
                skey = root.SelectSingleNode("skey").InnerText;
                wxsid = root.SelectSingleNode("wxsid").InnerText;
                wxuin = root.SelectSingleNode("wxuin").InnerText;
                pass_ticket = root.SelectSingleNode("pass_ticket").InnerText;
                foreach (Cookie cookie in response.Cookies)
                {
                    Console.WriteLine(String.Format("Cookie_in_login: {0}: {1}", cookie.Name, cookie.Value));
                }

                if (String.IsNullOrEmpty(skey) || String.IsNullOrEmpty(wxsid) || String.IsNullOrEmpty(wxuin) || String.IsNullOrEmpty(pass_ticket))
                {
                    return false;
                }
                return true;
            }
            catch(Exception e){
                Console.WriteLine(e.Message);
            }
            return false;
        }

        //初始化微信协议
        private bool webwxinit()
        {
            var url = base_url + String.Format("webwxinit?pass_ticket={0}&skey={1}&r={2}", pass_ticket, skey, getTimeStamp());
            var serializer = new JavaScriptSerializer();
            var base_req_param = new { Uin = Int64.Parse(wxuin), Sid = wxsid, Skey = skey, DeviceID = deviceId };
            var BaseRequest = serializer.Serialize(new { BaseRequest = base_req_param });
            Console.WriteLine(String.Format("BaseRequest: {0}", BaseRequest));
            var dic = new Dictionary<string, dynamic>();
            Post(url, BaseRequest, ref dic);
            Console.WriteLine(dic);
            self = dic["User"];
            this.synckeyObj = dic["SyncKey"];
            synckey = "";
            foreach(var obj in synckeyObj["List"]) {
                synckey += obj["Key"] + "_" + obj["Val"] + "|";
            }

            synckey = synckey.Substring(0, synckey.Length - 1);

            var ErrMsg = dic["BaseResponse"]["ErrMsg"];
            if (ErrMsg.Length > 0)
            {
                
            }

            if (dic["BaseResponse"]["Ret"] != 0)
            {
                return false;
            }

            return true;
        }

        //获取联系人
        private List<Dictionary<string, dynamic>> webwxgetcontact()
        {
            string url = base_url + String.Format("webwxgetcontact?pass_ticket={0}&skey={1}&r={2}", pass_ticket, skey, getTimeStamp());
            HttpWebRequest http = WebRequest.Create(url) as HttpWebRequest;
#if (DEBUG)
            //Uri target = new Uri("https://wx.qq.com");
            //cookieContainer.Add(new Cookie("mm_lang", "zh_CN") { Domain = target.Host });
            //cookieContainer.Add(new Cookie("webwx_data_ticket", "AQcsqhDcQuYzt1F1QvsvBV9W") { Domain = target.Host });
            //cookieContainer.Add(new Cookie("wxuin", "2622149902") { Domain = target.Host });
            //cookieContainer.Add(new Cookie("wxsid", "ZHu/WOz9i7GZXcFN") { Domain = target.Host });
            //cookieContainer.Add(new Cookie("wxloadtime", "1452642551") { Domain = target.Host });
            //cookieContainer.Add(new Cookie("webwxuvid", "20e90937e5f3173d94c75ee2c3805a26a0da27cb6eb2a7f692f9b26c09eb4a2d68ac2e5055bac129bc2e653ddc9b3312") { Domain = target.Host });
#endif
            http.CookieContainer = cookieContainer;
            http.ContentType = "application/json; charset=UTF-8";
            http.Method = "GET";
            var dic = new Dictionary<string, dynamic>();
            HttpWebResponse response = Post(url, "",ref dic);
            List<Dictionary<string, dynamic>> memberlist = new List<Dictionary<string, dynamic>>(dic["MemberList"].
                                                           ToArray(typeof(Dictionary<string, dynamic>)));
            var special_users = new List<string>()
            {
                "newsapp",
                "fmessage",
                "filehelper",
                "weibo",
                "qqmail",
                "tmessage",
                "qmessage",
                "qqsync",
                "floatbottle",
                "lbsapp", "shakeapp","medianote", "qqfriend", "readerapp",
                "blogapp", "facebookapp","masssendapp", "meishiapp", "feedsapp", "voip", "blogappweixin","weixin",
                "brandsessionholder", "weixinreminder","wxid_novlwrv3lqwv11", "gh_22b87fa7cb3c", "officialaccounts","notification_messages",
                "wxitil", "userexperience_alarm"
            };

            for (int i = memberlist.Count - 1; i > -1; i--)
            {
                Dictionary<string, dynamic> member = memberlist[i];
                if ((8 & Convert.ToInt32(member["VerifyFlag"])) != 0)
                {
                    memberlist.Remove(member);
                }
                else if (special_users.Contains(member["UserName"]))
                {
                    memberlist.Remove(member);
                }
                else if (member["UserName"].Contains("@@"))
                {
                    memberlist.Remove(member);
                }
                else if (member["UserName"] == self["UserName"])
                {
                    memberlist.Remove(member);
                }
            }

            return memberlist;

        }

        public bool AutoLogin() {
            if (!GetUUID()) {
                return false;
            }

            showQRImage();

            while (waitForLogin() != "200") ;

            if (!login()) {
                return false;
            }

            if (!webwxinit()) {
                return false;
            }

            var member_list = webwxgetcontact();
            Console.WriteLine("member_list: ");
            foreach (var member in member_list)
            {
                Console.WriteLine(member["NickName"]);
            }

            testSyncCheck();
            m_syncThread = new Thread(SyncMsgThread);
            m_syncThread.Start();
            return true;
        }

        private string syncHost;

        private bool syncCheck(ref int selector) {
            var timeStamp = getTimeStamp();
            string url = "https://" + syncHost + String.Format("/cgi-bin/mmwebwx-bin/synccheck?r={0}&sid={1}&uin={2}&skey={3}&deviceid={4}&synckey={5}&_={6}", timeStamp,wxsid,wxuin,skey,deviceId,System.Web.HttpUtility.UrlEncode(synckey),timeStamp);
            Console.WriteLine("sync check url = " + url);
            try
            {
                string responseText = "";
                var response = Get(url, ref responseText,30);
                Console.WriteLine("sync check " + responseText);
                var regex = @"window\.synccheck=\{retcode:""(\d+)"",selector:""(\d+)""\}";
                var r = new Regex(regex, RegexOptions.None);
                Match m = r.Match(responseText);
                if (m.Success)
                {
                    int retCode = int.Parse(m.Groups[1].Value);
                    selector = int.Parse(m.Groups[2].Value);
                    return retCode == 0;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message + " " +  syncHost);
            }
            return false;
        }

        private void testSyncCheck() {
            string[] hosts = new string[]{
                    "wx2.qq.com",
                    "webpush.wx2.qq.com",
                    "wx8.qq.com",
                    "webpush.wx8.qq.com",
                    "qq.com",
                    "wx.qq.com",
                    "webpush.wx8.qq.com",
                    "webpush.wx.qq.com",
                    "web2.wechat.com",
                    "webpush.web2.wechat.com",
                    "wechat.com",
                    "webpush.web.wechat.com",
                    "webpush.weixin.qq.com",
                    "webpush.wechat.com",
                    "webpush1.wechat.com",
                    "webpush2.wechat.com",
                    "webpush.wx.qq.com",
                    "webpush2.wx.qq.com" };

            findHost = false;
            
            for (var i = 0; i < hosts.Length; i++) {
                syncHost = hosts[i];
                selector = 0;
                if (syncCheck(ref selector)) {
                    findHost = true;
                    return ;
                }
            }
        }

        private int selector;

        private bool findHost = false;

        private void syncMsg() {
            string url = this.base_url + String.Format("webwxsync?sid={0}&skey={1}&pass_ticket={2}", wxsid, skey, pass_ticket);

            var serializer = new JavaScriptSerializer();
            var base_req_param = new { Uin = Int64.Parse(wxuin), Sid = wxsid, Skey = skey, DeviceID = deviceId };
            var timestamp = getTimeStampInt32();
            var payload = serializer.Serialize(new
            {
                BaseRequest = base_req_param,
                SyncKey = synckeyObj,
                rr = ~timestamp
            });

            var dict = new Dictionary<string, dynamic>();
            var response = Post(url, payload, ref dict, 10);

            var baseResponse = dict["BaseResponse"];
            Console.WriteLine("sync wx msg ret = " + baseResponse["Ret"]);
            if (baseResponse["Ret"] == 0) {
               
                synckeyObj = dict["SyncKey"];
                synckey = "";
                foreach (var obj in synckeyObj["List"])
                {
                    synckey += obj["Key"] + "_" + obj["Val"] + "|";
                }
                synckey = synckey.Substring(0, synckey.Length - 1);

                var msgList = dict["AddMsgList"];
                selector = 0;
            }
            response.Close();
        }

        private void SyncMsgThread() {
            while (true) {
                if (!findHost)
                {
                    testSyncCheck();
                }
                else {
                    if (selector > 0)
                    {
                        syncMsg();
                    }
                    syncCheck(ref selector);
                }
                Thread.Sleep(1000);
            }
        }
    }


}
