foreach (TreeNode node in treeView1.Nodes)
{
    // Eğer text çok uzunsa muhtemelen base64'tür, temizle.
    if (node.Text.Length > 50) 
    {
        node.Text = ""; // Veya "Resim" yazabilirsin.
        node.Tag = "Burada base64 vardi"; // Datayı kaybetmemek için Tag'e atabilirsin.
    }
}