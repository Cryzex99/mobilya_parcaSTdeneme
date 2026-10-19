using Microsoft.EntityFrameworkCore;
using mobilya_parcaST.Data;
using mobilya_parcaST.Models;

namespace mobilya_parcaST;

/// <summary>
/// Mobilya Stok Takip Sistemi - Ana Form
/// Kategori ve ÃœrÃ¼n CRUD iÅŸlemleri, arama ve filtreleme
/// </summary>
public partial class Form1 : Form
{
    // === VERITABANI ===
    // VeritabanÄ± context'i - global eriÅŸim iÃ§in
    private MobilyaDbContext _db = null!;

    // === UI KONTROLLERI ===
    private TabControl _tabControl = null!;
    private DataGridView _urunGridView = null!;
    private DataGridView _kategoriGridView = null!;
    private TextBox _urunAramaTextBox = null!;
    private ComboBox _kategoriFilterCombo = null!;

    public Form1()
    {
        InitializeComponent();
        Text = "ğŸ“¦ Mobilya Stok Takip Sistemi";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1300, 800);
        Font = new Font("Segoe UI", 10);
        BackColor = Color.White;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            // === VERITABANI BAÅLAT ===
            // 1. DbContext'i oluÅŸtur (MySQL baÄŸlantÄ± ayarlarÄ±nÄ± yÃ¼kle)
            // 2. Tabelolar yoksa oluÅŸtur (EnsureCreated)
            _db = new MobilyaDbContext(VeritabaniYapilandirmasi.DbContextSecenekleriOlustur());
            _db.Database.EnsureCreated();

            // === DEMO VERINI SÄ°L ===
            // Demo Ã¼rÃ¼nleri koddan kaldÄ±r (ahÅŸap masa vs.)
            var demoUrunler = _db.Urunler.Where(u => 
                u.UrunAdi.ToLower().Contains("demo") || 
                u.UrunAdi.ToLower().Contains("ahÅŸap masa") ||
                u.UrunAdi.ToLower().Contains("test")).ToList();

            if (demoUrunler.Any())
            {
                _db.Urunler.RemoveRange(demoUrunler);
                _db.SaveChanges();
            }

            // === UI OLUÅTUR ===
            // TÃ¼m arayÃ¼z elemanlarÄ±nÄ± (Tab, Grid, TextBox vb.) oluÅŸtur
            OlusturUI();

