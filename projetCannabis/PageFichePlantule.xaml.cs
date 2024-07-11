using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using ZXing;
using ZXing.QrCode;
using System.Drawing;
using System.IO;
using ZXing.Windows.Compatibility;
using System.Drawing.Imaging;






namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageFichePlantule.xaml
    /// </summary>
    public partial class PageFichePlantule : Page
    {
        public Plantule Plantule { get; set; }
        public ObservableCollection<Plantule> Plantules { get; set; }
        private GestionDonnee gestionDonnee;

        public PageFichePlantule()
        {
            InitializeComponent();
       
            DataContext = this;
            gestionDonnee = new GestionDonnee();


        }

        // la chaîne de connexion
        public static string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";


        SqlConnection connection = new SqlConnection(ConnectionString);

        public PageFichePlantule(Plantule selectedPlantule)
        {
            InitializeComponent();
            Plantule = selectedPlantule; // Initialisez Plantule avant de définir DataContext
            DataContext = Plantule;
            GenerateQRCode(selectedPlantule.Identification);
            gestionDonnee = new GestionDonnee();
        }




        public class CannabisContext : DbContext
        {
            public DbSet<Plantule> Plantules { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;");
            }

            // Autres configurations ou méthodes de modèle si nécessaire
        }

        private void UpdatePlantuleDetails()
        {
            // Mettez à jour les détails de la plantule ici
            gestionDonnee.UpdatePlantule(Plantule);
        }

        private Bitmap GenerateQRCode(string content)
        {
            var qrWriter = new BarcodeWriter();
            qrWriter.Format = BarcodeFormat.QR_CODE;
            qrWriter.Options = new QrCodeEncodingOptions
            {
                Width = 100,
                Height = 100
            };

            // Générer le code QR en bitmap
            var qrBitmap = qrWriter.Write(content);

            // Assigner le bitmap à qrCodeImage (si nécessaire)
            qrCodeImage.Source = ConvertBitmapToBitmapImage(qrBitmap);

            return qrBitmap;
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
            }
            return bitmapImage;
        }

        private void BtnImprimerQR_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Plantule plantule)
            {
                // Génération du code QR avec l'identification de la plantule
                Bitmap qrCodeBitmap = GenerateQRCode(plantule.Identification);

                // Imprimer le code QR avec l'identification
                PrintQRCode(qrCodeBitmap, plantule.Identification);
            }
        }




        private void PrintQRCode(Bitmap qrCodeBitmap, string identification)
        {
            // Convertir le Bitmap du code QR en BitmapImage
            BitmapImage bitmapImage = ConvertBitmapToBitmapImage(qrCodeBitmap);

            // Créer une image WPF à partir du BitmapImage
            System.Windows.Controls.Image imageQR = new System.Windows.Controls.Image();
            imageQR.Source = bitmapImage;

            // Créer un texte avec l'identification
            TextBlock textIdentification = new TextBlock();
            textIdentification.Text = identification;
            textIdentification.FontWeight = FontWeights.Bold; // Mettre le texte en gras
            textIdentification.FontSize = 16; // Agrandir la taille de la police
            textIdentification.TextAlignment = TextAlignment.Center; // Centrer le texte
            textIdentification.Margin = new Thickness(0, 10, 0, 0);

            // Créer un StackPanel pour contenir l'image QR et le texte d'identification
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(imageQR);
            stackPanel.Children.Add(textIdentification);

            // Imprimer le StackPanel
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintVisual(stackPanel, "Impression du code QR et de l'identification");
            }
        }




        private void BtnHistorique(object sender, RoutedEventArgs e) 
        { 
            if (DataContext is Plantule plantule) 
            { PageHistoriquePlantule pageHistorique = new PageHistoriquePlantule(plantule.Identification);
                NavigationService.Navigate(pageHistorique);
            } 
        }




        private int GetNumberOfModifications(string identification)
        {
            int numberOfModifications = 0;

            string query = "SELECT COUNT(*) AS NombreDeModification " +
                           "FROM Modification " +
                           "WHERE identification = @Identification";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", identification);
                    connection.Open();
                    numberOfModifications = (int)command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la récupération du nombre de modifications : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return numberOfModifications;
        }


        private void DeletePlantuleFromDatabase(Plantule plantule)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand("DELETE FROM Plantule WHERE Identification = @Identification", connection);
                    command.Parameters.AddWithValue("@Identification", plantule.Identification);
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("La plantule a été détruite avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Aucune plantule n'a été trouvée avec cette identification.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de la plantule: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void BtnDetruire_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Plantule plantuleToDelete)
            {
                // Supprimer la plantule de la base de données
                DeletePlantuleFromDatabase(plantuleToDelete);

                // Rafraîchir la liste des plantules dans le DataGrid
                if (NavigationService != null && NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
        }


        private void BtnGerer_Click(object sender, RoutedEventArgs e)
        {
            // Créer une instance de PageGererPlantule
            PageGererPlantule pageGerer = new PageGererPlantule();

            // Passer la plante actuelle à gérer à PageGererPlantule
            pageGerer.ReceivePlantule(this.DataContext as Plantule);

            // Naviguer vers la page PageGererPlantule
            NavigationService.Navigate(pageGerer);
        }


        // Supposons que vous ayez une méthode pour naviguer vers la page de modification
        private void BtnModifier_Click(object sender, RoutedEventArgs e)
        {
            // Supposons que la plantule actuelle soit stockée dans une variable plantuleActuelle
            var plantuleActuelle = this.DataContext as Plantule;
            if (plantuleActuelle != null)
            {
                // Naviguer vers la page de modification en passant l'objet plantule
                NavigationService.Navigate(new PageModifierPlantule(plantuleActuelle));
            }
            else
            {
                MessageBox.Show("Aucune plantule sélectionnée.");
            }
        }

      


        private void BtnArchiver_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Plantule plantule)
            {
                ArchiverPlantule(plantule);

                // Naviguer vers la page d'archives
                PageArchivePlantule pageArchive = new PageArchivePlantule();
                pageArchive.Archives.Add(plantule);

                // Retirer la plantule de la liste actuelle
                if (NavigationService?.Content is PageChercherPlantule pageRecherche)
                {
                    pageRecherche.RemovePlantule(plantule);
                }

                // Appeler la méthode de rafraîchissement du DataGrid Archives
                pageArchive.RefreshArchivesDataGrid();

                // Naviguer vers la page d'archives si nécessaire
                NavigationService?.Navigate(pageArchive);
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
    

   

       
    }
}





