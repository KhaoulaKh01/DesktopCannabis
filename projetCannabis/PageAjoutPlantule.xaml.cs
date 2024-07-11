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
using static projetCannabis.MainWindow;

namespace projetCannabis
{
    /// <summary>
    /// Logique d'interaction pour PageAjoutPlantule.xaml
    /// </summary>
    public partial class PageAjoutPlantule : Page
    {
        private ObservableCollection<string> entreposagesList = new ObservableCollection<string>();
        private ObservableCollection<string> Entreposages;
        private ObservableCollection<string> Responsables;
        private GestionDonnee gestionDonnee;
        private string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

        private ObservableCollection<string> _prefixes;
        public ObservableCollection<string> Identifications { get; set; } = new ObservableCollection<string>();





        public PageAjoutPlantule()
        {
            InitializeComponent();
            Entreposages = new ObservableCollection<string>();
            Responsables = new ObservableCollection<string>();
            comboBoxEntreposage.ItemsSource = Entreposages;
            comboBoxResponsable.ItemsSource = Responsables;
            LoadEntreposagesFromDatabase();
            LoadResponsablesFromDatabase();
            gestionDonnee = new GestionDonnee();

            _prefixes = new ObservableCollection<string>();
            comboBoxIdentification.ItemsSource = _prefixes;

           
            // Charge les préfixes uniques dans le ComboBox d'identification
            LoadUniquePrefixesFromDatabase();
            ChargerPrefixes();

        }




        private void BtnRetourAccueil(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new PageAcceuil());
        }

