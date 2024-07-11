using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;





namespace projetCannabis
{
    public partial class PageHistoriquePlantule : Page
    {
        public ObservableCollection<HistoriqueModifications> HistoriqueModifications { get; set; }
        private string currentIdentification;
        public PageHistoriquePlantule(string identification)
        {
            InitializeComponent();
            DataContext = this;
            HistoriqueModifications = new ObservableCollection<HistoriqueModifications>();
            ChargerHistorique(identification);
            currentIdentification = identification;
        }

        private void ChargerHistorique(string identification)
        {
            HistoriqueModifications.Clear(); // Assurez-vous de vider la collection avant de la recharger

            using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;"))
            {
                try
                {
                    connection.Open();
                    string query = "SELECT * FROM HistoriqueModifications WHERE Identification = @Identification ORDER BY DateModification";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", identification);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        HistoriqueModifications.Add(new HistoriqueModifications
                        {
                            Id = reader.GetInt32(0),
                            Identification = reader.GetString(1),
                            Description = reader.GetString(2),
                            DateModification = reader.GetDateTime(3),
                            Provenance = reader.IsDBNull(4) ? null : reader.GetString(4),
                            EtatSante = reader.IsDBNull(5) ? null : reader.GetString(5),
                            StadeVie = reader.IsDBNull(6) ? null : reader.GetString(6),
                            ActifInactif = reader.GetInt32(7),
                            Entreposage = reader.IsDBNull(8) ? null : reader.GetString(8),
                            ItemRetireInventaire = reader.IsDBNull(9) ? null : reader.GetString(9),
                            ResponsableDecontamination = reader.IsDBNull(10) ? null : reader.GetString(10),
                            Note = reader.IsDBNull(11) ? null : reader.GetString(11)
                        });
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors du chargement de l'historique : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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


        private void BtnImprimerHistorique_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PrintDialog printDialog = new PrintDialog();
                if (printDialog.ShowDialog() == true)
                {
                    // Configuration de l'impression
                    printDialog.PrintVisual(historiqueDataGrid, "Impression de l'historique des modifications");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'impression : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    }
}









