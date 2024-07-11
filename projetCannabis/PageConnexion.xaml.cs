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
    /// Logique d'interaction pour PageConnexion.xaml
    /// </summary>
        public partial class PageConnexion : Page
        {
            public ObservableCollection<string> NomsUtilisateurs { get; set; } = new ObservableCollection<string>();

            public PageConnexion()
            {
                InitializeComponent();
                LoadUserNames();
            }

            private void LoadUserNames()
            {
                string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
                string query = "SELECT nom_utilisateur FROM Utilisateur";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        connection.Open();
                        SqlDataReader reader = command.ExecuteReader();

                        NomsUtilisateurs.Clear();
                        while (reader.Read())
                        {
                            NomsUtilisateurs.Add(reader.GetString(0));
                        }

                        cbNomUtilisateur.ItemsSource = NomsUtilisateurs;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void CbNomUtilisateur_TextChanged(object sender, TextChangedEventArgs e)
            {
                string searchTerm = cbNomUtilisateur.Text;

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    cbNomUtilisateur.IsDropDownOpen = false;
                    return;
                }

                var filteredList = NomsUtilisateurs.Where(nom => nom.StartsWith(searchTerm, StringComparison.InvariantCultureIgnoreCase)).ToList();
                cbNomUtilisateur.ItemsSource = filteredList;

                cbNomUtilisateur.IsDropDownOpen = filteredList.Count > 0;
            }

            private void CbNomUtilisateur_SelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (cbNomUtilisateur.SelectedItem != null)
                {
                    string selectedUser = cbNomUtilisateur.SelectedItem.ToString();

                    string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
                    string query = "SELECT mot_de_passe FROM Utilisateur WHERE nom_utilisateur = @NomUtilisateur";

                    try
                    {
                        using (SqlConnection connection = new SqlConnection(connectionString))
                        {
                            SqlCommand command = new SqlCommand(query, connection);
                            command.Parameters.AddWithValue("@NomUtilisateur", selectedUser);

                            connection.Open();
                            var result = command.ExecuteScalar();

                            tbMotDePasse.Password = result?.ToString() ?? string.Empty;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }

            private void BtnConnexion_Click(object sender, RoutedEventArgs e)
            {
                string nomUtilisateur = cbNomUtilisateur.Text;
                string motDePasse = tbMotDePasse.Password;

                if (string.IsNullOrWhiteSpace(nomUtilisateur) || string.IsNullOrWhiteSpace(motDePasse))
                {
                    MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
                string query = "SELECT COUNT(*) FROM Utilisateur WHERE nom_utilisateur = @NomUtilisateur AND mot_de_passe = @MotDePasse";

                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@NomUtilisateur", nomUtilisateur);
                        command.Parameters.AddWithValue("@MotDePasse", motDePasse);

                        connection.Open();
                        int userCount = (int)command.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Connexion réussie.", "Succès", MessageBoxButton.OK, MessageBoxImage.Information);
                        // Navigation vers la page d'accueil après une connexion réussie
                        NavigationService.Navigate(new PageAcceuil());
                    }
                        else
                        {
                            MessageBox.Show("Nom d'utilisateur ou mot de passe incorrect.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
            {
                cbNomUtilisateur.Text = string.Empty;
                tbMotDePasse.Password = string.Empty;
                cbNomUtilisateur.IsDropDownOpen = false;
            }
        }
    }