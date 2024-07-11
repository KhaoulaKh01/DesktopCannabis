using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;


namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageAcceuil.xaml
    /// </summary>
    public partial class PageAcceuil : Page, INotifyPropertyChanged
    {
        // Interface pour la notification des changements de propriété
        public event PropertyChangedEventHandler PropertyChanged;


        private int _activePlantuleCount;
        public int ActivePlantuleCount
        {
            get { return _activePlantuleCount; }
            set
            {
                if (_activePlantuleCount != value)
                {
                    _activePlantuleCount = value;
                    OnPropertyChanged(nameof(ActivePlantuleCount));
                    
                }
            }
        }

        private int _stockValue;
        public int StockValue
        {
            get { return _stockValue; }
            set
            {
                if (_stockValue != value)
                {
                    _stockValue = value;
                    OnPropertyChanged(nameof(StockValue));
                }
            }
        }
      


        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void UpdateCapaciteRestanteFromDatabase()
        {
            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
            string updateQuery = "UPDATE Stock SET CapaciteRestante = stockValeur - (SELECT COUNT(*) FROM Plantule WHERE actif_Inactif = 1)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(updateQuery, connection))
                {
                    try
                    {
                        connection.Open();
                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            // Mise à jour réussie, maintenant récupérer la capacité restante depuis la base de données
                            string selectCapaciteQuery = "SELECT CapaciteRestante FROM Stock";
                            using (SqlCommand selectCapaciteCommand = new SqlCommand(selectCapaciteQuery, connection))
                            {
                                object result = selectCapaciteCommand.ExecuteScalar();
                                if (result != null && result != DBNull.Value)
                                {
                                    int capaciteRestante = Convert.ToInt32(result);
                                    CapaciteRestanteTextBlock.Text = capaciteRestante.ToString();
                                }
                                else
                                {
                                    MessageBox.Show("Aucune capacité restante trouvée dans la base de données.");
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Aucune ligne mise à jour dans la base de données.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors de la mise à jour de la capacité restante : " + ex.Message);
                    }
                }
            }
        }



        private void BtnAppliquerStock_Click(object sender, RoutedEventArgs e)
        {
            int newValue;
            if (int.TryParse(txtStock.Text, out newValue))
            {
                // Mettre à jour la base de données avec la nouvelle valeur du stock
                string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
                string updateQuery = "UPDATE Stock SET stockValeur = @NewStockValue"; // Requête d'update

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@NewStockValue", newValue);

                        try
                        {
                            connection.Open();
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // Mise à jour réussie, maintenant récupérer la valeur mise à jour
                                string selectQuery = "SELECT stockValeur FROM Stock"; // Requête SELECT
                                using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                                {
                                    // Exécuter la commande SELECT
                                    int updatedValue = (int)selectCommand.ExecuteScalar();

                                    // Mettre à jour l'interface avec la valeur récupérée
                                    StockValue = updatedValue;
                                    txtStock.Text = updatedValue.ToString(); // Mettre à jour le champ txtStock

                                    MessageBox.Show("La valeur du stock a été mise à jour avec succès dans la base de données.");
                                    UpdateCapaciteRestanteFromDatabase();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Aucune ligne mise à jour dans la base de données.");
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Erreur lors de la mise à jour du stock : " + ex.Message);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez entrer un nombre valide.");
            }
        }
        private void LoadStockValueFromDatabase()
        {
            // Connexion à la base de données et requête SELECT pour récupérer la valeur de stockValeur
            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
            string selectQuery = "SELECT stockValeur FROM Stock";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(selectQuery, connection))
                {
                    try
                    {
                        connection.Open();
                        object result = command.ExecuteScalar();
                        if (result != null)
                        {
                            int initialValue = Convert.ToInt32(result);
                            StockValue = initialValue; // Mettre à jour la valeur interne
                            txtStock.Text = initialValue.ToString(); // Mettre à jour l'interface utilisateur
                        }
                        else
                        {
                            MessageBox.Show("Aucune valeur trouvée dans la base de données.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Erreur lors du chargement de la valeur du stock : " + ex.Message);
                    }
                }
            }
        }




        public PageAcceuil()
        {
            InitializeComponent();
            DataContext = this;
            UpdateActivePlantuleCount();
            SetCurrentDate();
            LoadStockValueFromDatabase(); // Appel à la méthode de chargement au démarrage
            UpdateCapaciteRestanteFromDatabase();


        }



        private void UpdateActivePlantuleCount()
        {
            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
            int activeCount = 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Plantule WHERE actif_Inactif = 1";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    activeCount = (int)command.ExecuteScalar();
                }
            }

            // Mettre à jour le TextBlock avec le nombre de plantules actives
            activePlantuleCountTextBlock.Text = activeCount.ToString();
            UpdateCapaciteRestanteFromDatabase();
        }

        private void SetCurrentDate()
        {
            CurrentDateTextBlock.Text = DateTime.Now.ToString("dd/MM/yyyy");
        }


        private void BtnAjoutPlantule(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageAjoutPlantule());

        }
        private void BtnScanQR(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new PageScanQR());

        }

        private void BtnChercherPlantule(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new PageChercherPlantule());

        }

        private void BtnGererPlantule(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new PageGererPlantule());

        }

        private void BtnImporterDonnees(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new PageImporterDonnee());

        }


        private void BtnArchives(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageArchivePlantule());
        }



        private void BtnGestionUtilisateur(object sender, RoutedEventArgs e)
        {

            NavigationService.Navigate(new PageGestionUtilisateur());

        }
    }
}

