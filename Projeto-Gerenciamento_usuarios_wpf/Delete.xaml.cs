
using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Internal logic for Delete.xaml
    /// </summary>
    public partial class Delete : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        // Logged user ID
        private int usuarioLogadoId;

        public Delete(int idUsuarioLogado)
        {
            InitializeComponent();
            usuarioLogadoId = idUsuarioLogado;
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            string email = TxtEmail.Text.Trim();

            // Email is required
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Enter the email of the user you want to delete.", "Delete User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    // Searches for the user by email
                    string buscar = @"
                        SELECT
                            id,
                            nome_completo,
                            username,
                            email,
                            avatar,
                            tipo_usuario,
                            perfil_acesso,
                            status
                        FROM usuarios
                        WHERE email = @email
                        LIMIT 1";

                    int idUsuario;
                    string nomeUsuario;
                    string username;
                    string emailUsuario;
                    string avatar;
                    string tipoUsuario;
                    string perfilAcesso;
                    string status;

                    using (MySqlCommand command = new MySqlCommand(buscar, connection))
                    {
                        command.Parameters.AddWithValue("@email", email);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("No user was found with this email.", "Delete User", MessageBoxButton.OK, MessageBoxImage.Warning);
                                TxtEmail.Clear();
                                return;
                            }

                            idUsuario = Convert.ToInt32(reader["id"]);
                            nomeUsuario = reader["nome_completo"].ToString();
                            username = reader["username"].ToString();
                            emailUsuario = reader["email"].ToString();
                            avatar = reader["avatar"].ToString();
                            tipoUsuario = reader["tipo_usuario"].ToString();
                            perfilAcesso = reader["perfil_acesso"].ToString();
                            status = reader["status"].ToString();
                        }
                    }

                    // Prevents the administrator from deleting their own account
                    if (idUsuario == usuarioLogadoId)
                    {
                        MessageBox.Show("You cannot delete your own account while logged in.", "Deletion Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Checks if the user being deleted is an administrator
                    if (tipoUsuario == "Administrador")
                    {
                        string contarAdmins = @"
                            SELECT COUNT(*)
                            FROM usuarios
                            WHERE tipo_usuario = 'Administrador'";

                        int quantidadeAdmins;

                        using (MySqlCommand command = new MySqlCommand(contarAdmins, connection))
                        {
                            quantidadeAdmins = Convert.ToInt32(command.ExecuteScalar());
                        }

                        // Prevents deletion of the last administrator
                        if (quantidadeAdmins <= 1)
                        {
                            MessageBox.Show("The last administrator in the system cannot be deleted.", "Deletion Not Allowed", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Mandatory confirmation before deletion
                    MessageBoxResult confirmacao = MessageBox.Show(
                        "Are you sure you want to delete this user?\n\n" +
                        nomeUsuario +
                        "\n" +
                        email,
                        "Confirm Deletion",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);

                    if (confirmacao != MessageBoxResult.Yes)
                    {
                        TxtEmail.Clear();
                        return;
                    }

                    // Gets the username of the responsible administrator
                    string adminUsername = "";

                    string buscarAdmin = @"
                        SELECT username
                        FROM usuarios
                        WHERE id = @idAdmin";

                    using (MySqlCommand commandAdmin = new MySqlCommand(buscarAdmin, connection))
                    {
                        commandAdmin.Parameters.AddWithValue("@idAdmin", usuarioLogadoId);

                        object resultado = commandAdmin.ExecuteScalar();

                        if (resultado != null)
                        {
                            adminUsername = resultado.ToString();
                        }
                    }

                    // Deletes the user
                    string excluir = @"
                        DELETE FROM usuarios
                        WHERE id = @id";

                    using (MySqlCommand command = new MySqlCommand(excluir, connection))
                    {
                        command.Parameters.AddWithValue("@id", idUsuario);
                        command.ExecuteNonQuery();
                    }

                    // ==============================
                    // AUDIT
                    // ==============================

                    string inserirAuditoria = @"
                        INSERT INTO auditoria
                        (
                            usuario_responsavel_id,
                            usuario_responsavel,
                            operacao,
                            registro_afetado,
                            valor_anterior,
                            novo_valor
                        )
                        VALUES
                        (
                            @usuario_responsavel_id,
                            @usuario_responsavel,
                            @operacao,
                            @registro_afetado,
                            @valor_anterior,
                            @novo_valor
                        )";

                    using (MySqlCommand commandAuditoria = new MySqlCommand(inserirAuditoria, connection))
                    {
                        commandAuditoria.Parameters.AddWithValue("@usuario_responsavel_id", usuarioLogadoId);
                        commandAuditoria.Parameters.AddWithValue("@usuario_responsavel", adminUsername);
                        commandAuditoria.Parameters.AddWithValue("@operacao", "User deletion");
                        commandAuditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + idUsuario + " - User: " + username);

                        string valorAnterior = "Name: " + nomeUsuario +
                                               "; Username: " + username +
                                               "; Email: " + emailUsuario +
                                               "; Type: " + tipoUsuario +
                                               "; Profile: " + perfilAcesso +
                                               "; Status: " + status +
                                               "; Avatar: " + avatar;

                        commandAuditoria.Parameters.AddWithValue("@valor_anterior", valorAnterior);
                        commandAuditoria.Parameters.AddWithValue("@novo_valor", DBNull.Value);

                        commandAuditoria.ExecuteNonQuery();
                    }

                    MessageBox.Show("User deleted successfully!", "Delete User", MessageBoxButton.OK, MessageBoxImage.Information);

                    TxtEmail.Clear();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting user:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HubAdmin abrir = new HubAdmin(usuarioLogadoId);
            abrir.Show();
            this.Close();
        }
    }
}

