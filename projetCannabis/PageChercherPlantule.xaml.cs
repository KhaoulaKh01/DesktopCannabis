using Microsoft.Data.SqlClient;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using ZXing;
using ZXing.Common;
using ZXing.Windows.Compatibility;
using System.IO;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using static QRCoder.PayloadGenerator.ShadowSocksConfig;


namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageChercherPlantule.xaml
    /// </summary>
    public partial class PageChercherPlantule : Page
    {
        public ObservableCollection<Plantule> Plantules { get; set; }

        public PageChercherPlantule()
        {
            InitializeComponent();
            Plantules = new ObservableCollection<Plantule>();
            DataContext = this;

            

            // Charge les plantules depuis la base de données au démarrage
            LoadPlantulesFromDatabase();
        }

        // la chaîne de connexion
        public static string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

        SqlConnection connection = new SqlConnection(ConnectionString);



        private void BtnRechercherParQR(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png;";

            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                // Utiliser ZXing pour lire le code QR depuis l'image
                Bitmap bitmap = new Bitmap(filePath);
                var barcodeReader = new BarcodeReader();
                var result = barcodeReader.Decode(bitmap);

                if (result != null)
                {
                    string identification = result.Text;

                    // Effectuer une recherche basée sur l'identification récupérée
                    ObservableCollection<Plantule> plantulesRecherchees = SearchPlantulesByIdentification(identification);

                    if (plantulesRecherchees.Count > 0)
                    {
                        dataGridPlantules.ItemsSource = plantulesRecherchees;
                    }
                    else
                    {
                        MessageBox.Show($"Aucune plantule trouvée avec l'identification '{identification}'.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        dataGridPlantules.ItemsSource = null;
                    }
                }
                else
                {
                    MessageBox.Show("Aucun QR code trouvé dans l'image.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        private ObservableCollection<Plantule> SearchPlantulesByIdentification(string identification)
        {
            ObservableCollection<Plantule> plantules = new ObservableCollection<Plantule>();

            string query = "SELECT identification, description, date_reception, date_retrait, responsable_decontamination, actif_Inactif, provenance, entreposage, etat_sante, stade_vie, note FROM Plantule WHERE EstArchive = 0 AND identification LIKE @Identification";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", $"{identification}%"); // % signifie n'importe quelle chaîne de caractères suivant l'identification

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Plantule plantule = new Plantule();

                        plantule.Identification = reader.IsDBNull(0) ? null : reader.GetString(0);
                        plantule.Description = reader.IsDBNull(1) ? null : reader.GetString(1);
                        plantule.DateReception = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);
                        plantule.DateRetrait = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                        plantule.ResponsableDecontamination = reader.IsDBNull(4) ? null : reader.GetString(4);
                        plantule.Actif_Inactif = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                        plantule.Provenance = reader.IsDBNull(6) ? null : reader.GetString(6);
                        plantule.Entreposage = reader.IsDBNull(7) ? null : reader.GetString(7);
                        plantule.EtatSante = reader.IsDBNull(8) ? null : reader.GetString(8); // Garder comme string pour stockage
                        plantule.StadeDeVie = reader.IsDBNull(9) ? null : reader.GetString(9);
                        plantule.Note = reader.IsDBNull(10) ? null : reader.GetString(10);
                        plantule.QRCode = GenerateQRCode(plantule.Identification); // Génération du QR code à partir de l'identification

                        plantules.Add(plantule);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la recherche des plantules: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return plantules;
        }




        private void BtnRechercherIdentifiant(object sender, RoutedEventArgs e)
        {
            string identification = txtIdentification.Text.Trim();

            if (string.IsNullOrEmpty(identification))
            {
                MessageBox.Show("Veuillez entrer une valeur d'identification pour la recherche.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Plantules.Clear(); // Efface les plantules précédentes au cas où

            string query = "SELECT identification, description, date_reception, date_retrait, responsable_decontamination, actif_Inactif, provenance, entreposage, etat_sante, stade_vie, note  FROM Plantule WHERE EstArchive = 0 AND identification LIKE @Identification";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", $"{identification}%"); // % signifie n'importe quelle chaîne de caractères suivant l'identification

                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Plantule plantule = new Plantule();

                        plantule.Identification = reader.IsDBNull(0) ? null : reader.GetString(0);
                        plantule.Description = reader.IsDBNull(1) ? null : reader.GetString(1);
                        plantule.DateReception = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);
                        plantule.DateRetrait = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                        plantule.ResponsableDecontamination = reader.IsDBNull(4) ? null : reader.GetString(4);
                        plantule.Actif_Inactif = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                        plantule.Provenance = reader.IsDBNull(6) ? null : reader.GetString(6);
                        plantule.Entreposage = reader.IsDBNull(7) ? null : reader.GetString(7);
                        plantule.EtatSante = reader.IsDBNull(8) ? null : reader.GetString(8); // Garder comme string pour stockage
                        plantule.StadeDeVie = reader.IsDBNull(9) ? null : reader.GetString(9);
                        plantule.Note = reader.IsDBNull(10) ? null : reader.GetString(10);
                        plantule.QRCode = GenerateQRCode(plantule.Identification); // Génération du QR code à partir de l'identification

                        Plantules.Add(plantule);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des plantules: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        // Méthode pour générer le code QR
        public BitmapImage GenerateQRCode(string content)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            using (Bitmap qrCodeImage = qrCode.GetGraphic(20))
            {
                MemoryStream ms = new MemoryStream();
                qrCodeImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Position = 0;

                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = ms;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }
        private void LoadPlantulesFromDatabase()
        {
            Plantules.Clear(); // Efface les plantules précédentes au cas où

            string query = "SELECT identification, description, date_reception, date_retrait, responsable_decontamination, actif_Inactif, provenance, entreposage, etat_sante, stade_vie, note  FROM Plantule WHERE EstArchive = 0";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Plantule plantule = new Plantule();

                        plantule.Identification = reader.IsDBNull(0) ? null : reader.GetString(0);
                        plantule.Description = reader.IsDBNull(1) ? null : reader.GetString(1);
                        plantule.DateReception = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2);
                        plantule.DateRetrait = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3);
                        plantule.ResponsableDecontamination = reader.IsDBNull(4) ? null : reader.GetString(4);
                        plantule.Actif_Inactif = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                        plantule.Provenance = reader.IsDBNull(6) ? null : reader.GetString(6);
                        plantule.Entreposage = reader.IsDBNull(7) ? null : reader.GetString(7);
                        plantule.EtatSante = reader.IsDBNull(8) ? null : reader.GetString(8); // Garder comme string pour stockage
                        plantule.StadeDeVie = reader.IsDBNull(9) ? null : reader.GetString(9);
                        plantule.Note = reader.IsDBNull(10) ? null : reader.GetString(10);
                        plantule.QRCode = GenerateQRCode(reader.GetString(0)); // Génération du QR code

                        Plantules.Add(plantule);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des plantules: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private System.Windows.Media.Brush ConvertEtatSanteToColor(string etatSante)
        {
            switch (etatSante)
            {
                case "Bonne santé":
                    return System.Windows.Media.Brushes.Green;
                case "Moyenne santé":
                    return System.Windows.Media.Brushes.Yellow;
                case "Mauvaise santé":
                    return System.Windows.Media.Brushes.Orange;
                case "Santé en danger":
                    return System.Windows.Media.Brushes.Red;
                default:
                    return System.Windows.Media.Brushes.Transparent;
            }
        }


        private void ArchiverPlantule(Plantule plantule)
        {
            string query = "UPDATE Plantule SET EstArchive = 1 WHERE Identification = @Identification";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", plantule.Identification);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'archivage de la plantule: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    

         private void DataGridPlantules_SelectionChanged(object sender, SelectionChangedEventArgs e)
          {
              if (dataGridPlantules.SelectedItem is Plantule selectedPlantule)
              {
                  // Naviguer vers la page de détails de la plantule
                  PageFichePlantule pageFichePlantule = new PageFichePlantule(selectedPlantule);
                  NavigationService.Navigate(pageFichePlantule);

              }
          }

        public void RemovePlantule(Plantule plantule)
        {
            Plantules.Remove(plantule);
        }

    }

}