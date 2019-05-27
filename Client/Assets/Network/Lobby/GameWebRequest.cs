using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Network.Lobby
{
    class GameWebRequest
    {
        //private const string ServiceUrl = "https://localhost:44326/Match/Index";
        public const string ServiceUrl = "http://172.25.51.101/Match/Index";

        private Action<GameWebRequest> registrationCallback;    // optional (when using async reg)

        public string Message { get; private set; } // msg from server (in case of success, this is the appid)

        protected internal Exception Exception { get; set; } // exceptions in account-server communication

        public int ReturnCode { get; private set; } // 0 = OK. anything else is a error with Message

         /// <summary>
        /// Creates a instance of the Account Service to register Photon Cloud accounts.
        /// </summary>
        public GameWebRequest()
        {
            WebRequest.DefaultWebProxy = null;
            ServicePointManager.ServerCertificateValidationCallback = Validator;
        }

        public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;    // any certificate is ok in this case
        }

        /// <summary>
        /// Check ReturnCode, Message and AppId to get the result of this attempt.
        /// </summary>
        public void SendMessage(byte[] msg)
        {
            this.registrationCallback = null;
            this.Message = string.Empty;
            this.ReturnCode = -1;

            string result;
            try
            {
                string str = "good";
                byte[] buff = Encoding.UTF8.GetBytes(str);


                WebRequest req = HttpWebRequest.Create(ServiceUrl);
                req.Method = "POST";
                req.ContentType = "text/html";
                req.ContentLength = msg.Length;
                req.GetRequestStream().Write(msg, 0, msg.Length);
                HttpWebResponse resp = req.GetResponse() as HttpWebResponse;

                // now read result
                StreamReader reader = new StreamReader(resp.GetResponseStream());
                result = reader.ReadToEnd();
            }
            catch (Exception ex)
            {
                this.Message = "Failed to connect to Cloud Account Service. Please register via account website.";
                this.Exception = ex;
                return;
            }

        }

        /// <summary>
        /// Attempts to create a Photon Cloud Account asynchronously.
        /// Once your callback is called, check ReturnCode, Message and AppId to get the result of this attempt.
        /// </summary>
        /// <param name="callback">Called when the result is available.</param>
        public void SendMessageAsync(byte[] msg, Action<GameWebRequest> callback = null)
        {
            this.registrationCallback = callback;
            this.Message = string.Empty;
            this.ReturnCode = -1;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(ServiceUrl);
                req.Method = "POST";
                req.ContentType = "text/html";
                req.ContentLength = msg.Length;
                req.GetRequestStream().Write(msg, 0, msg.Length);

                req.Timeout = 5000; // TODO: The Timeout property has no effect on asynchronous requests made with the BeginGetResponse
                req.BeginGetResponse(this.OnSendMessageCompleted, req);
            }
            catch (Exception ex)
            {
                this.Message = "Failed to connect to Cloud Account Service. Please register via account website.";
                this.Exception = ex;
                if (this.registrationCallback != null)
                {
                    this.registrationCallback(this);
                }
            }
        }

        /// <summary>
        /// Internal callback with result of async HttpWebRequest
        /// </summary>
        /// <param name="ar"></param>
        private void OnSendMessageCompleted(IAsyncResult ar)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
                HttpWebResponse response = request.EndGetResponse(ar) as HttpWebResponse;

                if (response != null && response.StatusCode == HttpStatusCode.OK)
                {
                    // no error. use the result
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string result = reader.ReadToEnd();
                    Debug.Log("result : " + result);

                }
                else
                {
                    // a response but some error on server. show message
                    this.Message = "Failed to connect to Cloud Account Service. Please register via account website.";
                }
            }
            catch (Exception ex)
            {
                // not even a response. show message
                this.Message = "Failed to connect to Cloud Account Service. Please register via account website.";
                this.Exception = ex;
            }

            if (this.registrationCallback != null)
            {
                this.registrationCallback(this);
            }
        }


    }
}
