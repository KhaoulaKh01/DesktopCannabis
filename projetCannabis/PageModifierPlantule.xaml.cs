using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    /// Logique d'interaction pour PageModifierPlantule.xaml
    /// </summary>
    public partial class PageModifierPlantule : Page
    {
       
        private ObservableCollection<string> entreposagesList = new ObservableCollection<string>();
        private ObservableCollection<string> Entreposages;
        private ObservableCollection<string> Responsables;
        private GestionDonnee gestionDonnee;
        private string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";



        private ObservableCollection<string> _prefixes;
        public ObservableCollection<string> Identifications { get; set; } = new ObservableCollection<string>();

        private Plantule plantule;

        // public ObservableCollection<PlantuleModification> HistoriqueModifications { get; set; }

        public PageModifierPlantule(Plantule selectedPlantule)
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
            ChargerPrefixes();
            plantule = selectedPlantule;
            LoadPlantuleData();


        }
        public PageModifierPlantule()
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
            //LoadUniquePrefixesFromDatabase();
            ChargerPrefixes();
            //LoadData();
        }

        private void BtnRetour_Click(object sender, RoutedEventArgs e)
        {
            // Utilisez NavigationService pour revenir à la page précédente
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            // Recharger les données de la plantule depuis la base de données
            LoadPlantuleData();
        }

        private void BtnConfirmer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Utiliser la dernière incrémentation comme identifiant si nécessaire
                string identification = tbDerniereIncrementation.Text;

                // Récupérer les autres valeurs
                string provenance = textBoxProvenance.Text;
                string stade_vie = comboBoxStadeVie.Text;
                string etat_sante = tbEtatSante.Text;
                string entreposage = comboBoxEntreposage.Text;
                string description = textBoxDescription.Text;
                DateTime? date_reception = datePickerRecu.SelectedDate;
                DateTime? date_retrait = datePickerRetrait.SelectedDate;
                string item_retire_inventaire = textBoxItemRetireInventaire.Text;
                string responsable_decontamination = comboBoxResponsable.Text;
                string note = textBoxNote.Text;
                int actif_Inactif = date_retrait.HasValue ? 0 : 1;

                // Vérification des valeurs
                if (string.IsNullOrEmpty(identification))
                {
                    MessageBox.Show("Veuillez sélectionner une identification.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Afficher les valeurs pour débogage
                MessageBox.Show($"Identifiant: {identification}, Description: {description}, Date de réception: {date_reception}, Date de retrait: {date_retrait}, etc.");

                // Créer un objet Plantule avec les données mises à jour
                Plantule plantule = new Plantule
                {
                    Identification = identification,
                    Provenance = provenance,
                    StadeDeVie = stade_vie,
                    EtatSante = etat_sante,
                    Entreposage = entreposage,
                    Description = description,
                    DateReception = date_reception,
                    DateRetrait = date_retrait,
                    ItemRetireInventaire = item_retire_inventaire,
                    ResponsableDecontamination = responsable_decontamination,
                    Note = note,
                    Actif_Inactif = actif_Inactif
                };

                // Mettre à jour la plantule dans la base de données
                GestionDonnee gestionDonnee = new GestionDonnee();
                gestionDonnee.UpdatePlantule(plantule);

                // Afficher un message de succès
                MessageBox.Show("Plantule modifiée avec succès !");

                // Réinitialiser les champs du formulaire si nécessaire
                ResetFormFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Une erreur est survenue lors de la modification : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



   


        // Méthode pour réinitialiser les champs du formulaire après une mise à jour réussie
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
            comboBoxEntreposage.SelectedIndex=-1;
            comboBoxActifInactif.SelectedIndex = 0; // Actif par défaut
            textBoxDescription.Text = "";
            datePickerRecu.SelectedDate = null;
            datePickerRetrait.SelectedDate = null;
            textBoxItemRetireInventaire.Text = "";
            comboBoxResponsable.SelectedIndex = -1;
            textBoxNote.Text = "";
        }



        // Cette méthode est appelée pour charger les données d'une plantule sélectionnée.
        private void LoadPlantuleData()
        {
            if (plantule != null)
            {
                // Identification
                string prefix = ExtractPrefixFromIdentification(plantule.Identification);
                comboBoxIdentification.SelectedItem = prefix;


                tbDerniereIncrementation.Text = plantule.Identification ?? string.Empty;

                // Description
                textBoxDescription.Text = plantule.Description ?? string.Empty;

                // Date de réception
                datePickerRecu.SelectedDate = plantule.DateReception;

                // Date de retrait
                datePickerRetrait.SelectedDate = plantule.DateRetrait;

                // Item retiré inventaire
                textBoxItemRetireInventaire.Text = plantule.ItemRetireInventaire ?? string.Empty;

                // Responsable de décontamination
                var selectedResponsable = comboBoxResponsable.Items
                    .Cast<object>()
                    .FirstOrDefault(item => item.ToString() == plantule.ResponsableDecontamination);
                if (selectedResponsable != null)
                {
                    comboBoxResponsable.SelectedItem = selectedResponsable;
                }
                else
                {
                    // Ajouter le responsable si non existant
                    comboBoxResponsable.Items.Add(plantule.ResponsableDecontamination);
                    comboBoxResponsable.SelectedItem = plantule.ResponsableDecontamination;
                }

                // Actif/Inactif
                if (plantule.Actif_Inactif.HasValue)
                {
                    comboBoxActifInactif.SelectedItem = plantule.Actif_Inactif.Value == 1 ? "Actif" : "Inactif";
                }
                else
                {
                    comboBoxActifInactif.SelectedItem = null;
                }

                // Provenance
                textBoxProvenance.Text = plantule.Provenance ?? string.Empty;

                // Entreposage
                var selectedEntreposage = comboBoxEntreposage.Items
                    .Cast<object>()
                    .FirstOrDefault(item => item.ToString() == plantule.Entreposage);
                if (selectedEntreposage != null)
                {
                    comboBoxEntreposage.SelectedItem = selectedEntreposage;
                }
                /* else
                {
                    // Ajouter l'entreposage si non existant
                    comboBoxEntreposage.Items.Add(plantule.Entreposage);
                    comboBoxEntreposage.SelectedItem = plantule.Entreposage;
                } */

                // État de santé
                tbEtatSante.Text = plantule.EtatSante ?? string.Empty;
                SetCouleurFromEtatSante(plantule.EtatSante); // Appel de la fonction pour définir la couleur

                // Stade de vie
                var selectedStadeVie = comboBoxStadeVie.Items
                    .Cast<object>()
                    .FirstOrDefault(item => item.ToString() == plantule.StadeDeVie);
                if (selectedStadeVie != null)
                {
                    comboBoxStadeVie.SelectedItem = selectedStadeVie;
                }
                else
                {
                    // Ajouter le stade de vie si non existant
                    comboBoxStadeVie.Items.Add(plantule.StadeDeVie);
                    comboBoxStadeVie.SelectedItem = plantule.StadeDeVie;
                }

                // Note
                textBoxNote.Text = plantule.Note ?? string.Empty;
            }
            else
            {
                MessageBox.Show("La plantule sélectionnée est invalide.", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        // Fonction pour extraire le préfixe d'identification
        private string ExtractPrefixFromIdentification(string identification)
        {
            if (identification.Contains("."))
            {
                return identification.Substring(0, identification.IndexOf('.') + 1);
            }
            return identification; // Si pas de préfixe trouvé
        }

        // Méthode pour définir et appliquer la couleur en fonction de l'état de santé
        private void SetCouleurFromEtatSante(string etatSante)
        {
            ComboBoxItem itemToSelect = comboBoxCouleurs.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Tag.ToString() == etatSante);

            if (itemToSelect != null)
            {
                comboBoxCouleurs.SelectedItem = itemToSelect;
                ApplyColorToRectangle((SolidColorBrush)((Rectangle)itemToSelect.Content).Fill);
            }
            else
            {
                // État de santé non trouvé, couleur par défaut
                comboBoxCouleurs.SelectedIndex = -1;
                rectangleCouleur.Fill = new SolidColorBrush(Colors.Gray);
                tbEtatSante.Text = "";
            }
        }

        // Méthode pour appliquer la couleur au Rectangle et mettre à jour le texte
        private void ApplyColorToRectangle(SolidColorBrush brush)
        {
            rectangleCouleur.Fill = brush;
            //tbEtatSante.Text = brush.Color.ToString();
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
                // Accéder directement à l'élément en tant que chaîne
                string stadeVie = comboBoxStadeVie.SelectedItem.ToString();

                // Utilisez 'stadeVie' comme vous en avez besoin
                // Par exemple, affichez-le dans un TextBox ou utilisez-le dans une logique conditionnelle
                //Console.WriteLine(stadeVie);
            }
        }


        private void comboBoxEntreposage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBoxEntreposage.SelectedItem != null)
            {
                string entreposage = comboBoxEntreposage.SelectedItem.ToString();
                // Utilisez la variable 'entreposage' comme nécessaire
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


    }
}



