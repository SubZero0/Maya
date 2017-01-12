using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace Maya.Chatterbot
{
    internal static class ChatterBotUtils
    {
        public static string ParametersToWWWFormURLEncoded(IDictionary<string, string> parameters)
        {
            string wwwFormUrlEncoded = null;
            foreach (var parameterKey in parameters.Keys)
            {
                var parameterValue = parameters[parameterKey];
                var parameter = string.Format("{0}={1}", System.Uri.EscapeDataString(parameterKey), System.Uri.EscapeDataString(parameterValue));
                if (wwwFormUrlEncoded == null)
                {
                    wwwFormUrlEncoded = parameter;
                }
                else
                {
                    wwwFormUrlEncoded = string.Format("{0}&{1}", wwwFormUrlEncoded, parameter);
                }
            }
            return wwwFormUrlEncoded;
        }

        public static string MD5(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = System.Security.Cryptography.MD5.Create();
            var inputBytes = Encoding.ASCII.GetBytes(input);
            var hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            var sb = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();

        }

        public async static Task<CookieCollection> GetCookiesAsync(string url)
        {
            CookieContainer container = new CookieContainer();
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.Headers["UserAgent"] = "Mozilla/5.0 (Windows NT 6.3; WOW64; rv:28.0) Gecko/20100101 Firefox/28.0;";
            request.ContentType = "text/html";
            request.CookieContainer = container;

            var response = (HttpWebResponse)await request.GetResponseAsync();
            using (var responseStreamReader = new StreamReader(response.GetResponseStream()))
            {
                responseStreamReader.ReadToEnd();
            }

            return container.GetCookies(request.RequestUri);
        }

        public async static Task<string> PostAsync(string url, IDictionary<string, string> parameters, CookieCollection cookies)
        {
            var postData = ParametersToWWWFormURLEncoded(parameters);
            var postDataBytes = Encoding.ASCII.GetBytes(postData);

            var request = (HttpWebRequest)WebRequest.Create(url);

            if (cookies != null)
            {
                var container = new CookieContainer();
                foreach (Cookie c in cookies)
                    container.Add(request.RequestUri, c);
                request.CookieContainer = container;
            }

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            //request.ContentLength = postDataBytes.Length;

            using (var outputStream = await request.GetRequestStreamAsync())
            {
                outputStream.Write(postDataBytes, 0, postDataBytes.Length);
            }

            var response = (HttpWebResponse)await request.GetResponseAsync();
            using (var responseStreamReader = new StreamReader(response.GetResponseStream()))
            {
                return responseStreamReader.ReadToEnd().Trim();
            }
        }

        public static string XPathSearch(string input, string expression)
        {
            var document = new XPathDocument(new MemoryStream(Encoding.ASCII.GetBytes(input)));
            var navigator = document.CreateNavigator();
            return navigator.SelectSingleNode(expression).Value.Trim();
        }

        public static string StringAtIndex(string[] strings, int index)
        {
            if (index >= strings.Length) return "";
            return strings[index];
        }
    }
}
