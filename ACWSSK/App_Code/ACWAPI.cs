using ACWSSK.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static ACWSSK.Model.QRReceiptModel;

namespace ACWSSK.App_Code
{
    public class ACWAPI
    {
        const string TraceCategory = "ACWSSK.App_Code.ACWAPI";

        public ACWAPI()
		{
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
		}

        public CheckEquipStatusResponse QDCheckEquipmentStatus() 
        {
            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "QDCheckEquipmentStatus starting...", TraceCategory);
            CheckEquipStatusParam EquipStatusParam;
            CheckEquipStatusResponse responseData = new CheckEquipStatusResponse();

            string result = string.Empty;

            try
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                long unixTimeMilliseconds = now.ToUnixTimeMilliseconds();
                
                string secret = GeneralVar.QingDaoAPI_Secret.Substring(0, 13) + (unixTimeMilliseconds % 1000).ToString();
                string EncryptedSign = EncryptDataWithAES(GeneralVar.QingDaoAPI_OpenId, secret);

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, string.Format("QDCheckEquipmentStatus unixTimeMilliseconds: {0}", unixTimeMilliseconds), TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, string.Format("QDCheckEquipmentStatus secret string: {0}", secret), TraceCategory);
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, string.Format("QDCheckEquipmentStatus EncryptedSign: {0}", EncryptedSign), TraceCategory);

                string url = string.Format(@"{0}/api/open/v1/common/checkStatus", GeneralVar.QingDaoAPI_URL);

                EquipStatusParam = new CheckEquipStatusParam(GeneralVar.QingDaoAPI_OpenId, unixTimeMilliseconds, GeneralVar.QingDaoAPI_IotId);
                var ObjSerialize = JsonConvert.SerializeObject(EquipStatusParam);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                request.ContentType = "application/json";
                request.Headers["sign"] = EncryptedSign;
                

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, string.Format("QDCheckEquipmentStatus Url: {0}", url), TraceCategory);

                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, string.Format("QDCheckEquipmentStatus Parameter: {0}", ObjSerialize), TraceCategory);
                    streamWriter.Write(ObjSerialize);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                using (Stream responseStream = response.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    responseData = JsonConvert.DeserializeObject<CheckEquipStatusResponse>(reader.ReadToEnd());
                }

                if (responseData.code != 0) 
                {
                    Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("QDCheckEquipmentStatus Response Code: {0}, Message : {1}", responseData.code, responseData.msg), TraceCategory);
                }
            }
			catch (Exception ex)
			{
				Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("QDCheckEquipmentStatus Error : {0}", ex.Message), TraceCategory);
                responseData.code = -1;
                responseData.msg = ex.Message;
			}

            Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "QDCheckEquipmentStatus completed.", TraceCategory);
            return responseData;

        }

        public static string EncryptDataWithAES(string plainText, string Key)
        {
            byte[] encryptedData;

            using (Aes aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.KeySize = 128;
                aesAlgorithm.Mode = CipherMode.ECB;
                aesAlgorithm.Key = Encoding.ASCII.GetBytes(Key);


                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor();

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        encryptedData = ms.ToArray();
                    }
                }
            }
            return Convert.ToBase64String(encryptedData);
        }

        public bool GenerateQRReceipt(string APIurl, QRReceiptRequest qrPayment, int timeout, out QRReceiptResponse response, out string jsonRequest, out string jsonResponse, out string errorMsg)
        {
            bool success = false;
            response = null;
            errorMsg = string.Empty;
            string errMessage = string.Empty;
            jsonRequest = string.Empty;
            jsonResponse = string.Empty;
            string result = null;

            try
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GenerateQRReceipt Starting..."), TraceCategory);

                jsonRequest = JsonConvert.SerializeObject(qrPayment);
                if (string.IsNullOrEmpty(jsonRequest))
                    throw new Exception("Unable to Serialized GenerateQRReceipt");

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GenerateQRReceipt [Request] = {0}", jsonRequest), TraceCategory);

                string param = jsonRequest;
                ASCIIEncoding encode = new ASCIIEncoding();
                byte[] data = encode.GetBytes(param);

                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(APIurl);
                request.Timeout = 30000;
                request.Method = "POST";
                request.ContentType = "application/json";

                using (Stream writeStream = request.GetRequestStream())
                {
                    writeStream.Write(data, 0, data.Length);
                }

                HttpWebResponse HttpResponse = (HttpWebResponse)request.GetResponse();

                using (Stream responseStream = HttpResponse.GetResponseStream())
                {
                    StreamReader reader = new StreamReader(responseStream, Encoding.UTF8);
                    result = reader.ReadToEnd();
                    //WriteLogIntoFile(String.Format("HttpPostResponse result - {0}", result));
                }

                if (string.IsNullOrEmpty(result))
                    throw new Exception("Response[Json] is null");

                if (!string.IsNullOrEmpty(errMessage))
                {
                    errorMsg = errMessage;
                    throw new Exception(errorMsg);
                }

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GenerateQRReceipt [Response] = {0}", result), TraceCategory);

                response = JsonConvert.DeserializeObject<QRReceiptResponse>(result);
                if (response == null)
                    throw new Exception("GenerateQRReceiptResponse is NULL!");

                success = true;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("GenerateQRReceipt Completed."), TraceCategory);
            }
            catch (Exception ex)
            {
                errorMsg = ex.Message;
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[Error] GenerateQRReceipt = {0}", ex.Message), TraceCategory);
            }
            return success;
        }
    }
}
