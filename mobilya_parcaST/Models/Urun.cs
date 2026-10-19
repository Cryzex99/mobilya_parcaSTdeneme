using System.ComponentModel.DataAnnotations;

namespace mobilya_parcaST.Models;

public class Urun
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ürün adı boş bırakılamaz.")] //yapay zeka desteği
    [MaxLength(60)]
    public string UrunAdi { get; set; } = string.Empty;

    public string? Aciklama { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Fiyat 0,01 ile 99.999.999,99 arasında olmalıdır.")] //yapay zeka desteği
    public decimal Fiyat { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stok adedi negatif olamaz.")] //yapay zeka desteği
    public int StokAdedi { get; set; }

    public int KategoriId { get; set; }

    public Kategori Kategori { get; set; } = null!;
}
