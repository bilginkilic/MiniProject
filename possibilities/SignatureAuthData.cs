using System;
using System.Collections.Generic;

namespace AspxExamples
{
    public class YetkiliData
    {
        public int ID { get; set; }
        public int CircularID { get; set; }
        public string YetkiliKontakt { get; set; }
        public string YetkiliAdi { get; set; }
        public string YetkiSekli { get; set; }
        public string YetkiTarihi { get; set; }
        public string YetkiBitisTarihi { get; set; }
        public string YetkiGrubu { get; set; }
        public string SinirliYetkiDetaylari { get; set; }
        public string YetkiTurleri { get; set; }
        public decimal YetkiTutari { get; set; }
        public string YetkiDovizCinsi { get; set; }
        public string YetkiDurumu { get; set; }
        public List<SignatureImage> Imzalar { get; set; }

        public YetkiliData()
        {
            Imzalar = new List<SignatureImage>();
        }
    }

    public class SignatureAuthData
    {
        public List<YetkiliData> Yetkililer { get; set; }
        public string KaynakPdfAdi { get; set; }

        public SignatureAuthData()
        {
            Yetkililer = new List<YetkiliData>();
        }
    }

    public class SignatureImage
    {
        public string ImageData { get; set; }  // Base64 formatında imza verisi
        public int SiraNo { get; set; }        // İmzanın sıra numarası
        public string SourcePdfPath { get; set; } // İmzanın alındığı PDF dosyası
    }

    public class CircularData
    {
        public int ID { get; set; }
        public string CustomerNo { get; set; }
        public string CompanyTitle { get; set; }
        public DateTime? IssuedDate { get; set; }
        public DateTime? ValidityDate { get; set; }
        public DateTime? InternalBylawsTsgDate { get; set; }
        public string SpecialCases { get; set; }
        public string CircularType { get; set; }
        public string CircularNotaryNo { get; set; }
        public string Description { get; set; }
        public string CircularStatus { get; set; }
        public bool? IsABoardOfDirectorsDecisionRequired { get; set; }
        public DateTime? MainCircularDate { get; set; }
        public string MainCircularRef { get; set; }
        public string AdditionalDocuments { get; set; }
        public string RecordStatus { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastUpdate { get; set; }
        public string Channel { get; set; }

        public List<YetkiliData> Yetkililer { get; set; }

        public CircularData()
        {
            Yetkililer = new List<YetkiliData>();
        }
    }
}
