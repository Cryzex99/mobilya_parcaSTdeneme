using System.ComponentModel.DataAnnotations;

namespace mobilya_parcaST.Models;

public class Kategori
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kategori adı boş bırakılamaz.")]
    [MaxLength(50)]
    public string KategoriAdi { get; set; } = string.Empty;

    public string? Aciklama { get; set; }

    public ICollection<Urun> Urunler { get; set; } = new List<Urun>();
}