            // === VERÄ°LERÄ° YÃœKLEME ===
            // Form aÃ§Ä±ldÄ±ÄŸÄ±nda veritabanÄ±ndan verileri baÅŸlangÄ±Ã§ta yÃ¼kle
            KategorileriYukle();
            UrunleriYukle();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Uygulama baÅŸlatÄ±lÄ±rken hata:\n{ex.Message}", "Kritik Hata", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            Application.Exit();
        }
    }

    /// <summary>
    /// TÃ¼m UI elemanlarÄ±nÄ± oluÅŸturur: TabControl, DataGridView, TextBox, Button vb.
    /// TÃ¼rkÃ§e arayÃ¼z, dÃ¼zenli layout ve responsive tasarÄ±m
    /// </summary>
    private void OlusturUI()
    {
        // =====================================================================
        // TAB CONTROL - Sekmeli ArayÃ¼z (ÃœrÃ¼nler, Kategoriler)
        // =====================================================================
        _tabControl = new TabControl
        {
            Dock = DockStyle.Fill,
            Padding = new Point(5, 5)
        };
        Controls.Add(_tabControl);

        // ----- SEKME 1: ÃœRÃœNLER -----
        var urunSekmesi = new TabPage("ğŸ“¦ ÃœrÃ¼nler");
        urunSekmesi.Padding = new Padding(0);
        _tabControl.TabPages.Add(urunSekmesi);

        var urunPanel = new Panel { Dock = DockStyle.Fill };
        urunSekmesi.Controls.Add(urunPanel);

        // ARACI BAR (Arama, Filtre, Butonlar)
        var aramaPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 80,
            BackColor = Color.FromArgb(200, 220, 240),
            Padding = new Padding(10, 8, 10, 8)
        };
        urunPanel.Controls.Add(aramaPanel);

        // SATIR 1: ARAMA KUTUSU
        var aramaLabel = new Label 
        { 
            Text = "ğŸ” ÃœrÃ¼n Ara:", 
            Location = new Point(10, 10), 
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        aramaPanel.Controls.Add(aramaLabel);

        _urunAramaTextBox = new TextBox
        {
            Location = new Point(110, 8),
            Width = 200,
            Height = 28,
            Text = ""
        };
        aramaPanel.Controls.Add(_urunAramaTextBox);

        var aramaButonu = new Button
        {
            Text = "ğŸ” Ara",
            Location = new Point(320, 8),
            Width = 85,
            Height = 28,
            BackColor = Color.LightBlue
        };
        aramaButonu.Click += (s, e) => UrunleriYukle(_urunAramaTextBox.Text);
        aramaPanel.Controls.Add(aramaButonu);

        // SATIR 2: KATEGORÄ° FÄ°LTRESÄ° + BUTONLAR
        var filtreLabel = new Label 
        { 
            Text = "ğŸ·ï¸ Kategori:", 
            Location = new Point(10, 45), 
            AutoSize = true,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        aramaPanel.Controls.Add(filtreLabel);

        _kategoriFilterCombo = new ComboBox
        {
            Location = new Point(110, 43),
            Width = 200,
            Height = 28,
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _kategoriFilterCombo.SelectedIndexChanged += (s, e) => UrunleriYukle();
        aramaPanel.Controls.Add(_kategoriFilterCombo);

        var yenileButonu = new Button
        {
            Text = "ğŸ”„ Yenile",
            Location = new Point(320, 43),
            Width = 85,
            Height = 28,
            BackColor = Color.LightYellow
        };
        yenileButonu.Click += (s, e) =>
        {
            _urunAramaTextBox.Text = "";
            _kategoriFilterCombo.SelectedIndex = -1;
            UrunleriYukle();
        };
        aramaPanel.Controls.Add(yenileButonu);

        var urunEkleButonu = new Button
        {
            Text = "â• ÃœrÃ¼n Ekle",
            Location = new Point(415, 43),
            Width = 110,
            Height = 28,
            BackColor = Color.LightGreen,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        urunEkleButonu.Click += UrunEkleButonu_Click;
        aramaPanel.Controls.Add(urunEkleButonu);

        // DATAGRIDVIEW - ÃœRÃœNLER TABLOSU
        _urunGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D
        };
        urunPanel.Controls.Add(_urunGridView);

        // SÃ¼tunlarÄ± tanÄ±mla
        _urunGridView.Columns.Add("Id", "ID");
        _urunGridView.Columns.Add("UrunAdi", "ÃœrÃ¼n AdÄ±");
        _urunGridView.Columns.Add("Aciklama", "AÃ§Ä±klama");
        _urunGridView.Columns.Add("Fiyat", "Fiyat (â‚º)");
        _urunGridView.Columns.Add("StokAdedi", "Stok");
        _urunGridView.Columns.Add("KategoriAdi", "Kategori");

        // BUTON SÃœTUNLARI
        var duzenleButtonColumn = new DataGridViewButtonColumn
        {
            Name = "Duzenle",
            Text = "âœï¸ DÃ¼zenle",
            UseColumnTextForButtonValue = true
        };
        _urunGridView.Columns.Add(duzenleButtonColumn);

        var silButtonColumn = new DataGridViewButtonColumn
        {
            Name = "Sil",
            Text = "ğŸ—‘ï¸ Sil",
            UseColumnTextForButtonValue = true
        };
        _urunGridView.Columns.Add(silButtonColumn);

        // SÃ¼tun geniÅŸliklerini ayarla
        _urunGridView.Columns[0].Width = 50;
        _urunGridView.Columns[1].Width = 150;
        _urunGridView.Columns[2].Width = 200;
        _urunGridView.Columns[3].Width = 90;
        _urunGridView.Columns[4].Width = 70;
        _urunGridView.Columns[5].Width = 90;
        _urunGridView.Columns[6].Width = 85;  // DÃ¼zenle butonu
        _urunGridView.Columns[7].Width = 75;  // Sil butonu

        // CellClick event'i ekle
        _urunGridView.CellClick += UrunGridView_CellClick;
        _urunGridView.Columns[5].Width = 120;

        // ----- SEKME 2: KATEGORÄ°LER -----
        var kategoriSekmesi = new TabPage("ğŸ·ï¸ Kategoriler");
        kategoriSekmesi.Padding = new Padding(0);
        _tabControl.TabPages.Add(kategoriSekmesi);

        var kategoriPanel = new Panel { Dock = DockStyle.Fill };
        kategoriSekmesi.Controls.Add(kategoriPanel);

        // ARACI BAR (Kategori Ekle Butonu)
        var kategoriAramaPanel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 50,
            BackColor = Color.FromArgb(240, 200, 220),
            Padding = new Padding(10, 10, 10, 10)
        };
        kategoriPanel.Controls.Add(kategoriAramaPanel);

        var kategoriEkleButonu = new Button
        {
            Text = "â• Kategori Ekle",
            Location = new Point(10, 10),
            Width = 140,
            Height = 28,
            BackColor = Color.LightGreen,
            ForeColor = Color.Black,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        kategoriEkleButonu.Click += KategoriEkleButonu_Click;
        kategoriAramaPanel.Controls.Add(kategoriEkleButonu);

        // DATAGRIDVIEW - KATEGORÄ°LER TABLOSU
        _kategoriGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.Fixed3D
        };
        kategoriPanel.Controls.Add(_kategoriGridView);

        // SÃ¼tunlarÄ± tanÄ±mla
        _kategoriGridView.Columns.Add("Id", "ID");
        _kategoriGridView.Columns.Add("KategoriAdi", "Kategori AdÄ±");
        _kategoriGridView.Columns.Add("Aciklama", "AÃ§Ä±klama");

        // BUTON SÃœTUNLARI
        var duzenleButtonColumn2 = new DataGridViewButtonColumn
        {
            Name = "Duzenle",
            Text = "âœï¸ DÃ¼zenle",
            UseColumnTextForButtonValue = true
        };
        _kategoriGridView.Columns.Add(duzenleButtonColumn2);

        var silButtonColumn2 = new DataGridViewButtonColumn
        {
            Name = "Sil",
            Text = "ğŸ—‘ï¸ Sil",
            UseColumnTextForButtonValue = true
        };
        _kategoriGridView.Columns.Add(silButtonColumn2);

        _kategoriGridView.Columns[0].Width = 50;
        _kategoriGridView.Columns[1].Width = 150;
        _kategoriGridView.Columns[2].Width = 250;
        _kategoriGridView.Columns[3].Width = 85;  // DÃ¼zenle butonu
        _kategoriGridView.Columns[4].Width = 75;  // Sil butonu

        // CellClick event'i ekle
        _kategoriGridView.CellClick += KategoriGridView_CellClick;
    }

    /// <summary>
    /// VeritabanÄ±ndan tÃ¼m Ã¼rÃ¼nleri Ã§eker ve DataGridView'a yÃ¼kler.
    /// Arama metni ve kategori filtresi varsa ona gÃ¶re filtreler.
    /// BoÅŸ sonuÃ§ varsa MessageBox gÃ¶sterir.
    /// </summary>
    private void UrunleriYukle(string aramaMetni = "")
    {
        try
        {
            _urunGridView.Rows.Clear();

            // VeritabanÄ±ndan tÃ¼m Ã¼rÃ¼nleri Ã§ek + Kategori bilgisini yÃ¼kle
            var urunler = _db.Urunler
                .Include(u => u.Kategori)
                .ToList();

            // ARAMA FÄ°LTRESÄ° - Harflere dayalÄ± arama (partial string matching)
            // Ã–rn: "K" yazarsanÄ±z -> "Koltuk", "KanapÃ©" gibi K ile baÅŸlayan veya K iÃ§eren Ã¼rÃ¼nler
            if (!string.IsNullOrEmpty(aramaMetni))
            {
                // Hem baÅŸÄ±nda hem de iÃ§inde aramasÄ± yapacak
                urunler = urunler
                    .Where(u => u.UrunAdi.StartsWith(aramaMetni, StringComparison.OrdinalIgnoreCase) ||
                                u.UrunAdi.Contains(aramaMetni, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(u => u.UrunAdi.StartsWith(aramaMetni, StringComparison.OrdinalIgnoreCase)) // Ã–nce baÅŸÄ±nda olanlar
                    .ThenBy(u => u.UrunAdi) // Sonra alfabetik
                    .ToList();
            }

            // KATEGORÄ° FÄ°LTRESÄ°
            if (_kategoriFilterCombo.SelectedIndex >= 0 && _kategoriFilterCombo.SelectedItem is Kategori secilenKategori)
            {
                urunler = urunler.Where(u => u.KategoriId == secilenKategori.Id).ToList();
            }

            // SONUÃ‡ YOKSA MESSAGEBOX GÃ–STER
            if (urunler.Count == 0)
            {
                if (!string.IsNullOrEmpty(aramaMetni))
                {
                    MessageBox.Show(
                        $"âŒ '{aramaMetni}' ile eÅŸleÅŸen Ã¼rÃ¼n bulunamadÄ±!", 
                        "SonuÃ§ Yok", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Information
                    );
                }
                else if (_kategoriFilterCombo.SelectedIndex >= 0)
                {
                    MessageBox.Show(
                        "âŒ SeÃ§ilen kategoride Ã¼rÃ¼n bulunamadÄ±!", 
                        "SonuÃ§ Yok", 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Information
                    );
                }
            }

            // SonuÃ§larÄ± GridView'a ekle
            foreach (var urun in urunler)
            {
                _urunGridView.Rows.Add(
                    urun.Id,
                    urun.UrunAdi,
                    urun.Aciklama ?? "(BoÅŸ)",
                    urun.Fiyat.ToString("C2"),
                    urun.StokAdedi,
                    urun.Kategori?.KategoriAdi ?? "(Kategori Yok)"
                );
            }

            // Kategori dropdown'unu gÃ¼ncelle
            if (aramaMetni == "")
            {
                KategoriFilteriniGuncelle();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"ÃœrÃ¼nler yÃ¼klenirken hata:\n{ex.Message}", 
                "Hata", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Error
            );
        }
    }

    /// <summary>
    /// VeritabanÄ±ndan tÃ¼m kategorileri Ã§eker ve DataGridView'a yÃ¼kler.
    /// BoÅŸ sonuÃ§ varsa MessageBox gÃ¶sterir.
    /// </summary>
    private void KategorileriYukle()
    {
        try
        {
            _kategoriGridView.Rows.Clear();

            var kategoriler = _db.Kategoriler.ToList();

            // BoÅŸ ise mesaj gÃ¶ster
            if (kategoriler.Count == 0)
            {
                MessageBox.Show(
                    "âŒ VeritabanÄ±nda kategori bulunamadÄ±!\n\nÃ–nce bir kategori oluÅŸturunuz.", 
                    "Kategori Yok", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Information
                );
            }

            // GridView'a kategorileri ekle
            foreach (var kategori in kategoriler)
            {
                _kategoriGridView.Rows.Add(
                    kategori.Id,
                    kategori.KategoriAdi,
                    kategori.Aciklama ?? "(BoÅŸ)"
                );
            }

            KategoriFilteriniGuncelle();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Kategoriler yÃ¼klenirken hata:\n{ex.Message}", 
                "Hata", 
                MessageBoxButtons.YesNo, 
                MessageBoxIcon.Error
            );
        }
    }

    /// <summary>
    /// Kategori ComboBox'Ä±nÄ± (Ã¼rÃ¼n arama filterinde) veritabanÄ±ndaki kategorilerle gÃ¼nceller.
    /// </summary>
    private void KategoriFilteriniGuncelle()
    {
        _kategoriFilterCombo.Items.Clear();
        _kategoriFilterCombo.DisplayMember = "KategoriAdi";
        _kategoriFilterCombo.ValueMember = "Id";

        var kategoriler = _db.Kategoriler.ToList();

        foreach (var kategori in kategoriler)
        {
            _kategoriFilterCombo.Items.Add(kategori);
        }

        _kategoriFilterCombo.SelectedIndex = -1;
    }

    /// <summary>
    /// "ÃœrÃ¼n Ekle" butonuna tÄ±klandÄ±ÄŸÄ±nda Ã§alÄ±ÅŸÄ±r.
    /// Yeni Ã¼rÃ¼n eklemek iÃ§in dialog formu aÃ§ar ve veritabanÄ±na kaydeder.
    /// </summary>
    private void UrunEkleButonu_Click(object? sender, EventArgs e)
    {
        var ekleForm = new Form
        {
            Text = "â• Yeni ÃœrÃ¼n Ekle",
            Width = 500,
            Height = 430,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            Owner = this
        };

        // ÃœRÃœN ADI
        var urunAdiLabel = new Label { Text = "ÃœrÃ¼n AdÄ± *:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var urunAdiTextBox = new TextBox { Location = new Point(150, 18), Width = 310, Height = 28 };
        ekleForm.Controls.AddRange(new Control[] { urunAdiLabel, urunAdiTextBox });

        // AÃ‡IKLAMA
        var aciklamaLabel = new Label { Text = "AÃ§Ä±klama:", Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var aciklamaTextBox = new TextBox { Location = new Point(150, 58), Width = 310, Height = 80, Multiline = true };
        ekleForm.Controls.AddRange(new Control[] { aciklamaLabel, aciklamaTextBox });

        // FÄ°YAT
        var fiyatLabel = new Label { Text = "Fiyat (â‚º) *:", Location = new Point(20, 155), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var fiyatTextBox = new TextBox { Location = new Point(150, 153), Width = 310, Height = 28 };
        ekleForm.Controls.AddRange(new Control[] { fiyatLabel, fiyatTextBox });

        // STOK
        var stokLabel = new Label { Text = "Stok Adedi *:", Location = new Point(20, 200), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var stokNumeric = new NumericUpDown { Location = new Point(150, 198), Width = 310, Height = 28, Value = 1, Minimum = 0, Maximum = 100000 };
        ekleForm.Controls.AddRange(new Control[] { stokLabel, stokNumeric });

        // KATEGORÄ°
        var kategoriLabel = new Label { Text = "Kategori *:", Location = new Point(20, 250), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var kategoriCombo = new ComboBox { Location = new Point(150, 248), Width = 310, Height = 28, DropDownStyle = ComboBoxStyle.DropDownList };
        kategoriCombo.DisplayMember = "KategoriAdi";
        kategoriCombo.ValueMember = "Id";
        foreach (var k in _db.Kategoriler.ToList())
        {
            kategoriCombo.Items.Add(k);
        }
        ekleForm.Controls.AddRange(new Control[] { kategoriLabel, kategoriCombo });

        // KAYDET BUTONU
        var kaydetButonu = new Button
        {
            Text = "âœ… Kaydet",
            Location = new Point(150, 310),
            Width = 100,
            Height = 32,
            BackColor = Color.LightGreen,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        kaydetButonu.Click += (s, e) =>
        {
            // VALIDASYON
            if (string.IsNullOrEmpty(urunAdiTextBox.Text))
            {
                MessageBox.Show("âŒ ÃœrÃ¼n adÄ± boÅŸ olamaz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            if (kategoriCombo.SelectedIndex < 0 || kategoriCombo.SelectedItem == null)
            {
                MessageBox.Show("âŒ Kategori seÃ§iniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            if (!decimal.TryParse(fiyatTextBox.Text, out var fiyat) || fiyat < 0)
            {
                MessageBox.Show("âŒ GeÃ§erli bir fiyat giriniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            // DUPLICATE KONTROL - AynÄ± adlÄ± Ã¼rÃ¼n var mÄ±?
            var trimmedUrunAdi = urunAdiTextBox.Text.Trim();
            var mevcutUrun = _db.Urunler.FirstOrDefault(u => 
                u.UrunAdi.ToLower() == trimmedUrunAdi.ToLower());

            if (mevcutUrun != null)
            {
                MessageBox.Show(
                    $"âŒ Bu Ã¼rÃ¼n zaten mevcut!\\n\\nÃœrÃ¼n: {mevcutUrun.UrunAdi}\\nKategori: {mevcutUrun.Kategori?.KategoriAdi}",
                    "Duplicate ÃœrÃ¼n",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );`r`n                if (result == DialogResult.No)`r`n                    return;
            }

            try
            {
                // Yeni ÃœrÃ¼n objesi oluÅŸtur ve DB'ye ekle
                var secilenKategori = (Kategori)kategoriCombo.SelectedItem;
                var yeniUrun = new Urun
                {
                    UrunAdi = urunAdiTextBox.Text.Trim(),
                    Aciklama = aciklamaTextBox.Text.Trim(),
                    Fiyat = fiyat,
                    StokAdedi = (int)stokNumeric.Value,
                    KategoriId = secilenKategori.Id
                };

                _db.Urunler.Add(yeniUrun);
                _db.SaveChanges();

                MessageBox.Show("âœ… ÃœrÃ¼n baÅŸarÄ±yla eklendi!", "BaÅŸarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                ekleForm.Close();

                // DbContext'i refresh et ve Ã¼rÃ¼nleri yeniden yÃ¼kle
                _db.Dispose();
                _db = new MobilyaDbContext(VeritabaniYapilandirmasi.DbContextSecenekleriOlustur());
                UrunleriYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"âŒ ÃœrÃ¼n eklenirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            }
        };
        ekleForm.Controls.Add(kaydetButonu);

        // Ä°PTAL BUTONU
        var iptalButonu = new Button
        {
            Text = "âŒ Ä°ptal",
            Location = new Point(260, 310),
            Width = 100,
            Height = 32,
            BackColor = Color.LightCoral
        };
        iptalButonu.Click += (s, e) => ekleForm.Close();
        ekleForm.Controls.Add(iptalButonu);

        ekleForm.ShowDialog();
    }

    /// <summary>
    /// "Kategori Ekle" butonuna tÄ±klandÄ±ÄŸÄ±nda Ã§alÄ±ÅŸÄ±r.
    /// Yeni kategori eklemek iÃ§in dialog formu aÃ§ar ve veritabanÄ±na kaydeder.
    /// </summary>
    private void KategoriEkleButonu_Click(object? sender, EventArgs e)
    {
        var ekleForm = new Form
        {
            Text = "â• Yeni Kategori Ekle",
            Width = 450,
            Height = 250,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            MaximizeBox = false,
            MinimizeBox = false,
            StartPosition = FormStartPosition.CenterParent,
            Owner = this
        };

        // KATEGORÄ° ADI
        var adLabel = new Label { Text = "Kategori AdÄ± *:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var adTextBox = new TextBox { Location = new Point(150, 18), Width = 260, Height = 28 };
        ekleForm.Controls.AddRange(new Control[] { adLabel, adTextBox });

        // AÃ‡IKLAMA
        var aciklamaLabel = new Label { Text = "AÃ§Ä±klama:", Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
        var aciklamaTextBox = new TextBox { Location = new Point(150, 58), Width = 260, Height = 80, Multiline = true };
        ekleForm.Controls.AddRange(new Control[] { aciklamaLabel, aciklamaTextBox });

        // KAYDET BUTONU
        var kaydetButonu = new Button
        {
            Text = "âœ… Kaydet",
            Location = new Point(150, 160),
            Width = 100,
            Height = 32,
            BackColor = Color.LightGreen,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };
        kaydetButonu.Click += (s, e) =>
        {
            // VALIDASYON
            if (string.IsNullOrEmpty(adTextBox.Text))
            {
                MessageBox.Show("âŒ Kategori adÄ± boÅŸ olamaz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            // DUPLICATE KONTROL - AynÄ± adlÄ± kategori var mÄ±?
            var trimmedKategoriAdi = adTextBox.Text.Trim();
            var mevcutKategori = _db.Kategoriler.FirstOrDefault(k =>
                k.KategoriAdi.ToLower() == trimmedKategoriAdi.ToLower());

            if (mevcutKategori != null)
            {
                MessageBox.Show(
                    $"âŒ Bu kategori zaten mevcut!\\n\\nKategori: {mevcutKategori.KategoriAdi}",
                    "Duplicate Kategori",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                return;
            }

            try
            {
                // Yeni Kategori objesi oluÅŸtur ve DB'ye ekle
                var yeniKategori = new Kategori
                {
                    KategoriAdi = adTextBox.Text.Trim(),
                    Aciklama = aciklamaTextBox.Text.Trim()
                };

                _db.Kategoriler.Add(yeniKategori);
                _db.SaveChanges();

                MessageBox.Show("âœ… Kategori baÅŸarÄ±yla eklendi!", "BaÅŸarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                ekleForm.Close();

                KategorileriYukle();
                UrunleriYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"âŒ Kategori eklenirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
            }
        };
        ekleForm.Controls.Add(kaydetButonu);

        // Ä°PTAL BUTONU
        var iptalButonu = new Button
        {
            Text = "âŒ Ä°ptal",
            Location = new Point(260, 160),
            Width = 100,
            Height = 32,
            BackColor = Color.LightCoral
        };
        iptalButonu.Click += (s, e) => ekleForm.Close();
        ekleForm.Controls.Add(iptalButonu);

        ekleForm.ShowDialog();
    }

    /// <summary>
    /// ÃœrÃ¼n DÃ¼zenle butonuna tÄ±klandÄ±ÄŸÄ±nda Ã§alÄ±ÅŸÄ±r.
    /// SeÃ§ilen Ã¼rÃ¼nÃ¼ dÃ¼zenlemek iÃ§in dialog formu aÃ§ar ve veritabanÄ±nda gÃ¼nceller.
    /// </summary>
    private void UrunDuzenleButonu_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_urunGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("âŒ LÃ¼tfen dÃ¼zenlemek iÃ§in bir Ã¼rÃ¼n seÃ§iniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            var secilenSatir = _urunGridView.SelectedRows[0];
            var urunId = (int)secilenSatir.Cells["Id"].Value;

            // VeritabanÄ±ndan Ã¼rÃ¼nÃ¼ Ã§ek
            var urun = _db.Urunler.Include(u => u.Kategori).FirstOrDefault(u => u.Id == urunId);
            if (urun == null)
            {
                MessageBox.Show("âŒ ÃœrÃ¼n bulunamadÄ±!", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return;
            }

            var duzenleForm = new Form
            {
                Text = "âœï¸ ÃœrÃ¼nÃ¼ DÃ¼zenle",
                Width = 500,
                Height = 430,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                Owner = this
            };

            // ÃœRÃœN ADI
            var urunAdiLabel = new Label { Text = "ÃœrÃ¼n AdÄ± *:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var urunAdiTextBox = new TextBox { Location = new Point(150, 18), Width = 310, Height = 28, Text = urun.UrunAdi };
            duzenleForm.Controls.AddRange(new Control[] { urunAdiLabel, urunAdiTextBox });

            // AÃ‡IKLAMA
            var aciklamaLabel = new Label { Text = "AÃ§Ä±klama:", Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var aciklamaTextBox = new TextBox { Location = new Point(150, 58), Width = 310, Height = 80, Multiline = true, Text = urun.Aciklama ?? "" };
            duzenleForm.Controls.AddRange(new Control[] { aciklamaLabel, aciklamaTextBox });

            // FÄ°YAT
            var fiyatLabel = new Label { Text = "Fiyat (â‚º) *:", Location = new Point(20, 155), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var fiyatTextBox = new TextBox { Location = new Point(150, 153), Width = 310, Height = 28, Text = urun.Fiyat.ToString() };
            duzenleForm.Controls.AddRange(new Control[] { fiyatLabel, fiyatTextBox });

            // STOK
            var stokLabel = new Label { Text = "Stok Adedi *:", Location = new Point(20, 200), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var stokNumeric = new NumericUpDown { Location = new Point(150, 198), Width = 310, Height = 28, Value = urun.StokAdedi, Minimum = 0, Maximum = 100000 };
            duzenleForm.Controls.AddRange(new Control[] { stokLabel, stokNumeric });

            // KATEGORÄ°
            var kategoriLabel = new Label { Text = "Kategori *:", Location = new Point(20, 250), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var kategoriCombo = new ComboBox { Location = new Point(150, 248), Width = 310, Height = 28, DropDownStyle = ComboBoxStyle.DropDownList };
            kategoriCombo.DisplayMember = "KategoriAdi";
            kategoriCombo.ValueMember = "Id";
            foreach (var k in _db.Kategoriler.ToList())
            {
                kategoriCombo.Items.Add(k);
                if (k.Id == urun.KategoriId)
                    kategoriCombo.SelectedItem = k;
            }
            duzenleForm.Controls.AddRange(new Control[] { kategoriLabel, kategoriCombo });

            // KAYDET BUTONU
            var kaydetButonu = new Button
            {
                Text = "âœ… Kaydet",
                Location = new Point(150, 310),
                Width = 100,
                Height = 32,
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            kaydetButonu.Click += (s, e) =>
            {
                // VALIDASYON
                if (string.IsNullOrEmpty(urunAdiTextBox.Text))
                {
                    MessageBox.Show("âŒ ÃœrÃ¼n adÄ± boÅŸ olamaz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    return;
                }

                if (kategoriCombo.SelectedIndex < 0 || kategoriCombo.SelectedItem == null)
                {
                    MessageBox.Show("âŒ Kategori seÃ§iniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    return;
                }

                if (!decimal.TryParse(fiyatTextBox.Text, out var fiyat) || fiyat < 0)
                {
                    MessageBox.Show("âŒ GeÃ§erli bir fiyat giriniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    // ÃœrÃ¼nÃ¼ gÃ¼ncelle
                    urun.UrunAdi = urunAdiTextBox.Text.Trim();
                    urun.Aciklama = aciklamaTextBox.Text.Trim();
                    urun.Fiyat = fiyat;
                    urun.StokAdedi = (int)stokNumeric.Value;
                    urun.KategoriId = ((Kategori)kategoriCombo.SelectedItem).Id;

                    _db.SaveChanges();

                    MessageBox.Show("âœ… ÃœrÃ¼n baÅŸarÄ±yla gÃ¼ncellendir!", "BaÅŸarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    duzenleForm.Close();

                    // Verileri yenile
                    _db.Dispose();
                    _db = new MobilyaDbContext(VeritabaniYapilandirmasi.DbContextSecenekleriOlustur());
                    UrunleriYukle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"âŒ ÃœrÃ¼n gÃ¼ncellenirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                }
            };
            duzenleForm.Controls.Add(kaydetButonu);

            // Ä°PTAL BUTONU
            var iptalButonu = new Button
            {
                Text = "âŒ Ä°ptal",
                Location = new Point(260, 310),
                Width = 100,
                Height = 32,
                BackColor = Color.LightCoral
            };
            iptalButonu.Click += (s, e) => duzenleForm.Close();
            duzenleForm.Controls.Add(iptalButonu);

            duzenleForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata oluÅŸtu:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// ÃœrÃ¼n Sil butonuna tÄ±klandÄ±ÄŸÄ±nda Ã§alÄ±ÅŸÄ±r.
    /// SeÃ§ilen Ã¼rÃ¼nÃ¼ veritabanÄ±ndan siler.
    /// </summary>
    private void UrunSilButonu_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_urunGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("âŒ LÃ¼tfen silmek iÃ§in bir Ã¼rÃ¼n seÃ§iniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            var secilenSatir = _urunGridView.SelectedRows[0];
            var urunId = (int)secilenSatir.Cells["Id"].Value;
            var urunAdi = secilenSatir.Cells["UrunAdi"].Value.ToString();

            // Onay sor
            var result = MessageBox.Show(
                $"Emin misiniz? '{urunAdi}' Ã¼rÃ¼nÃ¼ kalÄ±cÄ± olarak silinecektir!",
                "Silme OnayÄ±",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
                return;

            var urun = _db.Urunler.FirstOrDefault(u => u.Id == urunId);
            if (urun != null)
            {
                _db.Urunler.Remove(urun);
                _db.SaveChanges();

                MessageBox.Show("âœ… ÃœrÃ¼n baÅŸarÄ±yla silindi!", "BaÅŸarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                // Verileri yenile
                _db.Dispose();
                _db = new MobilyaDbContext(VeritabaniYapilandirmasi.DbContextSecenekleriOlustur());
                UrunleriYukle();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"âŒ ÃœrÃ¼n silinirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Kategori DÃ¼zenle butonuna tÄ±klandÄ±ÄŸÄ±nda Ã§alÄ±ÅŸÄ±r.
    /// SeÃ§ilen kategoriyi dÃ¼zenlemek iÃ§in dialog formu aÃ§ar ve veritabanÄ±nda gÃ¼nceller.
    /// </summary>
    private void KategoriDuzenleButonu_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_kategoriGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("âŒ LÃ¼tfen dÃ¼zenlemek iÃ§in bir kategori seÃ§iniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            var secilenSatir = _kategoriGridView.SelectedRows[0];
            var kategoriId = (int)secilenSatir.Cells["Id"].Value;

            // VeritabanÄ±ndan kategoriyi Ã§ek
            var kategori = _db.Kategoriler.FirstOrDefault(k => k.Id == kategoriId);
            if (kategori == null)
            {
                MessageBox.Show("âŒ Kategori bulunamadÄ±!", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                return;
            }

            var duzenleForm = new Form
            {
                Text = "âœï¸ Kategoriyi DÃ¼zenle",
                Width = 450,
                Height = 250,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                StartPosition = FormStartPosition.CenterParent,
                Owner = this
            };

            // KATEGORÄ° ADI
            var adLabel = new Label { Text = "Kategori AdÄ± *:", Location = new Point(20, 20), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var adTextBox = new TextBox { Location = new Point(150, 18), Width = 260, Height = 28, Text = kategori.KategoriAdi };
            duzenleForm.Controls.AddRange(new Control[] { adLabel, adTextBox });

            // AÃ‡IKLAMA
            var aciklamaLabel = new Label { Text = "AÃ§Ä±klama:", Location = new Point(20, 60), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            var aciklamaTextBox = new TextBox { Location = new Point(150, 58), Width = 260, Height = 80, Multiline = true, Text = kategori.Aciklama ?? "" };
            duzenleForm.Controls.AddRange(new Control[] { aciklamaLabel, aciklamaTextBox });

            // KAYDET BUTONU
            var kaydetButonu = new Button
            {
                Text = "âœ… Kaydet",
                Location = new Point(150, 160),
                Width = 100,
                Height = 32,
                BackColor = Color.LightGreen,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            kaydetButonu.Click += (s, e) =>
            {
                // VALIDASYON
                if (string.IsNullOrEmpty(adTextBox.Text))
                {
                    MessageBox.Show("âŒ Kategori adÄ± boÅŸ olamaz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    return;
                }

                try
                {
                    // Kategoriyi gÃ¼ncelle
                    kategori.KategoriAdi = adTextBox.Text.Trim();
                    kategori.Aciklama = aciklamaTextBox.Text.Trim();

                    _db.SaveChanges();

                    MessageBox.Show("âœ… Kategori baÅŸarÄ±yla gÃ¼ncellendir!", "BaÅŸarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    duzenleForm.Close();

                    KategorileriYukle();
                    UrunleriYukle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"âŒ Kategori gÃ¼ncellenirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                }
            };
            duzenleForm.Controls.Add(kaydetButonu);

            // Ä°PTAL BUTONU
            var iptalButonu = new Button
            {
                Text = "âŒ Ä°ptal",
                Location = new Point(260, 160),
                Width = 100,
                Height = 32,
                BackColor = Color.LightCoral
            };
            iptalButonu.Click += (s, e) => duzenleForm.Close();
            duzenleForm.Controls.Add(iptalButonu);

            duzenleForm.ShowDialog();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata oluÅŸtu:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Kategori Sil butonuna tÄ±klandÄ±ÄŸÄ±nda Ã§alÄ±ÅŸÄ±r.
    /// SeÃ§ilen kategoriyi veritabanÄ±ndan siler (dÄ±ÅŸ anahtar kontrolÃ¼ ile).
    /// </summary>
    private void KategoriSilButonu_Click(object? sender, EventArgs e)
    {
        try
        {
            if (_kategoriGridView.SelectedRows.Count == 0)
            {
                MessageBox.Show("âŒ LÃ¼tfen silmek iÃ§in bir kategori seÃ§iniz!", "UyarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                return;
            }

            var secilenSatir = _kategoriGridView.SelectedRows[0];
            var kategoriId = (int)secilenSatir.Cells["Id"].Value;
            var kategoriAdi = secilenSatir.Cells["KategoriAdi"].Value.ToString();

            // Bu kategoriye ait Ã¼rÃ¼n var mÄ± kontrol et
            var Ã¼rÃ¼nSayÄ±sÄ± = _db.Urunler.Count(u => u.KategoriId == kategoriId);
            if (Ã¼rÃ¼nSayÄ±sÄ± > 0)
            {
                MessageBox.Show(
                    $"âŒ Bu kategoriye ait {Ã¼rÃ¼nSayÄ±sÄ±} Ã¼rÃ¼n bulunmaktadÄ±r!\nKategoriyi silmeden Ã¶nce Ã¼rÃ¼nleri siliniz veya baÅŸka kategoriye taÅŸÄ±yÄ±nÄ±z.",
                    "Silinemezse",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                return;
            }

            // Onay sor
            var result = MessageBox.Show(
                $"Emin misiniz? '{kategoriAdi}' kategorisi kalÄ±cÄ± olarak silinecektir!",
                "Silme OnayÄ±",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.No)
                return;

            var kategori = _db.Kategoriler.FirstOrDefault(k => k.Id == kategoriId);
            if (kategori != null)
            {
                _db.Kategoriler.Remove(kategori);
                _db.SaveChanges();

                MessageBox.Show("âœ… Kategori baÅŸarÄ±yla silindi!", "BaÅŸarÄ±", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                KategorileriYukle();
                UrunleriYukle();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"âŒ Kategori silinirken hata:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// ÃœrÃ¼n DataGridView'de buton tÄ±klanmasÄ±nÄ± iÅŸler
    /// </summary>
    private void UrunGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        try
        {
            // Header tÄ±klanmÄ±ÅŸsa veya geÃ§ersiz satÄ±rsa Ã§Ä±k
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // TÄ±klanan kolonu kontrol et
            var columnName = _urunGridView.Columns[e.ColumnIndex].Name;

            if (columnName == "Duzenle")
            {
                // DÃ¼zenle butonuna tÄ±klandÄ±
                _urunGridView.ClearSelection();
                _urunGridView.Rows[e.RowIndex].Selected = true;
                UrunDuzenleButonu_Click(null, EventArgs.Empty);
            }
            else if (columnName == "Sil")
            {
                // Sil butonuna tÄ±klandÄ±
                _urunGridView.ClearSelection();
                _urunGridView.Rows[e.RowIndex].Selected = true;
                UrunSilButonu_Click(null, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata oluÅŸtu:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }
    }

    /// <summary>
    /// Kategori DataGridView'de buton tÄ±klanmasÄ±nÄ± iÅŸler
    /// </summary>
    private void KategoriGridView_CellClick(object? sender, DataGridViewCellEventArgs e)
    {
        try
        {
            // Header tÄ±klanmÄ±ÅŸsa veya geÃ§ersiz satÄ±rsa Ã§Ä±k
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            // TÄ±klanan kolonu kontrol et
            var columnName = _kategoriGridView.Columns[e.ColumnIndex].Name;

            if (columnName == "Duzenle")
            {
                // DÃ¼zenle butonuna tÄ±klandÄ±
                _kategoriGridView.ClearSelection();
                _kategoriGridView.Rows[e.RowIndex].Selected = true;
                KategoriDuzenleButonu_Click(null, EventArgs.Empty);
            }
            else if (columnName == "Sil")
            {
                // Sil butonuna tÄ±klandÄ±
                _kategoriGridView.ClearSelection();
                _kategoriGridView.Rows[e.RowIndex].Selected = true;
                KategoriSilButonu_Click(null, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Hata oluÅŸtu:\n{ex.Message}", "Hata", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _db?.Dispose();
        }
        base.Dispose(disposing);
    }
}

