﻿using ACWSSK.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;


namespace ACWSSK.App_Code
{
    public class AppAPI
    {
        public AppAPI()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        }

        public bool SendAppApi(string barcode, int amount, out ACWAppAPI.ACWAppAPIResponse responseOut, out ACWAppAPI.ACWAppAPIResponseFailed responseFailedOut)
        {
            responseOut = new ACWAppAPI.ACWAppAPIResponse();
            responseFailedOut = new ACWAppAPI.ACWAppAPIResponseFailed() ;
            bool isSuccess = false;
            string responseBody = string.Empty;

            try
            {
                Trace.WriteLine("SendAppApi Starting...");

                SendAppApiParam param = new SendAppApiParam(barcode, amount, GeneralVar.CurrentComponent.ComponentCode);

                string requestBody = JsonConvert.SerializeObject(param);

                string apiURl = string.Empty;
                if (GeneralVar.ApplicationMode == AppMode.PROD)
                    apiURl = GeneralVar.AppUrl;
                else
                    apiURl = "https://sb-api.lecshine.com/v1/open/scan-dynamic-payment";

                Trace.WriteLine(requestBody);
                Trace.WriteLine(string.Format("SendAppApi AppUrl = {0}", apiURl));
                Trace.WriteLine(string.Format("SendAppApi X_Client_ID = {0}", GeneralVar.X_Client_ID));
                Trace.WriteLine(string.Format("SendAppApi X_Client_Secret = {0}", GeneralVar.X_Client_Secret));
                Trace.WriteLine(string.Format("SendAppApi Cookie = {0}", GeneralVar.Cookie));

                isSuccess = SendPostRequest(apiURl, requestBody, GeneralVar.X_Client_ID, GeneralVar.X_Client_Secret, GeneralVar.Cookie, out responseBody);

                Trace.WriteLine(string.Format("SendAppApi responseBody = {0}", responseBody));
                if (isSuccess)
                {
                    responseOut = JsonConvert.DeserializeObject<ACWAppAPI.ACWAppAPIResponse>(responseBody);

                    isSuccess = responseOut.item.Status.ToLower() == "success";
                }
                else
                {
                    responseFailedOut = JsonConvert.DeserializeObject<ACWAppAPI.ACWAppAPIResponseFailed>(responseBody);
                }
                Trace.WriteLine("SendAppApi Completed.");
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, String.Format("[Error] SendAppApi = {0}", ex.Message), "AppAPI");
            }
            return isSuccess;
        }

        public bool GetAppPriceAPI(string kioskId, out ACWAppAPI.GetKioskPriceResponse responseOut)
        {
            responseOut = new ACWAppAPI.GetKioskPriceResponse();
            bool isSuccess = false;
            string responseBody = string.Empty;

            try
            {
                Trace.WriteLine("GetAppPriceAPI Starting...");

                GetKioskPriceRequest param = new GetKioskPriceRequest
                {
                    branchKioskId = kioskId
                };

                string requestBody = JsonConvert.SerializeObject(param);

                string apiURl = string.Empty;
                if (GeneralVar.ApplicationMode == AppMode.PROD)
                    apiURl = "https://api.lecshine.com/v1/open/branch-kiosk/get-kiosk-price";
                else
                    apiURl = "https://sb-api.lecshine.com/v1/open/branch-kiosk/get-kiosk-price";

                Trace.WriteLine(requestBody);
                Trace.WriteLine(string.Format("SendAppApi API URL = {0}", apiURl));
                Trace.WriteLine(string.Format("SendAppApi X_Client_ID = {0}", GeneralVar.X_Client_ID));
                Trace.WriteLine(string.Format("SendAppApi X_Client_Secret = {0}", GeneralVar.X_Client_Secret));
                Trace.WriteLine(string.Format("SendAppApi Cookie = {0}", GeneralVar.Cookie));

                isSuccess = SendPostRequest(apiURl, requestBody, GeneralVar.X_Client_ID, GeneralVar.X_Client_Secret, GeneralVar.Cookie, out responseBody);

                Trace.WriteLine(string.Format("GetAppPriceAPI response = {0}", responseBody));
                if (isSuccess)
                {              
                    responseOut = JsonConvert.DeserializeObject<ACWAppAPI.GetKioskPriceResponse>(responseBody);

                    //isSuccess = !string.IsNullOrEmpty(responseOut.item.branchKioskId);
                }
                else
                    throw new Exception("API returned error");

                Trace.WriteLine("GetAppPriceAPI Completed.");
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, String.Format("[Error] GetAppPriceAPI = {0}", ex.Message), "AppAPI");
            }
            return isSuccess;
        }

        private bool SendPostRequest(string apiUrl, string requestBody, string clientId, string clientSecret, string cookie, out string responseBody)
        {
            bool isSent = false;
            responseBody = string.Empty;
            try
            {
                Trace.WriteLine("SendPostRequest Starting...");

                using (var client = new HttpClient())
                {
                    // Set headers
                    client.DefaultRequestHeaders.Add("X-Client-ID", clientId);
                    client.DefaultRequestHeaders.Add("X-Client-Secret", clientSecret);
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    var content = new StringContent(requestBody, System.Text.Encoding.UTF8, "application/json");

                    // Add the cookie to the request if it's not empty
                    if (!string.IsNullOrWhiteSpace(cookie))
                    {
                        client.DefaultRequestHeaders.Add("Cookie", cookie);
                    }

                    HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                    responseBody = response.Content.ReadAsStringAsync().Result;
                    if (response.IsSuccessStatusCode)
                    {
                        isSent = true;
                    }
                    else
                    {
                        Trace.WriteLine("API POST NOT SUCCESS", "AppAPI");
                        Trace.WriteLine(responseBody);
                        return isSent;
                    }
                }

                Trace.WriteLine("SendPostRequest Completed.");
            }
            catch (Exception ex)
            {
                Trace.WriteLine(string.Format("[Error] SendPostRequest = {0}", ex.ToString()), "AppAPI");
            }

            return isSent;
        }

    }
}