        private void BtnCreer_Click(object sender, RoutedEventArgs e)
        {
            // Charger les entreposages existants depuis la base de données
            LoadEntreposagesFromDatabase();

            // Générer l'identification
            string prefix = comboBoxIdentification.SelectedItem?.ToString();
            if (prefix == null)
            {
                MessageBox.Show("Veuillez sélectionner un préfixe.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string nouvelIdentifiant = GenerateNextIdentification(prefix);
            if (nouvelIdentifiant == null)
            {
                MessageBox.Show("Erreur lors de la génération de l'identification.");
                return;
            }

            // Collecter les informations de la plantule
            string provenance = textBoxProvenance.Text;
            string stade_vie = comboBoxStadeVie.Text;
            string etat_sante = tbEtatSante.Text;
            string entreposage = tbEntreposage.Text;
            string description = textBoxDescription.Text;
            DateTime? date_reception = datePickerRecu.SelectedDate;
            DateTime? date_retrait = datePickerRetrait.SelectedDate;
            string item_retire_inventaire = textBoxItemRetireInventaire.Text;
            string responsable_decontamination = comboBoxResponsable.Text;
            string note = textBoxNote.Text;
            int actif_Inactif = date_retrait.HasValue ? 0 : 1;

            // Créer un objet Plantule
            Plantule nouvellePlantule = new Plantule
            {
                Identification = nouvelIdentifiant,
                Description = description,
                DateReception = date_reception,
                DateRetrait = date_retrait,
                Provenance = provenance,
                EtatSante = etat_sante,
                StadeDeVie = stade_vie,
                Actif_Inactif = actif_Inactif,
                Entreposage = entreposage,
                ItemRetireInventaire = item_retire_inventaire,
                ResponsableDecontamination = responsable_decontamination,
                Note = note
            };

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                try
                {
                    connection.Open();

                    // Vérifier si l'entreposage existe déjà
                    string checkEntreposageQuery = "SELECT COUNT(*) FROM Entreposage WHERE designation = @entreposage";
                    SqlCommand checkEntreposageCommand = new SqlCommand(checkEntreposageQuery, connection);
                    checkEntreposageCommand.Parameters.AddWithValue("@entreposage", entreposage);

                    int count = (int)checkEntreposageCommand.ExecuteScalar();
                    if (count == 0)
                    {
                        // Insérer le nouvel entreposage s'il n'existe pas
                        string insertEntreposageQuery = "INSERT INTO Entreposage (designation) VALUES (@entreposage)";
                        SqlCommand insertEntreposageCommand = new SqlCommand(insertEntreposageQuery, connection);
                        insertEntreposageCommand.Parameters.AddWithValue("@entreposage", entreposage);
                        insertEntreposageCommand.ExecuteNonQuery();

                        // Ajouter le nouvel entreposage à la liste
                        if (!entreposagesList.Contains(entreposage))
                        {
                            entreposagesList.Add(entreposage);
                        }
                    }

                    // Insérer la nouvelle plantule dans la base de données
                    string sqlInsert = @"INSERT INTO Plantule (identification, description, date_reception, date_retrait, provenance, 
                etat_sante, stade_vie, actif_Inactif, entreposage, item_retire_inventaire, 
                responsable_decontamination, note)
                VALUES (@identification, @description, @date_reception, @date_retrait, @provenance,
                @etat_sante, @stade_vie, @actif_Inactif, @entreposage, @item_retire_inventaire,
                @responsable_decontamination, @note)";

                    SqlCommand command = new SqlCommand(sqlInsert, connection);
                    command.Parameters.AddWithValue("@identification", nouvelIdentifiant);
                    command.Parameters.AddWithValue("@provenance", provenance);
                    command.Parameters.AddWithValue("@stade_vie", stade_vie);
                    command.Parameters.AddWithValue("@actif_Inactif", actif_Inactif);
                    command.Parameters.AddWithValue("@etat_sante", etat_sante);
                    command.Parameters.AddWithValue("@entreposage", entreposage);
                    command.Parameters.AddWithValue("@responsable_decontamination", responsable_decontamination);
                    command.Parameters.AddWithValue("@description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                    command.Parameters.AddWithValue("@date_reception", date_reception.HasValue ? (object)date_reception.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@date_retrait", date_retrait.HasValue ? (object)date_retrait.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@item_retire_inventaire", string.IsNullOrEmpty(item_retire_inventaire) ? (object)DBNull.Value : item_retire_inventaire);
                    command.Parameters.AddWithValue("@note", string.IsNullOrEmpty(note) ? (object)DBNull.Value : note);

                    int rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Nouvelle plantule ajoutée avec succès !");
                        ResetFormFields();
                        // Ajouter l'historique pour la nouvelle plantule
                        gestionDonnee.AjouterHistoriqueModification(nouvellePlantule, $"Création de la nouvelle plantule {nouvelIdentifiant}");


                        // Recharger les entreposages
                        LoadEntreposagesFromDatabase();
                    }
                    else
                    {
                        MessageBox.Show("Erreur lors de l'ajout de la plantule.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors de l'insertion : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }





        private void ResetFormFields()
        {
            // Réinitialiser tous les champs du formulaire après l'ajout réussi
            comboBoxIdentification.SelectedIndex = -1;
            rectangleIdentification.Fill = Brushes.Transparent;
            rectangleCouleur.Fill = Brushes.Transparent;
            tbEtatSante.Text = "";
            tbDerniereIncrementation.Text = "";
            textBoxProvenance.Text = "";
            comboBoxStadeVie.SelectedIndex = -1;
            comboBoxCouleurs.SelectedIndex = -1;
            comboBoxActifInactif.SelectedIndex = 0; // Actif par défaut
            textBoxDescription.Text = "";
            datePickerRecu.SelectedDate = null;
            datePickerRetrait.SelectedDate = null;
            textBoxItemRetireInventaire.Text = "";
            comboBoxResponsable.SelectedIndex = -1;
            textBoxNote.Text = "";
        }





        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Réinitialisation des champs

            // Identification
            comboBoxIdentification.SelectedIndex = -1;
            rectangleIdentification.Fill = Brushes.Transparent;

            tbDerniereIncrementation.Text = "";

            // Description
            textBoxDescription.Text = "";

            // Dates
            datePickerRecu.SelectedDate = null;
            datePickerRetrait.SelectedDate = null;

            // Responsable
            comboBoxResponsable.SelectedIndex = -1;

            // Actif/Inactif
            comboBoxActifInactif.SelectedIndex = -1;
            rectangleActifInactif.Fill = Brushes.Transparent;

            // Provenance
            textBoxProvenance.Text = "";

            // Entreposage
            comboBoxEntreposage.SelectedIndex = -1;

            // État de santé (ComboBox avec couleurs)
            comboBoxCouleurs.SelectedIndex = -1;
            rectangleCouleur.Fill = Brushes.Transparent;

            // Stade de vie
            comboBoxStadeVie.SelectedIndex = -1;

            // Notes
            textBoxNote.Text = "";

            // Message de confirmation
            MessageBox.Show("Données réinitialisées.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void IdentificationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxIdentification.SelectedItem != null)
            {
                string selectedPrefix = comboBoxIdentification.SelectedItem.ToString();
                string newIdentification = GenerateNewIdentification(selectedPrefix);
                tbDerniereIncrementation.Text = newIdentification;
            }
        }


        public List<string> GetExistingIdentifications(string prefix)
        {
            List<string> existingIdentifications = new List<string>();

            // Votre code pour récupérer les identifications depuis la base de données
            string query = "SELECT identification FROM Plantule WHERE identification LIKE @Prefix";
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Prefix", prefix + "%");
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    existingIdentifications.Add(reader.GetString(0));
                }
            }

            return existingIdentifications;
        }


        private void ChargerPrefixes()
        {
            List<string> prefixes = new List<string>();

            // Remplacez par votre méthode pour obtenir la connexion à la base de données
            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT DISTINCT SUBSTRING(identification, 0, CHARINDEX('.', identification) + 1) AS Prefix FROM Plantule";
                    SqlCommand cmd = new SqlCommand(query, conn);

                    SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        prefixes.Add(reader.GetString(0));
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors du chargement des préfixes : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            comboBoxIdentification.ItemsSource = prefixes;
        }

        private string GenerateNextIdentification(string prefix)
        {
            if (string.IsNullOrEmpty(prefix))
            {
                MessageBox.Show("Veuillez sélectionner un préfixe.");
                return null;
            }

            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

            int newIncrement = 1;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT MAX(CAST(SUBSTRING(identification, LEN(@Prefix) + 1, LEN(identification) - LEN(@Prefix)) AS INT)) FROM Plantule WHERE identification LIKE @Prefix + '%'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Prefix", prefix);

                    var result = cmd.ExecuteScalar();
                    if (result != DBNull.Value && result != null)
                    {
                        newIncrement = (int)result + 1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Erreur lors de la génération de l'identification : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            return $"{prefix}{newIncrement}";
        }



        public string GenerateNewIdentification(string prefix)
        {
            List<string> existingIdentifications = GetExistingIdentifications(prefix);

            // Extraire les numéros d'incrémentation
            List<int> numbers = existingIdentifications
                .Where(id => id.StartsWith(prefix))
                .Select(id =>
                {
                    string numberPart = id.Substring(prefix.Length).TrimStart('.');
                    if (int.TryParse(numberPart, out int num))
                    {
                        return num;
                    }
                    return 0;
                })
                .ToList();

            // Trouver le numéro le plus élevé et incrémenter
            int newIncrement = numbers.Any() ? numbers.Max() + 1 : 1;

            // Générer la nouvelle identification
            string newIdentification = $"{prefix}{newIncrement}";

            return newIdentification;
        }



       
        private void comboBoxCouleurs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxCouleurs.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboBoxCouleurs.SelectedItem;
                Rectangle rect = (Rectangle)selectedItem.Content;
                rectangleCouleur.Fill = rect.Fill;

                // Mettre à jour le tbEtatSante en fonction de la couleur
                SolidColorBrush fillBrush = rect.Fill as SolidColorBrush;
                if (fillBrush != null)
                {
                    if (fillBrush.Color == Colors.Green)
                    {
                        tbEtatSante.Text = "Bonne santé";
                    }
                    else if (fillBrush.Color == Colors.Yellow)
                    {
                        tbEtatSante.Text = "Moyenne santé";
                    }
                    else if (fillBrush.Color == Colors.Orange)
                    {
                        tbEtatSante.Text = "Mauvaise santé";
                    }
                    else if (fillBrush.Color == Colors.Red)
                    {
                        tbEtatSante.Text = "Santé en danger";
                    }
                }
            }
        }

        private void comboBoxResponsable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxResponsable.SelectedItem != null)
            {
                string responsable = comboBoxResponsable.SelectedItem.ToString(); // Récupère directement la string du Content

            }
        }



