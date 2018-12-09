using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;

namespace XingKongUtils
{
    /// <summary>
    /// 用于处理Http的Post和Get请求的工具类
    /// static版的Post方法不会保持长连接，在请求次数较少的情况下使用
    /// 实例版的Post方法会可以重用Tcp连接，在请求次数频繁的情况下使用
    /// 实例版的Post方法线程安全
    /// </summary>
    public class HttpUtils
    {
        private WebClient webClient;

        /// <summary>
        /// 本工具类的全局编码设定
        /// </summary>
        public static Encoding RequestEncoding;

        /// <summary>
        /// 默认的UserAgent
        /// </summary>
        public static readonly string DefaultUserAgent = "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";

        private static readonly char[] _semicolonSpliter = new char[] { ';' };
        private static readonly char[] _equalSpliter = new char[] { '=' };

        private static void CheckEncoding()
        {
            if (RequestEncoding == null)
            {
                RequestEncoding = Encoding.UTF8;
            }
        }

        /// <summary>
        /// Post请求的数据格式
        /// </summary>
        public enum RequestType
        {
            Json,
            Form,
            Raw
        }

        /// <summary>
        /// 创建一个HTTP请求
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="parameters">要Post的参数，可以为string、byte[]或IDictionary &lt;string, string&gt;</param>
        /// <param name="type">Post请求的数据格式</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="userAgent">UserAgent字符串</param>
        /// <param name="cookies">如果没有身份验证信息的话这里可以为null</param>
        /// <returns>HTTP请求</returns>
        private static HttpWebResponse CreatePostHttpResponse(string url, object parameters, RequestType type, int? timeout, string userAgent, CookieCollection cookies, Action<HttpWebRequest> preRequestHandler = null)
        {
            //检查各项参数
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentNullException("url");
            }

            CheckEncoding();

            byte[] data = GetBytes(parameters, type);


            HttpWebRequest request = null;
            //如果是发送HTTPS请求  
            if (url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
            {
                ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version10;
            }
            else
            {
                request = WebRequest.Create(url) as HttpWebRequest;
            }
            request.Method = "POST";
            switch (type)
            {
                case RequestType.Json:
                    request.ContentType = "application/json";
                    break;
                case RequestType.Form:
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                case RequestType.Raw:
                    request.ContentType = "application/x-www-form-urlencoded";
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(userAgent))
            {
                request.UserAgent = userAgent;
            }
            else
            {
                request.UserAgent = DefaultUserAgent;
            }
            if (timeout.HasValue)
            {
                request.Timeout = timeout.Value;
            }
            if (cookies != null)
            {
                request.CookieContainer = new CookieContainer();
                request.CookieContainer.Add(cookies);
            }
            //处理自定义Request
            preRequestHandler?.Invoke(request);

            //向服务器发送数据
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            //获得服务器响应
            var response = request.GetResponse() as HttpWebResponse;
            //保存服务器返回的Cookie
            ProcessHttpSetCookie(response);
            return response;
        }

