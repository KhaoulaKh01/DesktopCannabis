using Microsoft.Data.SqlClient;
using QRCoder;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;




namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageArchivePlantule.xaml
    /// </summary>
    public partial class PageArchivePlantule : Page
    {
        public ObservableCollection<Plantule> Plantules { get; set; }
        public ObservableCollection<Plantule> Archives { get; set; }
        // Déclarer une propriété pour l'identification de la plantule si nécessaire
        private string _identification;
        private string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
        public PageArchivePlantule()
        {
            InitializeComponent();
            Archives = new ObservableCollection<Plantule>();
            DataContext = Archives;
            Plantules = new ObservableCollection<Plantule>();
            dataGridArchives.ItemsSource = Archives;

            LoadArchivedPlantulesFromDatabase();
        }

        // Constructeur avec un argument pour l'identification
        public PageArchivePlantule(string identification) : this()
        {
            _identification = identification;
            // Utilisez l'identification pour charger les données ou effectuer d'autres opérations nécessaires
        }



        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string etatSante)
            {
                System.Diagnostics.Debug.WriteLine($"EtatSante: {etatSante}");
                switch (etatSante)
                {
                    case "Bonne santé":
                        return Brushes.Green;
                    case "Moyenne santé":
                        return Brushes.Yellow;
                    case "Mauvaise santé":
                        return Brushes.Orange;
                    case "Santé en danger":
                        return Brushes.Red;
                    default:
                        return Brushes.Transparent;
                }
            }
            return Brushes.Transparent;
        }


        private void LoadArchivedPlantulesFromDatabase()
        {
            Archives.Clear(); // Efface les archives précédentes au cas où

            string query = "SELECT identification, description, date_reception, date_retrait, responsable_decontamination, actif_Inactif, provenance, entreposage, etat_sante, stade_vie, note FROM Plantule WHERE EstArchive = 1";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Plantule plantule = new Plantule
                        {
                            Identification = reader.IsDBNull(0) ? null : reader.GetString(0),
                            Description = reader.IsDBNull(1) ? null : reader.GetString(1),
                            DateReception = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                            DateRetrait = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            ResponsableDecontamination = reader.IsDBNull(4) ? null : reader.GetString(4),
                            Actif_Inactif = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                            Provenance = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Entreposage = reader.IsDBNull(7) ? null : reader.GetString(7),
                            EtatSante = reader.IsDBNull(8) ? null : reader.GetString(8),
                            StadeDeVie = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Note = reader.IsDBNull(10) ? null : reader.GetString(10),
                            QRCode = GenerateQRCode(reader.GetString(0)) // Génération du QR code
                    };

                        Archives.Add(plantule);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des plantules archivées: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Méthode pour générer le code QR
        public BitmapImage GenerateQRCode(string content)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            using (System.Drawing.Bitmap qrCodeImage = qrCode.GetGraphic(20))
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



        public void RefreshArchivesDataGrid()
        {
            Archives.Clear();
            LoadArchivedPlantulesFromDatabase();
        }



        private void BtnDesarchiver_Click(object sender, RoutedEventArgs e)
        {
            if (dataGridArchives.SelectedItem is Plantule selectedPlantule)
            {
                // Retirer la plantule des archives
                Archives.Remove(selectedPlantule);

                // Mettre à jour la base de données pour marquer la plantule comme non archivée
                UnarchivePlantule(selectedPlantule);

                // Ajouter la plantule à la liste de recherche
                if (NavigationService?.Content is PageChercherPlantule pageRecherche)
                {
                    pageRecherche.Plantules.Add(selectedPlantule);
                }

                // Appeler la méthode de rafraîchissement du DataGrid Archives
                RefreshArchivesDataGrid();
            }
        }

        private void UnarchivePlantule(Plantule plantule)
        {
            string query = "UPDATE Plantule SET EstArchive = 0 WHERE Identification = @Identification";

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
                MessageBox.Show($"Erreur lors du désarchivage de la plantule: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void dataGridArchives_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}





