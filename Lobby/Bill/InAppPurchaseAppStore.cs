using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace Lobby.Bill
{
    public class InAppPurchaseAppStore : InAppPurchase
    {
        public int statusCode = -1;
        string appleResponseText = null;
        bool SendboxReceiptEnable = false;
        bool is_test_account = false;
        public string transactionID;


        /// <summary>
        /// WebException : Web요청 200 이외 에러 코드 리턴시
        /// IOException : 서버 Connection 실패시
        /// Exception : 기타 알려지지 않은 익셉션
        /// </summary>
        /// <param name="url">요청 URL</param>
        /// <param name="requestData">POST 요청 넣을 데이터</param>
        /// <param name="isJsonContent">ContentType이 Json인가?</param>
        /// <returns>
        ///     응답 Text
        /// </returns>
        private string HttpRequestText(string url, string requestData, bool isJsonContent)
        {
            HttpWebRequest webReq = null;
            StreamWriter requestStreamWriter = null;
            WebResponse webRes = null;
            StreamReader resReader = null;

            try
            {
                webReq = (HttpWebRequest)WebRequest.Create(url);
                webReq.KeepAlive = false;
                webReq.Method = "POST";
                webReq.ContentType = isJsonContent == true ? "application/json" : "application/x-www-form-urlencoded";
                webReq.ContentLength = requestData.Length;

                requestStreamWriter = new StreamWriter(webReq.GetRequestStream());
                requestStreamWriter.Write(requestData);
                requestStreamWriter.Close();

                webRes = webReq.GetResponse();
                resReader = new StreamReader(webRes.GetResponseStream());
                return resReader.ReadToEnd();
            }
            catch (WebException e)
            {
                throw e;
            }
            catch (IOException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                throw new Exception("Unknown exception: " + e.Message, e);
            }
            finally
            {
                if (resReader != null)
                    resReader.Close();

                if (webRes != null)
                    webRes.Close();
            }
        }

        public override bool VerifyReceipt(string receipt, string signature)
        {
            var requestJson = new JObject();
            JObject responseJson = null;
            string appleRequestText = null;

            try
            {
                requestJson["receipt-data"] = receipt;
                appleRequestText = requestJson.ToString();
            }
            catch (Exception e)
            {
                Log.Error(e, "in VerifyReceipt");
                return false;
            }

            try
            {
                appleResponseText = HttpRequestText("https://buy.itunes.apple.com/verifyReceipt", appleRequestText, false);
                try
                {
                    responseJson = JObject.Parse(appleResponseText);
                }
                catch (Exception e)
                {
                    throw new Exception("apple response json parse error, " + e.Message);
                }

                statusCode = (int)responseJson["status"];
            }
            catch (WebException e)
            {
                throw new WebException("https://buy.itunes.apple.com/verifyReceipt exception:" + e.Message, e);
            }
            catch (IOException e)
            {
                throw new IOException("https://buy.itunes.apple.com/verifyReceipt exception:" + e.Message, e);
            }
            catch (Exception e)
            {
                Log.Error(e, "in VerifyReceipt");
                return false;
            }

            try
            {
                // Sandbox 영수증일 경우 
                // Sandbox 영수증 옵션이 켜져있을때만 검증 요청
                if (SendboxReceiptEnable && statusCode == 21007)
                {
                    appleResponseText = HttpRequestText("https://sandbox.itunes.apple.com/verifyReceipt", appleRequestText, false);
                    responseJson = JObject.Parse(appleResponseText);
                    statusCode = (int)responseJson["status"];

                    is_test_account = true;
                }
                else
                {
                    if (0 != statusCode)
                    {
                        return false;
                    }
                    else
                    {
                        is_test_account = false;
                    }
                }
            }
            catch (WebException e)
            {
                throw new WebException("https://sandbox.itunes.apple.com/verifyReceipt exception:" + e.Message, e);
            }
            catch (IOException e)
            {
                throw new IOException("https://sandbox.itunes.apple.com/verifyReceipt exception:" + e.Message, e);
            }
            catch (Exception e)
            {
                Log.Error(e, "in VerifyReceipt");
                return false;
            }

            // 영수증 체크 : 결제 해킹 앱 사용한 케이스(in_app필드 상에 데이터가 없음)
            {
 
                // 정상적인 영수증 확인
                if (null == responseJson["receipt"]["in_app"])
                {
                    Log.Error("null == resultJson[in_app]" + appleResponseText);
                    return false;
                }

                // 중복처리 요청인지
                if (null == responseJson["receipt"]["in_app"][0]["transaction_id"])
                {
                    Log.Error("null == resultJson[in_app][transaction_id]" + appleResponseText);
                    return false;
                }

                transactionID = responseJson["receipt"]["in_app"][0]["transaction_id"].ToString();

                // 우리 게임 맞는지 번들 id비교
                if ("com.ftt.boxingstar.gl.ios" != (string)responseJson["receipt"]["bundle_id"])
                {
                    Log.Error("com.friendsgames.ios != (string)resultJson[bundle_id]" + appleResponseText);
                    return false;

                }
            }

            if (0 != statusCode)
            {
                Log.Error("Status " + statusCode + ", " + appleResponseText);
                return false;
            }

            return true;
        }

    }
}