        /// <summary>
        /// 将参数转换成指定类型的byte数组
        /// </summary>
        /// <param name="parameters">参数，可以为string、byte[]或IDictionary &lt;string, string&gt;</param>
        /// <param name="type">要转换的目标格式</param>
        /// <returns></returns>
        private static byte[] GetBytes(object parameters, RequestType type)
        {
            byte[] data = null;
            //确定要POST的数据的类型
            if (parameters != null)
            {
                switch (type)
                {
                    case RequestType.Json:
                        if (parameters is IDictionary<string, string>)
                        {
                            data = GetJsonBytes(parameters as IDictionary<string, string>);
                        }
                        else if (parameters is string)
                        {
                            data = Encoding.UTF8.GetBytes(parameters as string);
                        }
                        else
                        {
                            throw new ArgumentException("parameters");
                        }
                        break;
                    case RequestType.Form:
                        if (parameters is IDictionary<string, string>)
                        {
                            data = GetFormBytes(parameters as IDictionary<string, string>);
                        }
                        else
                        {
                            throw new ArgumentException("parameters");
                        }
                        break;
                    case RequestType.Raw:
                        if (parameters is byte[])
                        {
                            data = parameters as byte[];
                        }
                        else if (parameters is string)
                        {
                            data = RequestEncoding.GetBytes(parameters as string);
                        }
                        else
                        {
                            throw new ArgumentException("parameters");
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                throw new ArgumentException("parameters");
            }
            return data;
        }

        /// <summary>
        /// 将给定的参数转换成表单提交的字符串格式
        /// 再转换成byte数组
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static byte[] GetFormBytes(IDictionary<string, string> parameters)
        {
            StringBuilder buffer = new StringBuilder();
            int i = 0;
            foreach (string key in parameters.Keys)
            {
                if (i > 0)
                {
                    buffer.AppendFormat("&{0}={1}", key, parameters[key]);
                }
                else
                {
                    buffer.AppendFormat("{0}={1}", key, parameters[key]);
                }
                i++;
            }
            CheckEncoding();
            return RequestEncoding.GetBytes(buffer.ToString());
        }

        /// <summary>
        /// 将给定的参数转换成Json的字符串格式
        /// 再转换成byte数组
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static byte[] GetJsonBytes(IDictionary<string, string> parameters)
        {
            StringBuilder buffer = new StringBuilder();
            int i = 0;
            buffer.Append("{");
            foreach (string key in parameters.Keys)
            {
                if (i > 0)
                {
                    buffer.AppendFormat(",\"{0}\":\"{1}\"", key, parameters[key]);
                }
                else
                {
                    buffer.AppendFormat("\"{0}\":\"{1}\"", key, parameters[key]);
                }
                i++;
            }
            buffer.Append("}");
            CheckEncoding();
            return RequestEncoding.GetBytes(buffer.ToString());
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 简化的POST版本
        /// </summary>
        /// <param name="url">接口地址</param>
        /// <param name="parameters">接口参数</param>
        /// <param name="type">要Post的参数，可以为string、byte[]或IDictionary &lt;string, string&gt;</param>
        /// <returns>服务器返回的信息</returns>
        public static string Post(string url, object parameters, RequestType type, string userAgent = "", CookieCollection cookies = null)
        {
            return Post(url, parameters, type, out CookieCollection outputCookies, userAgent, cookies);
        }

        public static string Post(string url, object parameters, RequestType type, out CookieCollection outputCookies, string userAgent = "", CookieCollection cookies = null, Action<HttpWebRequest> preRequestHandler = null)
        {
            HttpWebResponse response = CreatePostHttpResponse(url, parameters, type, null, userAgent, cookies, preRequestHandler);
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string Html = streamReader.ReadToEnd();
            outputCookies = response.Cookies;
            return Html;
        }

        /// <summary>
        /// 支持长连接的POST版本
        /// </summary>
        /// <param name="url">接口地址</param>
        /// <param name="args">接口参数</param>
        /// <param name="type">要Post的参数，可以为string、byte[]或IDictionary &lt;string, string&gt;</param>
        /// <returns>服务器返回的信息</returns>
        public string Post_KeepAlive(string url, object args, RequestType type)
        {
            CheckEncoding();
            var encoding = Encoding.UTF8;
            if (webClient == null)
            {
                webClient = new WebClient();
                // 采取POST方式必须加的Header
                webClient.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                webClient.Headers.Add(HttpRequestHeader.KeepAlive, "TRUE");
            }

            lock (webClient)
            {
                byte[] data = GetBytes(args, type);
                byte[] responseData = webClient.UploadData(url, "POST", data); // 得到返回字符流
                return encoding.GetString(responseData);// 解码  
            }
        }

        /// <summary>
        /// GET方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>网页源代码</returns>
        public static string Get(string url, SecurityProtocolType securityType = SecurityProtocolType.Tls12, Action<HttpWebRequest> preRequestHandler = null)
        {
            return Get(url, out CookieCollection cookies, securityType, preRequestHandler);
        }

        /// <summary>
        /// GET方法
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="cookies">服务器返回的Cookies</param>
        /// <returns>网页源代码</returns>
        public static string Get(string url, out CookieCollection cookies, SecurityProtocolType securityType = SecurityProtocolType.Tls12, Action<HttpWebRequest> preRequestHandler = null)
        {
            if (url.StartsWith("https"))
            {
                if (ServicePointManager.ServerCertificateValidationCallback == null)
                {
                    ServicePointManager.ServerCertificateValidationCallback += CheckValidationResult;
                }
            }
            Uri uri = new Uri(url);
            ServicePointManager.SecurityProtocol = securityType;
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(url);
            myReq.UserAgent = "User-Agent:Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705";
            myReq.Accept = "*/*";
            myReq.KeepAlive = true;
            myReq.Headers.Add("Accept-Language", "zh-cn,en-us;q=0.5");
            myReq.Method = "GET";
            //处理自定义Request
            preRequestHandler?.Invoke(myReq);

            HttpWebResponse result = (HttpWebResponse)myReq.GetResponse();

            ProcessHttpSetCookie(result);
            cookies = result.Cookies;
            foreach (Cookie cookie in cookies)
            {
                if (string.IsNullOrWhiteSpace(cookie.Domain))
                {
                    cookie.Domain = uri.Host;
                }
            }

            Stream receviceStream = result.GetResponseStream();
            StreamReader readerOfStream = new StreamReader(receviceStream, System.Text.Encoding.GetEncoding("utf-8"));
            string strHTML = readerOfStream.ReadToEnd();
            readerOfStream.Close();
            receviceStream.Close();
            result.Close();
            return strHTML;
        }

        /// <summary>
        /// 处理HTTP头中的Set-Cookie字段
        /// </summary>
        /// <param name="response"></param>
        public static void ProcessHttpSetCookie(HttpWebResponse response)
        {
            if (response.Cookies == null)
            {
                response.Cookies = new CookieCollection();
            }
            if (response != null && response.Headers != null && response.Headers.AllKeys != null)
            {
                foreach (var header in response.Headers.AllKeys)
                {
                    if (header.Equals("Set-Cookie"))
                    {
                        Cookie cookie = GetCookieFromSetCookie(response.Headers.Get(header));
                        if (cookie != null)
                        {
                            response.Cookies.Add(cookie);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 从Set-Cookie字段中解析Cookie
        /// </summary>
        /// <param name="setCookie">HTTP头中的Set-Cookie字段</param>
        /// <returns>Cookie</returns>
        private static Cookie GetCookieFromSetCookie(string setCookie)
        {
            if (string.IsNullOrWhiteSpace(setCookie))
            {
                return null;
            }
            string[] parts = setCookie.Split(_semicolonSpliter, StringSplitOptions.RemoveEmptyEntries);
            if (parts == null)
            {
                return null;
            }

            bool _isCookieValid = false;
            Cookie cookie = new Cookie();
            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                string[] keyValuePair = part.Split(_equalSpliter, StringSplitOptions.RemoveEmptyEntries);
                if (i == 0 && keyValuePair.Length == 2)
                {
                    _isCookieValid = true;
                    cookie.Name = keyValuePair[0];
                    cookie.Value = keyValuePair[1];
                }
                else
                {
                    if (keyValuePair.Length > 0)
                    {
                        switch (keyValuePair[0])
                        {
                            case "HttpOnly":
                                cookie.HttpOnly = true;
                                break;
                            case "Secure":
                                cookie.Secure = true;
                                break;
                            case "Expires":
                                DateTime.TryParse(keyValuePair[1], out var expires);
                                cookie.Expires = expires;
                                break;
                            case "Domain":
                                cookie.Domain = keyValuePair[1];
                                break;
                            case "Path":
                                cookie.Path = keyValuePair[1];
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            if (_isCookieValid)
            {
                return cookie;
            }
            return null;
        }

        public static Dictionary<string, string> ConstructArgs()
        {
            return new Dictionary<string, string>();
        }
    }
}
