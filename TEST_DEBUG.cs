// UrunleriYukle() metodunun başına şu satırları ekle (debug amaçlı):

private void UrunleriYukle(string aramaMetni = "")
{
    try
    {
        var toplamUrunSayisi = _db.Urunler.Count();
        MessageBox.Show($"DEBUG: Veritabanında toplam {toplamUrunSayisi} ürün var!", "Debug");

        _urunGridView.Rows.Clear();

        var urunler = _db.Urunler.Include(u => u.Kategori).ToList();

        MessageBox.Show($"DEBUG: Yüklenen ürün sayısı: {urunler.Count()}", "Debug");

        foreach (var urun in urunler)
        {
            _urunGridView.Rows.Add(urun.Id, urun.UrunAdi, urun.Aciklama ?? "-");
        }

        MessageBox.Show($"DEBUG: GridView'a eklenen satır sayısı: {_urunGridView.Rows.Count}", "Debug");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"ERROR in UrunleriYukle: {ex.Message}\n{ex.StackTrace}", "Hata");
    }
}
