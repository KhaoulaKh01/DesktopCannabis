using Microsoft.Data.SqlClient;
using projetCannabis;
using System.Collections.ObjectModel;
using System.Text;
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
using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore;

namespace projetCannabis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GestionDonnee gestionDonnee;
       
        public MainWindow()
        {
            InitializeComponent();
            gestionDonnee = new GestionDonnee();

            // Par défaut, naviguez vers la page d'accueil
            MainFrame.Navigate(new PageConnexion());
            // Abonnement à l'événement Navigated du Frame
            MainFrame.Navigated += MainFrame_Navigated;
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (MainFrame.Content is PageConnexion)
            {
                // Désactiver le MenuItem "Options"
                foreach (MenuItem item in OptionsMenu.Items)
                {
                    item.IsEnabled = false;
                }

                // Désactiver le bouton "Accueil"
                btnAccueil.IsEnabled = false;
            }
            else
            {
                // Activer le MenuItem "Options"
                foreach (MenuItem item in OptionsMenu.Items)
                {
                    item.IsEnabled = true;
                }

                // Activer le bouton "Accueil"
                btnAccueil.IsEnabled = true;
            }
        }



        private void BtnAjoutPlantule(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PageAjoutPlantule());

        }
        private void BtnScanQR(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new PageScanQR());

        }

        private void BtnChercherPlantule(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new PageChercherPlantule());

        }

        private void BtnArchives(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new PageArchivePlantule());
        }



        private void BtnGestionUtilisateur(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new PageGestionUtilisateur());

        }
        private void BtnImporterDonnees(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new PageImporterDonnee());
        }

        private void BtnAccueil(object sender, RoutedEventArgs e)
        {
            // Naviguer vers la page d'accueil seulement si ce n'est pas la page de connexion
            if (!(MainFrame.Content is PageConnexion))
            {
                MainFrame.Navigate(new PageAcceuil());
            }
        }


        private void BtnDeconnecter(object sender, RoutedEventArgs e)
        {

            MainFrame.Navigate(new PageConnexion());
        }






        // la chaîne de connexion
        public static string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

        SqlConnection connection = new SqlConnection(ConnectionString);



        // Méthode pour insérer des données dans la table Plantule
        private void InsertDataIntoPlantule(string selectedPrefix, string lastIncrement)
        {
            string connectionString = "votre chaîne de connexion à la base de données";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Requête SQL d'insertion
                    string query = "INSERT INTO Plantule (Prefix, Identification) VALUES (@Prefix, @Identification)";

                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        // Ajoutez les paramètres avec leurs valeurs
                        cmd.Parameters.AddWithValue("@Prefix", selectedPrefix);
                        cmd.Parameters.AddWithValue("@Identification", lastIncrement);

                        // Exécutez la commande d'insertion
                        cmd.ExecuteNonQuery();
                    }

                    // Fermez la connexion après l'insertion
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erreur lors de l'insertion des données dans la table Plantule : " + ex.Message);
            }
        }

        // Autres méthodes et événements de la classe...
    


    private void BtnLoadPlantules_Click(object sender, RoutedEventArgs e)
            {
                var plantules = gestionDonnee.LoadPlantulesFromDatabase();
                // Faites quelque chose avec les plantules, par exemple, les lier à une liste dans l'interface utilisateur
            }

        /*private void BtnAddPlantule_Click(object sender, RoutedEventArgs e)
        {
            Plantule newPlantule = new Plantule
            {
                EtatDeSante = "Bon",
                Date = DateTime.Now,
                Identification = "ID123",
                Provenance = "Provenance A",
                Description = "Description de la plantule",
                Stade = "Stade 1",
                Entreposage = "Entreposage A",
                QteAjoutee = 10,
                QteRetiree = 0,
                ItemRetireInventaire = false,
                ResponsableDecontamination = "Responsable A",
                Note = "Aucune note"
            };

            if (gestionDonnee.AddPlantuleToDatabase(newPlantule))
            {
                MessageBox.Show("Plantule ajoutée avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Erreur lors de l'ajout de la plantule.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }*/


        public class CannabisContext : DbContext
        {
            public DbSet<Plantule> Plantules { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseSqlServer("Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;");
            }

            // Autres configurations ou méthodes de modèle si nécessaire
        }
    }
}
    


    

