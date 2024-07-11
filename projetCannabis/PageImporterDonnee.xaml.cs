using Microsoft.Data.SqlClient;
using QRCoder;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClosedXML.Excel;
using System.IO;
//using OfficeOpenXml;


namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageImporterDonnee.xaml
    /// </summary>
    public partial class PageImporterDonnee : Page
    {
        public ObservableCollection<Plantule> Plantules { get; set; }
        private string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

        public PageImporterDonnee()
        {
            InitializeComponent();
            Plantules = new ObservableCollection<Plantule>();
            dataGridPlantules.ItemsSource = Plantules;
            LoadPlantulesFromDatabase();
        }

        private void LoadPlantulesFromDatabase()
        {
            Plantules.Clear();

            string query = @"SELECT identification, description, date_reception, date_retrait, responsable_decontamination, 
                            actif_Inactif, provenance, entreposage, etat_sante, stade_vie, note,
                            qte_ajoutee, qte_retiree, item_retire_inventaire,
                            EstArchive FROM Plantule";

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
                            QteAjoutee = reader.IsDBNull(11) ? (int?)null : reader.GetInt32(11),
                            QteRetiree = reader.IsDBNull(12) ? (int?)null : reader.GetInt32(12),
                            ItemRetireInventaire = reader.IsDBNull(13) ? null : reader.GetString(13),
                            EstArchive = reader.GetBoolean(14), // Assurez-vous que le numéro correspond à la position de la colonne dans le SELECT

                            // Autres attributs...
                            QRCode = GenerateQRCode(reader.GetString(0))



                        };
                        //plantule.QRCodeFilePath = GenerateQRCode(plantule);

                        Plantules.Add(plantule);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Une erreur est survenue lors du chargement des données: {ex.Message}");
            }
        }


        private BitmapImage GenerateQRCode(string text)
        {
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);
            Bitmap qrCodeImage = qrCode.GetGraphic(20);

            // Convertir le Bitmap en BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream memory = new MemoryStream())
            {
                qrCodeImage.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = memory;
                bitmapImage.EndInit();
            }

            return bitmapImage;
        }


        private void BtnImporter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Fichiers Excel (*.xlsx)|*.xlsx|Tous les fichiers (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    using (var workbook = new XLWorkbook(filePath))
                    {
                        var worksheet = workbook.Worksheet(1);

                        if (!WorksheetHasHeaders(worksheet))
                        {
                            // Ajouter les entêtes si elles sont absentes
                            AddHeadersToWorksheet(worksheet);
                        }

                        // Lire et importer les données depuis le fichier Excel
                        ImporterDonneesDansExcel(filePath);
                    }

                    // Mettre à jour l'interface avec les données importées
                    dataGridPlantules.ItemsSource = null; // Désactive le binding temporairement
                    dataGridPlantules.ItemsSource = Plantules; // Réactive le binding avec les nouvelles données
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue lors de l'importation du fichier Excel : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        bool WorksheetHasHeaders(IXLWorksheet worksheet)
        {
            // Vérifiez si la première ligne contient les entêtes nécessaires
            // Exemple basique : vérification des noms de colonnes attendus
            return worksheet.Cell(1, 1).Value.ToString() == "Identification"
                && worksheet.Cell(1, 2).Value.ToString() == "Description"
                && worksheet.Cell(1, 3).Value.ToString() == "Date de réception"
                && worksheet.Cell(1, 4).Value.ToString() == "Date de retrait"
                && worksheet.Cell(1, 5).Value.ToString() == "Responsable Décontamination"
                && worksheet.Cell(1, 6).Value.ToString() == "Actif/Inactif"
                && worksheet.Cell(1, 7).Value.ToString() == "Provenance"
                && worksheet.Cell(1, 8).Value.ToString() == "Entreposage"
                && worksheet.Cell(1, 9).Value.ToString() == "État de santé"
                && worksheet.Cell(1, 10).Value.ToString() == "Stade de vie"
                && worksheet.Cell(1, 11).Value.ToString() == "Note"
                && worksheet.Cell(1, 12).Value.ToString() == "Item retiré inventaire";
        }


        void AddHeadersToWorksheet(IXLWorksheet worksheet)
        {
            // Ajoutez les entêtes à la première ligne de la feuille de calcul
            worksheet.Cell(1, 1).Value = "Identification";
            worksheet.Cell(1, 2).Value = "Description";
            worksheet.Cell(1, 3).Value = "Date de réception";
            worksheet.Cell(1, 4).Value = "Date de retrait";
            worksheet.Cell(1, 5).Value = "Responsable Décontamination";
            worksheet.Cell(1, 6).Value = "Actif/Inactif";
            worksheet.Cell(1, 7).Value = "Provenance";
            worksheet.Cell(1, 8).Value = "Entreposage";
            worksheet.Cell(1, 9).Value = "État de santé";
            worksheet.Cell(1, 10).Value = "Stade de vie";
            worksheet.Cell(1, 11).Value = "Note";
            worksheet.Cell(1, 12).Value = "Item retiré inventaire";
            worksheet.Cell(1, 13).Value = "Est Archivée";

            // Mettez en gras les entêtes
            worksheet.Row(1).Style.Font.Bold = true;

            // Sauvegardez les modifications dans le fichier Excel
            worksheet.Workbook.Save();
        }


        void ImporterDonneesDansExcel(string filePath)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(1);

                    // Vérifiez et ajoutez les entêtes si elles sont absentes
                    if (!WorksheetHasHeaders(worksheet))
                    {
                        AddHeadersToWorksheet(worksheet);
                    }

                    // Ajoutez les données à partir de la liste Plantules
                    int rowNum = worksheet.LastRowUsed().RowNumber() + 1; // Trouvez la prochaine ligne vide
                    foreach (var plantule in Plantules)
                    {
                        worksheet.Cell(rowNum, 1).Value = plantule.Identification;
                        worksheet.Cell(rowNum, 2).Value = plantule.Description;
                        worksheet.Cell(rowNum, 3).Value = plantule.DateReception;
                        worksheet.Cell(rowNum, 4).Value = plantule.DateRetrait;
                        worksheet.Cell(rowNum, 5).Value = plantule.ResponsableDecontamination;
                        worksheet.Cell(rowNum, 6).Value = plantule.Actif_Inactif;
                        worksheet.Cell(rowNum, 7).Value = plantule.Provenance;
                        worksheet.Cell(rowNum, 8).Value = plantule.Entreposage;
                        worksheet.Cell(rowNum, 9).Value = plantule.EtatSante;
                        worksheet.Cell(rowNum, 10).Value = plantule.StadeDeVie;
                        worksheet.Cell(rowNum, 11).Value = plantule.Note;
                        worksheet.Cell(rowNum, 12).Value = plantule.ItemRetireInventaire;
                        worksheet.Cell(rowNum, 13).Value = plantule.EstArchive ? "Oui" : "Non"; // Ajoutez EstArchive

                        // Générez le QR Code et ajoutez-le à la feuille Excel
                        BitmapImage qrCodeImage = GenerateQRCode(plantule.Identification);
                        using (MemoryStream ms = new MemoryStream())
                        {
                            BitmapEncoder encoder = new PngBitmapEncoder();
                            encoder.Frames.Add(BitmapFrame.Create(qrCodeImage));
                            encoder.Save(ms);
                            using (MemoryStream ms2 = new MemoryStream(ms.ToArray()))
                            {
                                var picture = worksheet.AddPicture(ms2)
                                    .MoveTo(worksheet.Cell(rowNum, 14))
                                    .WithSize(80, 80);
                            }

                        }

                        // Appliquez une couleur de fond en fonction de EstArchive
                        if (plantule.EstArchive)
                        {
                            worksheet.Row(rowNum).Style.Fill.BackgroundColor = XLColor.LightGray;
                        }

                        // Appliquez une couleur de fond en fonction de l'état de santé
                        worksheet.Cell(rowNum, 9).Style.Fill.BackgroundColor = GetHealthStateColor(plantule.EtatSante);

                        rowNum++;
                    }

                    // Sauvegardez les modifications dans le fichier Excel
                    workbook.SaveAs(filePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue lors de l'exportation des données vers Excel : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        XLColor GetHealthStateColor(string etatSante)
        {
            switch (etatSante)
            {
                case "Bonne santé":
                    return XLColor.FromHtml("#00FF00"); // Vert
                case "Moyenne santé":
                    return XLColor.FromHtml("#FFFF00"); // Jaune
                case "Mauvaise santé":
                    return XLColor.FromHtml("#FFA500"); // Orange
                case "Santé en danger":
                    return XLColor.FromHtml("#FF0000"); // Rouge
                default:
                    return XLColor.Transparent;
            }
        }


        private void BtnImprimer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Configuration de l'impression
                    printDialog.PrintVisual(dataGridPlantules, "Impression de la liste des plantules");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}



