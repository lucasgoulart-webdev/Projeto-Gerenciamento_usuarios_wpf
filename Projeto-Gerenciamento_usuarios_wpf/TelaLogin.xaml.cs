```csharp
using System;
using System.Windows;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Internal logic for TelaLogin.xaml
    /// </summary>
    public partial class TelaLogin : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        public TelaLogin()
        {
            InitializeComponent();
            VerificarAdministrador();
        }

        // Checks if an administrator is already registered
        private void VerificarAdministrador()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE tipo_usuario = 'Administrador'";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        int quantidadeAdmins = Convert.ToInt32(command.ExecuteScalar());

                        // If there are no administrators
                        if (quantidadeAdmins == 0)
                        {
                            MessageBox.Show(
                                "No administrator has been registered yet.\n" +
                                "Register the first administrator to continue.",
                                "First Access",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            AdminCadastro adminCadastro = new AdminCadastro();
                            adminCadastro.Show();

                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking administrator:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // LOGIN BUTTON
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string senha = TxtPassword.Password;

            // Checks empty fields
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Invalid username or password.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT
                            id,
                            nome_completo,
                            username,
                            senha,
                            avatar,
                            tipo_usuario,
                            perfil_acesso,
                            status,
                            tentativas_login
                        FROM usuarios
                        WHERE username = @username
                        LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // User does not exist
                            if (!reader.Read())
                            {
                                reader.Close();

                                RegistrarEventoAutenticacao(connection, null, username, "Login attempt", "User not found");

                                MessageBox.Show("Invalid username or password.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            int id = Convert.ToInt32(reader["id"]);
                            string senhaHash = reader["senha"].ToString();
                            string status = reader["status"].ToString();
                            string tipoUsuario = reader["tipo_usuario"].ToString();
                            string perfilAcesso = reader["perfil_acesso"].ToString();
                            int tentativas = Convert.ToInt32(reader["tentativas_login"]);

                            // Checks if the user is active
                            if (status != "Ativo")
                            {
                                reader.Close();

                                RegistrarEventoAutenticacao(connection, id, username, "Login attempt", "User disabled");

                                MessageBox.Show("This user is currently disabled.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            // Compares the password with the HASH
                            bool senhaCorreta = BCrypt.Net.BCrypt.Verify(senha, senhaHash);

                            // INCORRECT PASSWORD
                            if (!senhaCorreta)
                            {
                                reader.Close();

                                tentativas++;

                                // 5 attempts = DISABLED
                                if (tentativas >= 5)
                                {
                                    string bloquear = @"
                                        UPDATE usuarios
                                        SET tentativas_login = @tentativas,
                                            status = 'Desativado'
                                        WHERE id = @id";

                                    using (MySqlCommand update = new MySqlCommand(bloquear, connection))
                                    {
                                        update.Parameters.AddWithValue("@tentativas", tentativas);
                                        update.Parameters.AddWithValue("@id", id);
                                        update.ExecuteNonQuery();
                                    }

                                    // ==============================
                                    // AUDIT - AUTOMATIC DEACTIVATION
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

                                    using (MySqlCommand auditoria = new MySqlCommand(inserirAuditoria, connection))
                                    {
                                        auditoria.Parameters.AddWithValue("@usuario_responsavel_id", id);
                                        auditoria.Parameters.AddWithValue("@usuario_responsavel", username);
                                        auditoria.Parameters.AddWithValue("@operacao", "Automatic deactivation");
                                        auditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + id + " - User: " + username);
                                        auditoria.Parameters.AddWithValue("@valor_anterior", "Status: Active");
                                        auditoria.Parameters.AddWithValue("@novo_valor", "Status: Disabled");

                                        auditoria.ExecuteNonQuery();
                                    }

                                    RegistrarEventoAutenticacao(connection, id, username, "Login attempt", "User disabled after 5 attempts");
                                }
                                else
                                {
                                    string atualizarTentativas = @"
                                        UPDATE usuarios
                                        SET tentativas_login = @tentativas
                                        WHERE id = @id";

                                    using (MySqlCommand update = new MySqlCommand(atualizarTentativas, connection))
                                    {
                                        update.Parameters.AddWithValue("@tentativas", tentativas);
                                        update.Parameters.AddWithValue("@id", id);
                                        update.ExecuteNonQuery();
                                    }

                                    RegistrarEventoAutenticacao(connection, id, username, "Login attempt", "Incorrect password");
                                }

                                MessageBox.Show("Invalid username or password.", "Login", MessageBoxButton.OK, MessageBoxImage.Warning);
                                return;
                            }

                            // SUCCESSFUL LOGIN
                            reader.Close();

                            string atualizarLogin = @"
                                UPDATE usuarios
                                SET tentativas_login = 0,
                                    status = 'Ativo',
                                    ultimo_login = UTC_TIMESTAMP()
                                WHERE id = @id";

                            using (MySqlCommand update = new MySqlCommand(atualizarLogin, connection))
                            {
                                update.Parameters.AddWithValue("@id", id);
                                update.ExecuteNonQuery();
                            }

                            // Records successful login
                            RegistrarEventoAutenticacao(connection, id, username, "Login", "Success");

                            // IDENTIFIES USER PERMISSIONS
                            if (tipoUsuario == "Admin" || perfilAcesso == "Administrador")
                            {
                                HubAdmin hubAdmin = new HubAdmin(id);
                                hubAdmin.Show();
                            }
                            else
                            {
                                MainWindow hubUsuario = new MainWindow(id);
                                hubUsuario.Show();
                            }

                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error while logging in:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // AUTHENTICATION EVENTS
        // ==============================

        private void RegistrarEventoAutenticacao(MySqlConnection connection, int? usuarioId, string usuario, string tipoEvento, string resultado)
        {
            string sql = @"
                INSERT INTO eventos_autenticacao
                (
                    usuario_id,
                    usuario,
                    tipo_evento,
                    resultado
                )
                VALUES
                (
                    @usuario_id,
                    @usuario,
                    @tipo_evento,
                    @resultado
                )";

            using (MySqlCommand command = new MySqlCommand(sql, connection))
            {
                if (usuarioId.HasValue)
                {
                    command.Parameters.AddWithValue("@usuario_id", usuarioId.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@usuario_id", DBNull.Value);
                }

                command.Parameters.AddWithValue("@usuario", usuario);
                command.Parameters.AddWithValue("@tipo_evento", tipoEvento);
                command.Parameters.AddWithValue("@resultado", resultado);

                command.ExecuteNonQuery();
            }
        }
    }
}
```
