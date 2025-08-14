using System;
using System.Collections.Generic;

namespace AspxExamples
{
    public class YetkiliData
    {
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
}
