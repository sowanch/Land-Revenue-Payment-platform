using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace TangaDummy.Safaricom
{
    public class oathAccount
    {
        private string _token;
        private string _consumerKey;
        private string _consumerSecret;


        public string Token
        {
            get
            {
                return _token;
            }
        }

        public string Consumerkey
        {
            get
            {
                return _consumerKey;
            }
            set
            {
                _consumerKey = value;
            }
        }

        public string Consumersecret
        {
            get
            {
                return _consumerSecret;
            }
            set
            {
                _consumerSecret = value;
            }
        }

        public oathAccount(string consumerKey, string consumerSecret)
        {
            _consumerKey = consumerKey;
            _consumerSecret = consumerSecret;
            _token = GetToken(consumerKey, consumerSecret);
        }



        public string GetToken(string consumerKey, string consumerSecret)
        {

            var data = new NameValueCollection();
            data["grant_type"] = "client_credentials";
            String appKeySecret = consumerKey + ":" + consumerSecret;
            data["Authorization"] = Convert.ToBase64String(Encoding.ASCII.GetBytes(appKeySecret));

            TokenInfo x = JsonConvert.DeserializeObject<TokenInfo>(_getResponse(data, "https://sandbox.safaricom.co.ke/oauth/v1/generate?grant_type=client_credentials"));
            return x.access_token;
        }

        private string _getResponse(NameValueCollection data, string url)
        {
            string responseData;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Headers[HttpRequestHeader.Authorization] = string.Format("Basic {0}", data["Authorization"]);

            var response = (HttpWebResponse)request.GetResponse();

            responseData = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return responseData;
        }

        public class TokenInfo
        {
            public string access_token { get; set; }
            public long expires_in { get; set; }
            public bool ssl { get; set; }
        }
    }
}