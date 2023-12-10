using System;
using System.Data.SqlClient;
namespace GestionClient
{
   

    class Program
    {
        static void Main()
        {
            string ConnectionString = @"Data Source=localhost\SQLEXPRESS;";
            ConnectionString += @"Initial Catalog=GestionClient;Integrated Security=SSPI";
            GetAllClients(ConnectionString);

            // Ajouter un nouveau client
            AddNewClient(ConnectionString,"33535", "NouveauNom", "NouveauPrenom", "NouvelleVille", "NouveauTelephone");
            FilterClients(ConnectionString, "VilleA");
            UpdateTablesWithTransaction(ConnectionString, 123456789, "NouvelleVille");
        }
        static void GetAllClients(string connectionString)
        {
            SqlConnection connection = null;
            try
            {
                connection = new SqlConnection();
                connection.ConnectionString = connectionString;
                connection.Open();
                string query = "SELECT * FROM Client";
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Console.WriteLine($"CIN: {reader["CIN"]}, Nom: {reader["Nom"]}, Prénom: {reader["Prenom"]}, Ville: {reader["Ville"]}, Téléphone: {reader["Telephone"]}");
                }
                reader.Close();
                connection.Close();
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Erreur SQL : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
            finally
            {
                // Assurez-vous de fermer la connexion même en cas d'exception

                connection.Close();
            }
        }
        static void AddNewClient(string connectionString,string cin,string nom, string prenom, string ville, string telephone)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                transaction = connection.BeginTransaction();

                string insertQuery = "INSERT INTO Client (CIN,Nom, Prenom, Ville, Telephone) VALUES (@CIN,@Nom, @Prenom, @Ville, @Telephone)";

                SqlCommand command = new SqlCommand(insertQuery, connection, transaction);
                command.Parameters.AddWithValue("@CIN", cin);
                command.Parameters.AddWithValue("@Nom", nom);
                command.Parameters.AddWithValue("@Prenom", prenom);
                command.Parameters.AddWithValue("@Ville", ville);
                command.Parameters.AddWithValue("@Telephone", telephone);

                command.ExecuteNonQuery();
                transaction.Commit();

                Console.WriteLine("Nouveau client ajouté avec succès.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Erreur SQL : {ex.Message}");
                transaction?.Rollback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                transaction?.Rollback();
            }
            finally
            {
               
                    connection.Close();
                
            }
        }

        static void FilterClients(string connectionString, string filterVille)
        {
            SqlConnection connection = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                string query = "SELECT * FROM Client WHERE Ville = @FilterVille";

                SqlCommand command = new SqlCommand(query, connection);

                // Ajouter un paramètre à la commande
                command.Parameters.AddWithValue("@FilterVille", filterVille);

                using (SqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Console.WriteLine($"CIN: {reader["CIN"]}, Nom: {reader["Nom"]}, Prénom: {reader["Prenom"]}, Ville: {reader["Ville"]}, Téléphone: {reader["Telephone"]}");
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Erreur SQL : {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
            }
            finally
            {
                
                    connection.Close();
               
            }
        }
        static void UpdateTablesWithTransaction(string connectionString, int cinToUpdate, string nouvelleVille)
        {
            SqlConnection connection = null;
            SqlTransaction transaction = null;

            try
            {
                connection = new SqlConnection(connectionString);
                connection.Open();

                // Démarre une transaction
                transaction = connection.BeginTransaction();

                // Met à jour la table Client
                UpdateClientTable(connection, transaction, cinToUpdate, nouvelleVille);

                // Met à jour la table Commande
                UpdateCommandTable(connection, transaction, cinToUpdate);

                // Commit la transaction si tout s'est bien passé
                transaction.Commit();

                Console.WriteLine("Mise à jour des tables réussie.");
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Erreur SQL : {ex.Message}");

                // Rollback la transaction en cas d'erreur
                transaction?.Rollback();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");

                // Rollback la transaction en cas d'erreur
                transaction?.Rollback();
            }
            finally
            {
                
                    connection.Close();
                
            }
        }

        static void UpdateClientTable(SqlConnection connection, SqlTransaction transaction, int cinToUpdate, string nouvelleVille)
        {
            string updateClientQuery = "UPDATE Client SET Ville = @NouvelleVille WHERE CIN = @CIN";

            using (SqlCommand command = new SqlCommand(updateClientQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@NouvelleVille", nouvelleVille);
                command.Parameters.AddWithValue("@CIN", cinToUpdate);

                command.ExecuteNonQuery();
            }
        }
        static void UpdateCommandTable(SqlConnection connection, SqlTransaction transaction, int cinToUpdate)
        {
            string updateCommandQuery = "UPDATE Commande SET Datecmd = GETDATE() WHERE CIN = @CIN";

            using (SqlCommand command = new SqlCommand(updateCommandQuery, connection, transaction))
            {
                command.Parameters.AddWithValue("@CIN", cinToUpdate);

                command.ExecuteNonQuery();
            }
        }
    }
}

