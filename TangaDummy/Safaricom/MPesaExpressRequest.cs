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
    public class MPesaExpressRequest
    {

        public MPesaExpressRequest()
        {
            
        }

        public string send(int bsc, string pwd, DateTime stamp, int amount, string partyA, string partyB, string accRef, string tDesc, string token, Building nyumba)
        {
            MPesaRequest mpequest = new MPesaRequest
            {
                BusinessShortCode = bsc.ToString(),
                Password = pwd,
                Timestamp = stamp.ToString("yyyyMMddHHmmss"),
                TransactionType = "CustomerPayBillOnline",
                Amount = amount.ToString(),
                PartyA = partyA,
                PartyB = partyB,
                PhoneNumber = partyA,
                CallBackURL = "http://169.239.252.207/PostClient/",
                AccountReference = nyumba.LandRefNo.ToString(),
                TransactionDesc = tDesc
            };

            var rezponze = _getResponse(mpequest, "https://sandbox.safaricom.co.ke/mpesa/stkpush/v1/processrequest", token);

            return rezponze;
        }

        private string _getResponse(MPesaRequest data, string url, string token)
        {
            string responseData;
            var webClient = new System.Net.WebClient();
            webClient.Headers.Add("Authorization","Bearer "+  token);
            webClient.Headers.Add("Content-Type", "application/json");
            var response = webClient.UploadData(url, Encoding.Default.GetBytes(JsonConvert.SerializeObject(data)));
            responseData = System.Text.Encoding.UTF8.GetString(response);
            return responseData;
        }

        public class TokenInfo
        {
            public string MerchantRequestID { get; set; }
            public string CheckoutRequestID { get; set; }
            public string ResponseDescription { get; set; }
            public int ResponseCode { get; set; }
            public string CustomerMessage { get; set; }
        }

        public class ErrorInfo
        {
            public string requestId { get; set; }
            public string errorCode { get; set; }
            public string errorMessage { get; set; }
        }

        public class MPesaRequest
        {
            [JsonProperty("BusinessShortCode")]
            public string BusinessShortCode { get; set; }
            [JsonProperty("Password")]
            public string Password { get; set; }
            [JsonProperty("Timestamp")]
            public string Timestamp { get; set; }
            [JsonProperty("TransactionType")]
            public string TransactionType { get; set; }
            [JsonProperty("Amount")]
            public string Amount { get; set; }
            [JsonProperty("PartyA")]
            public string PartyA { get; set; }
            [JsonProperty("PartyB")]
            public string PartyB { get; set; }
            [JsonProperty("PhoneNumber")]
            public string PhoneNumber { get; set; }
            [JsonProperty("CallBackURL")]
            public string CallBackURL { get; set; }
            [JsonProperty("AccountReference")]
            public string AccountReference { get; set; }
            [JsonProperty("TransactionDesc")]
            public string TransactionDesc { get; set; }
        }
    }
}