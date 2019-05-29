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
        private const string ServiceUrl = "https://localhost:44326/";
        //public const string ServiceUrl = "http://172.25.51.101/";


        public delegate void CallbackResponse(string msg);


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
        /// 비동기 메시지 전송
        /// </summary>
        /// <param name="callback">요청에 대한 응답을 콜백 함수(주의사항 : 다른 스레드에서 호출됨)</param>
        public void SendMessageAsync(string page, object msg, CallbackResponse callback = null)
        {
            this.Message = string.Empty;
            this.ReturnCode = -1;

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(string.Format("{0}{1}", ServiceUrl, page));
                req.Method = "POST";
                req.ContentType = "Application/json";


                string str;
                if (msg != null)
                {
                    str = JsonUtility.ToJson(msg);
                }
                else
                    str = "{}";

                byte[] buff = Encoding.UTF8.GetBytes(str);
                req.ContentLength = buff.Length;
                req.GetRequestStream().Write(buff, 0, buff.Length);


                req.Timeout = 5000; // TODO: The Timeout property has no effect on asynchronous requests made with the BeginGetResponse
                req.BeginGetResponse((IAsyncResult ar) =>
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



                            callback?.Invoke(result);

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

                }, req);
            }
            catch (Exception ex)
            {
                this.Message = "Failed to connect to Cloud Account Service. Please register via account website.";
                this.Exception = ex;
            }
        }

    }
}
