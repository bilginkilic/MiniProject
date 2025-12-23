using System.Drawing; // Gerekli kütüphane
using System.IO;

public void ConvertBase64ToNodeImage()
{
    // 1. Yeni bir ImageList oluştur (veya var olanı kullan)
    ImageList myImageList = new ImageList();
    myImageList.ImageSize = new Size(16, 16); // İkon boyutunu ayarla
    
    // TreeView'a bu listeyi ata
    treeView1.ImageList = myImageList;

    // 2. TreeView içindeki node'ları gez
    foreach (TreeNode node in treeView1.Nodes)
    {
        ProcessNode(node, myImageList);
    }
}

private void ProcessNode(TreeNode node, ImageList imgList)
{
    // Node'un text'inde Base64 var mı kontrol et (Basit bir kontrol)
    // Genelde Base64 uzun olur ve özel karakter içermez.
    // Burada verinin formatını bildiğini varsayıyorum.
    string nodeText = node.Text;

    if (IsBase64String(nodeText)) 
    {
        try
        {
            // A. Base64'ü Resme Çevir
            byte[] imageBytes = Convert.FromBase64String(nodeText);
            using (MemoryStream ms = new MemoryStream(imageBytes))
            {
                Image img = Image.FromStream(ms);
                
                // B. Resmi Listeye Ekle
                imgList.Images.Add(img);
                
                // C. Node'a bu resmin index'ini ver
                int imageIndex = imgList.Images.Count - 1;
                node.ImageIndex = imageIndex;
                node.SelectedImageIndex = imageIndex; // Seçilince de aynı kalsın
                
                // D. Node üzerindeki o uzun Base64 yazısını kaldır veya mantıklı bir isim ver
                node.Text = "Görsel " + imageIndex; 
            }
        }
        catch
        {
            // Base64 çevrilirken hata olursa node'u bozma
        }
    }

    // Alt node'lar varsa (recursive) onlar için de aynısını yap
    foreach (TreeNode child in node.Nodes)
    {
        ProcessNode(child, imgList);
    }
}

// Basit bir Base64 doğrulama fonksiyonu (İhtiyaca göre geliştirilebilir)
private bool IsBase64String(string s)
{
    // String çok kısaysa base64 değildir veya boşsa
    if (string.IsNullOrEmpty(s) || s.Length < 20) return false;
    // Genelde verinin "iVBOR..." gibi başladığını kontrol edebilirsin
    return (s.Length % 4 == 0) && System.Text.RegularExpressions.Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,2}$", System.Text.RegularExpressions.RegexOptions.None);
}