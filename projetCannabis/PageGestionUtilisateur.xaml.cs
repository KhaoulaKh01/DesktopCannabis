using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageGestionUtilisateur.xaml
    /// </summary>
    public partial class PageGestionUtilisateur : Page
    {
        public ObservableCollection<Utilisateur> Utilisateurs { get; set; } = new ObservableCollection<Utilisateur>();

        public PageGestionUtilisateur()
        {
            InitializeComponent();
            dataGridUtilisateurs.ItemsSource = Utilisateurs;
            ChargerUtilisateurs();
        }

        private void ChargerUtilisateurs()
        {
            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
            string query = "SELECT id_user, nom, prenom, nom_utilisateur, mot_de_passe FROM Utilisateur";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    Utilisateurs.Add(new Utilisateur
                    {
                        IdUser = reader.GetInt32(0),  // Assurez-vous que l'ID est bien lu
                        Nom = reader.GetString(1),
                        Prenom = reader.GetString(2),
                        NomUtilisateur = reader.GetString(3),
                        MotDePasse = reader.GetString(4)
                    });
                }
            }
        }

        private void BtnAjouterUtilisateur(object sender, RoutedEventArgs e)
        {
            string nom = textBoxNom.Text;
            string prenom = textBoxPrenom.Text;
            string nomUtilisateur = textBoxNomUtilisateur.Text;
            string motDePasse = textBoxMotDePasse.Text;

            if (string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(prenom) ||
                string.IsNullOrWhiteSpace(nomUtilisateur) || string.IsNullOrWhiteSpace(motDePasse))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
            string query = "INSERT INTO Utilisateur (nom, prenom, nom_utilisateur, mot_de_passe) OUTPUT INSERTED.id_user VALUES (@Nom, @Prenom, @NomUtilisateur, @MotDePasse)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Nom", nom);
                command.Parameters.AddWithValue("@Prenom", prenom);
                command.Parameters.AddWithValue("@NomUtilisateur", nomUtilisateur);
                command.Parameters.AddWithValue("@MotDePasse", motDePasse);

                connection.Open();
                int newUserId = (int)command.ExecuteScalar();

                Utilisateurs.Add(new Utilisateur
                {
                    IdUser = newUserId,
                    Nom = nom,
                    Prenom = prenom,
                    NomUtilisateur = nomUtilisateur,
                    MotDePasse = motDePasse
                });
            }

            MessageBox.Show("Utilisateur ajouté avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            EffacerChamps();
        }


        // Gestionnaire pour annuler l'action (effacer les champs)
        private void BtnAnnulerUtilisateur(object sender, RoutedEventArgs e)
        {
            EffacerChamps();
        }

        // Gestionnaire pour supprimer un utilisateur
        private void BtnSupprimerUtilisateur(object sender, RoutedEventArgs e)
        {
            if (dataGridUtilisateurs.SelectedItem is Utilisateur utilisateurSelectionne)
            {
                string  ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
                string query = "DELETE FROM Utilisateur WHERE id_user = @IdUser";

                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@IdUser", utilisateurSelectionne.IdUser);

                    connection.Open();
                    command.ExecuteNonQuery();
                }

                Utilisateurs.Remove(utilisateurSelectionne);
                MessageBox.Show("Utilisateur supprimé avec succès.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Méthode pour effacer les champs
        private void EffacerChamps()
        {
            textBoxNom.Text = string.Empty;
            textBoxPrenom.Text = string.Empty;
            textBoxNomUtilisateur.Text = string.Empty;
            textBoxMotDePasse.Text = string.Empty;
        }
    }

    public class Utilisateur
    {
        public int IdUser { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public string NomUtilisateur { get; set; }
        public string MotDePasse { get; set; }

    }
}
