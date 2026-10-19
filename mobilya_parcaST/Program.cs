using Microsoft.EntityFrameworkCore;
using mobilya_parcaST.Data;

namespace mobilya_parcaST
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Uygulama başlamadan önce veritabanını oluştur
            try
            {
                using (var db = new MobilyaDbContext(VeritabaniYapilandirmasi.DbContextSecenekleriOlustur()))
                {
                    // Veritabanını oluştur (varsa hiçbir şey yapmaz)
                    db.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veritabanı başlatma hatası:\n" + ex.Message, "Hata", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();
            Application.Run(new Form1());
        }
    }
}