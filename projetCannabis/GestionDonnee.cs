using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.EntityFrameworkCore; 
using static System.Runtime.InteropServices.JavaScript.JSType;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ListView;


namespace projetCannabis
{
    public class GestionDonnee
    {
        private static string ConnectionString = "Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;";

        public ObservableCollection<Plantule> LoadPlantulesFromDatabase()
        {
            ObservableCollection<Plantule> plantules = new ObservableCollection<Plantule>();


            string query = "SELECT [etat_sante], [date_reception] , [id_qr] , [date_etat_sante] , [id_entreposage] , [date_retrait] ,[identification], [provenance], [description], [stade_vie] ,[statut], [entreposage], [qte_ajoutee], [qte_retiree], [item_retire_inventaire], [responsable_decontamination], [note] FROM Plantule"; // Modifiez le nom de la table selon vos besoins

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
                            EtatSante = reader.IsDBNull(0) ? null : reader.GetString(0),
                            DateReception = reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1),
                            Identification = reader.GetString(2),
                            Provenance = reader.IsDBNull(3) ? null : reader.GetString(3),
                            Description = reader.IsDBNull(4) ? null : reader.GetString(4),
                            StadeDeVie = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Entreposage = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Actif_Inactif = reader.IsDBNull(7) ? (int?)null : reader.GetInt32(7),
                            DateRetrait = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                            QteAjoutee = reader.IsDBNull(9) ? (int?)null : reader.GetInt32(9),
                            QteRetiree = reader.IsDBNull(10) ? (int?)null : reader.GetInt32(10),
                            ItemRetireInventaire = reader.IsDBNull(11) ? null : reader.GetString(11),
                            ResponsableDecontamination = reader.IsDBNull(12) ? null : reader.GetString(12),
                            Note = reader.IsDBNull(13) ? null : reader.GetString(13)
                        };
                        plantules.Add(plantule);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors du chargement des plantules: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return plantules;
        }




        // Méthode pour ajouter une plantule à la base de données
        public bool AddPlantuleToDatabase(Plantule plantule)
        {
            string query = "INSERT INTO Plantule (etat_sante, date_etat_sante, id_qr, date_reception, identification, provenance, description, stade_vie, date_retrait, entreposage, id_entreposage, statut, qte_ajoutee, qte_retiree, item_retire_inventaire, responsable_decontamination, note) " +
                           "VALUES (@EtatDeSante, @DateEtatSante, @IdQR, @DateReception, @Identification, @Provenance, @Description, @StadeDeVie, @DateRetrait, @Entreposage, @IdEntreposage, @Statut, @QteAjoutee, @QteRetiree, @ItemRetireInventaire, @ResponsableDecontamination, @Note)";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EtatDeSante", (object)plantule.EtatSante ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateEtatSante", (object)plantule.DateEtatSante ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IdQR", (object)plantule.IdQR ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateReception", (object)plantule.DateReception ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Identification", plantule.Identification);
                    command.Parameters.AddWithValue("@Provenance", (object)plantule.Provenance ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)plantule.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StadeDeVie", (object)plantule.StadeDeVie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateRetrait", (object)plantule.DateRetrait ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Entreposage", (object)plantule.Entreposage ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IdEntreposage", (object)plantule.IdEntreposage ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Statut", (object)plantule.Actif_Inactif ?? DBNull.Value);
                    command.Parameters.AddWithValue("@QteAjoutee", (object)plantule.QteAjoutee ?? DBNull.Value);
                    command.Parameters.AddWithValue("@QteRetiree", (object)plantule.QteRetiree ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ItemRetireInventaire", (object)plantule.ItemRetireInventaire ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ResponsableDecontamination", (object)plantule.ResponsableDecontamination ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Note", (object)plantule.Note ?? DBNull.Value);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de l'ajout de la plantule: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool UpdatePlantuleInDatabase(Plantule plantule)
        {
            string query = "UPDATE Plantule SET etat_sante = @EtatDeSante, date_etat_sante = @DateEtatSante, id_qr = @IdQR, " +
                           "date_reception = @DateReception, provenance = @Provenance, description = @Description, " +
                           "stade_vie = @StadeDeVie, date_retrait = @DateRetrait, entreposage = @Entreposage, " +
                           "id_entreposage = @IdEntreposage, statut = @Statut, qte_ajoutee = @QteAjoutee, " +
                           "qte_retiree = @QteRetiree, item_retire_inventaire = @ItemRetireInventaire, " +
                           "responsable_decontamination = @ResponsableDecontamination, note = @Note " +
                           "WHERE identification = @Identification";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@EtatDeSante", (object)plantule.EtatSante ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateEtatSante", (object)plantule.DateEtatSante ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IdQR", (object)plantule.IdQR ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateReception", (object)plantule.DateReception ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Provenance", (object)plantule.Provenance ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Description", (object)plantule.Description ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StadeDeVie", (object)plantule.StadeDeVie ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DateRetrait", (object)plantule.DateRetrait ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Entreposage", (object)plantule.Entreposage ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IdEntreposage", (object)plantule.IdEntreposage ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Statut", (object)plantule.Actif_Inactif ?? DBNull.Value);
                    command.Parameters.AddWithValue("@QteAjoutee", (object)plantule.QteAjoutee ?? DBNull.Value);
                    command.Parameters.AddWithValue("@QteRetiree", (object)plantule.QteRetiree ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ItemRetireInventaire", (object)plantule.ItemRetireInventaire ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ResponsableDecontamination", (object)plantule.ResponsableDecontamination ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Note", (object)plantule.Note ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Identification", plantule.Identification);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la mise à jour de la plantule: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public bool DeletePlantuleFromDatabase(string identification)
        {
            string query = "DELETE FROM Plantule WHERE identification = @Identification";

            try
            {
                using (SqlConnection connection = new SqlConnection(ConnectionString))
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", identification);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erreur lors de la suppression de la plantule: {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }



        public void AjouterHistoriqueModification(Plantule plantule, string description)
        {
            using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;"))
            {
                try
                {
                    connection.Open();
                    string query = @"INSERT INTO HistoriqueModifications (Identification, Description, DateModification, Provenance, EtatSante, StadeVie, ActifInactif, Entreposage, ItemRetireInventaire, ResponsableDecontamination, Note)
                             VALUES (@Identification, @Description, @DateModification, @Provenance, @EtatSante, @StadeVie, @ActifInactif, @Entreposage, @ItemRetireInventaire, @ResponsableDecontamination, @Note)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Identification", plantule.Identification);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@DateModification", DateTime.Now);
                    command.Parameters.AddWithValue("@Provenance", plantule.Provenance ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@EtatSante", plantule.EtatSante ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@StadeVie", plantule.StadeDeVie ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ActifInactif", plantule.Actif_Inactif);
                    command.Parameters.AddWithValue("@Entreposage", plantule.Entreposage ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ItemRetireInventaire", plantule.ItemRetireInventaire ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@ResponsableDecontamination", plantule.ResponsableDecontamination ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Note", plantule.Note ?? (object)DBNull.Value);

                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors de l'ajout de l'historique : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public void UpdatePlantule(Plantule plantule)
        {
            using (SqlConnection connection = new SqlConnection("Data Source=DESKTOP-1AQ9LC7\\SQLEXPRESS;Initial Catalog=CannabisDesktop;Integrated Security=True;Encrypt=True;Trust Server Certificate=True;"))
            {
                try
                {
                    connection.Open();
                    string query = @"UPDATE Plantule 
                             SET description = @description,
                                 date_reception = @date_reception,
                                 date_retrait = @date_retrait,
                                 provenance = @provenance,
                                 etat_sante = @etat_sante,
                                 stade_vie = @stade_vie,
                                 actif_Inactif = @actif_Inactif,
                                 entreposage = @entreposage,
                                 item_retire_inventaire = @item_retire_inventaire,
                                 responsable_decontamination = @responsable_decontamination,
                                 note = @note
                             WHERE identification = @identification";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@identification", plantule.Identification);
                    command.Parameters.AddWithValue("@provenance", plantule.Provenance);
                    command.Parameters.AddWithValue("@stade_vie", plantule.StadeDeVie);
                    command.Parameters.AddWithValue("@actif_Inactif", plantule.Actif_Inactif);
                    command.Parameters.AddWithValue("@etat_sante", plantule.EtatSante);
                    command.Parameters.AddWithValue("@entreposage", plantule.Entreposage);
                    command.Parameters.AddWithValue("@responsable_decontamination", plantule.ResponsableDecontamination);
                    command.Parameters.AddWithValue("@description", string.IsNullOrEmpty(plantule.Description) ? (object)DBNull.Value : plantule.Description);
                    command.Parameters.AddWithValue("@date_reception", plantule.DateReception.HasValue ? (object)plantule.DateReception.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@date_retrait", plantule.DateRetrait.HasValue ? (object)plantule.DateRetrait.Value : DBNull.Value);
                    command.Parameters.AddWithValue("@item_retire_inventaire", string.IsNullOrEmpty(plantule.ItemRetireInventaire) ? (object)DBNull.Value : plantule.ItemRetireInventaire);
                    command.Parameters.AddWithValue("@note", string.IsNullOrEmpty(plantule.Note) ? (object)DBNull.Value : plantule.Note);

                    command.ExecuteNonQuery();

                    // Ajouter une entrée d'historique
                    AjouterHistoriqueModification(plantule, $"Modification des détails de la plantule {plantule.Identification}");

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Une erreur est survenue lors de la mise à jour de la plantule : {ex.Message}", "Erreur", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }




    }

}