        private void comboBoxActifInactif_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxActifInactif.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboBoxActifInactif.SelectedItem;
                string actif_Inactif = selectedItem.Content.ToString();

                if (rectangleActifInactif != null)
                {
                    if (actif_Inactif.Contains("Actif : 1"))
                    {
                        rectangleActifInactif.Fill = Brushes.Green;
                    }
                    else
                    {
                        rectangleActifInactif.Fill = Brushes.Red;
                    }
                }
            }
        }

        private void comboBoxStadeVie_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxStadeVie.SelectedItem != null)
            {
                ComboBoxItem selectedItem = (ComboBoxItem)comboBoxStadeVie.SelectedItem;
                string stadeVie = selectedItem.Content.ToString();


            }
        }

        private void comboBoxEntreposage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxEntreposage.SelectedItem != null)
            {
                string entreposage = comboBoxEntreposage.SelectedItem.ToString();
                // Utilisez la variable 'entreposage' comme nécessaire
                tbEntreposage.Text = entreposage; // Mettre à jour le TextBox avec l'entreposage sélectionné
            }
        }




        private void LoadEntreposagesFromDatabase()
        {
            entreposagesList.Clear(); // Effacez la collection pour la recharger

            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Charger les entreposages de la table Plantule
                string queryPlantule = "SELECT DISTINCT entreposage FROM Plantule WHERE entreposage IS NOT NULL"; // Ajouter une condition WHERE pour exclure les valeurs NULL
                using (SqlCommand command = new SqlCommand(queryPlantule, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Vérifier si la valeur n'est pas null avant de l'ajouter
                        if (!reader.IsDBNull(0))
                        {
                            string entreposage = reader.GetString(0);
                            if (!string.IsNullOrEmpty(entreposage) && !entreposagesList.Contains(entreposage))
                            {
                                entreposagesList.Add(entreposage);
                            }
                        }
                    }
                    reader.Close();
                }

                // Charger les entreposages de la table Entreposage
                string queryEntreposage = "SELECT designation FROM Entreposage WHERE designation IS NOT NULL"; // Ajouter une condition WHERE pour exclure les valeurs NULL
                using (SqlCommand command = new SqlCommand(queryEntreposage, connection))
                {
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        // Vérifier si la valeur n'est pas null avant de l'ajouter
                        if (!reader.IsDBNull(0))
                        {
                            string entreposage = reader.GetString(0);
                            if (!string.IsNullOrEmpty(entreposage) && !entreposagesList.Contains(entreposage))
                            {
                                entreposagesList.Add(entreposage);
                            }
                        }
                    }
                    reader.Close();
                }
            }

            // Mettre à jour ItemsSource du ComboBox
            comboBoxEntreposage.ItemsSource = entreposagesList;
        }


        private void LoadResponsablesFromDatabase()
        {
            Responsables.Clear();

            string query = "SELECT DISTINCT responsable_decontamination FROM Plantule";
            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            string responsable = reader.GetString(0);
                            Responsables.Add(responsable);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des responsables de décontamination: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void BtnAjouterEntreposage_Click(object sender, RoutedEventArgs e)
        {
            string nouveauEntreposage = comboBoxEntreposage.Text.Trim();

            if (!string.IsNullOrEmpty(nouveauEntreposage) && !entreposagesList.Contains(nouveauEntreposage))
            {
                // Ajouter le nouvel entreposage à la liste
                entreposagesList.Add(nouveauEntreposage);
                comboBoxEntreposage.SelectedItem = nouveauEntreposage; // Sélectionner le nouvel entreposage

                // Mettre à jour le ComboBox
                comboBoxEntreposage.ItemsSource = null; // Définir à null pour permettre la mise à jour
                comboBoxEntreposage.ItemsSource = entreposagesList;

                // Insérer le nouvel entreposage dans la base de données
                string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Entreposage (designation, description_entreposage) VALUES (@designation, @description)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@designation", nouveauEntreposage);
                        command.Parameters.AddWithValue("@description", "Description par défaut");

                        try
                        {
                            command.ExecuteNonQuery();
                            MessageBox.Show("Nouvel entreposage ajouté avec succès !");
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Erreur lors de l'ajout de l'entreposage : {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez entrer un entreposage valide et non dupliqué.");
            }
        }





       
        // Méthode pour générer le prochain identifiant


        private void BtnAjouterIdentification_Click(object sender, RoutedEventArgs e)
        {
            // Récupérer l'identification complète saisie par l'utilisateur
            string newIdentification = comboBoxIdentification.Text.Trim();

            // Vérifier si l'identification est vide
            if (string.IsNullOrEmpty(newIdentification))
            {
                MessageBox.Show("Veuillez saisir une nouvelle identification.");
                return;
            }

            // Vérifier si l'identification existe déjà dans la liste
            if (_prefixes.Contains(newIdentification))
            {
                MessageBox.Show("Cette identification existe déjà dans la liste.");
                return;
            }

            // Ajouter la nouvelle identification à la liste des identifications existantes
            _prefixes.Add(newIdentification);

            // Ajouter le nouveau préfixe à la liste des préfixes si nécessaire
            string prefix = newIdentification.Split('.')[0] + ".";
            if (!_prefixes.Contains(prefix))
            {
                _prefixes.Add(prefix);
            }

            // Mettre à jour le ComboBox avec les nouveaux choix possibles
            comboBoxIdentification.ItemsSource = null; // Clear the ItemsSource
            comboBoxIdentification.ItemsSource = _prefixes; // Rebind to updated prefixes
            comboBoxIdentification.SelectedItem = newIdentification;

            // Informer l'utilisateur que la nouvelle identification a été ajoutée avec succès
            MessageBox.Show("Nouvelle identification ajoutée avec succès.");
        }



  

        private void LoadUniquePrefixesFromDatabase()
        {
            // Vous devrez adapter la chaîne de connexion à votre base de données
            string connectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

            // Liste pour stocker les préfixes
            List<string> uniquePrefixes = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Requête SQL pour sélectionner les préfixes sans doublons
                string sqlQuery = "SELECT DISTINCT Prefix FROM Plantule";

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Ajout des préfixes dans la liste
                            uniquePrefixes.Add(reader["Prefix"].ToString());
                        }
                    }
                }
            }

            // Mettre à jour l'ObservableCollection _prefixes avec les préfixes chargés
            _prefixes.Clear();
            foreach (string prefix in uniquePrefixes)
            {
                _prefixes.Add(prefix);
            }
        }

        
    }
}