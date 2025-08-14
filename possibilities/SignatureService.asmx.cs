using System;
using System.Web.Services;
using System.Web.Script.Services;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace AspxExamples
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class SignatureService : WebService
    {
        // In-memory cache için Dictionary (gerçek uygulamada veritabanı kullanılmalı)
        private static readonly Dictionary<string, SignatureAuthData> _signatureCache 
            = new Dictionary<string, SignatureAuthData>();

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public SaveSignatureResponse SaveSignature(string referenceId, SignatureAuthData authData)
        {
            try
            {
                // Veriyi cache'e kaydet
                lock (_signatureCache)
                {
                    _signatureCache[referenceId] = authData;
                }

                return new SaveSignatureResponse
                {
                    Success = true,
                    Message = "Veriler başarıyla kaydedildi",
                    ReferenceId = referenceId
                };
            }
            catch (Exception ex)
            {
                return new SaveSignatureResponse
                {
                    Success = false,
                    Message = "Hata: " + ex.Message
                };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public GetSignatureResponse GetSignature(string referenceId)
        {
            try
            {
                SignatureAuthData authData;
                lock (_signatureCache)
                {
                    if (!_signatureCache.TryGetValue(referenceId, out authData))
                    {
                        return new GetSignatureResponse
                        {
                            Success = false,
                            Message = "Belirtilen referans numarasına ait veri bulunamadı"
                        };
                    }

                    // Veriyi cache'den sil (bir kez okunduğunda silinsin)
                    _signatureCache.Remove(referenceId);
                }

                return new GetSignatureResponse
                {
                    Success = true,
                    Data = authData
                };
            }
            catch (Exception ex)
            {
                return new GetSignatureResponse
                {
                    Success = false,
                    Message = "Hata: " + ex.Message
                };
            }
        }
    }

    public class SaveSignatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string ReferenceId { get; set; }
    }

    public class GetSignatureResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public SignatureAuthData Data { get; set; }
    }
}
