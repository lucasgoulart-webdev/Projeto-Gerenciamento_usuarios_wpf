```csharp
using System;
using System.Text.RegularExpressions;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Internal logic for AdminCadastro.xaml
    /// </summary>
    public partial class AdminCadastro : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        public AdminCadastro()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string nome = TxtNome.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();

            string senha = TxtPassword.Password;
            string confirmarSenha = TxtConfirmPassword.Password;

            string avatar = "Imagens/adminAvatar.jpg";

            // Name is required
            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("Name is required.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Username is required
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Username must have at least 3 characters
            if (username.Length < 3)
            {
                MessageBox.Show("Username must contain at least 3 characters.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Email is required
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email is required.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Email format
            string padraoEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, padraoEmail))
            {
                MessageBox.Show("Enter a valid email address.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password is required
            if (string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Password is required.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password must have at least 8 characters
            if (senha.Length < 8)
            {
                MessageBox.Show("Password must contain at least 8 characters.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password confirmation is required
            if (string.IsNullOrWhiteSpace(confirmarSenha))
            {
                MessageBox.Show("Password confirmation is required.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Passwords must match
            if (senha != confirmarSenha)
            {
                MessageBox.Show("Password and confirmation must match.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Avatar is required
            if (string.IsNullOrWhiteSpace(avatar))
            {
                MessageBox.Show("A profile image must be selected.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    // Checks for duplicate username or email
                    string verificar = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE username = @username
                           OR email = @email";

                    using (MySqlCommand command = new MySqlCommand(verificar, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@email", email);

                        int quantidade = Convert.ToInt32(command.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show("The username or email is already registered.", "Registration", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Creates the password hash
                    string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

                    // Automatic administrator data
                    string tipoUsuario = "Administrador";
                    string perfilAcesso = "Administrador";
                    string status = "Ativo";

                    // Inserts the administrator
                    string sql = @"
                        INSERT INTO usuarios
                        (
                            nome_completo,
                            username,
                            email,
                            senha,
                            avatar,
                            tipo_usuario,
                            perfil_acesso,
                            status,
                            tentativas_login,
                            bloqueado_ate,
                            data_criacao,
                            data_alteracao
                        )
                        VALUES
                        (
                            @nome,
                            @username,
                            @email,
                            @senha,
                            @avatar,
                            @tipo_usuario,
                            @perfil_acesso,
                            @status,
                            0,
                            NULL,
                            UTC_TIMESTAMP(),
                            UTC_TIMESTAMP()
                        );

                        SELECT LAST_INSERT_ID();";

                    long idUsuarioCriado;

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@senha", senhaHash);
                        command.Parameters.AddWithValue("@avatar", avatar);
                        command.Parameters.AddWithValue("@tipo_usuario", tipoUsuario);
                        command.Parameters.AddWithValue("@perfil_acesso", perfilAcesso);
                        command.Parameters.AddWithValue("@status", status);

                        idUsuarioCriado = Convert.ToInt64(command.ExecuteScalar());
                    }

                    // ==============================
                    // AUDIT - ADMIN CREATION
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

                    using (MySqlCommand command = new MySqlCommand(inserirAuditoria, connection))
                    {
                        command.Parameters.AddWithValue("@usuario_responsavel_id", idUsuarioCriado);
                        command.Parameters.AddWithValue("@usuario_responsavel", username);
                        command.Parameters.AddWithValue("@operacao", "User creation");
                        command.Parameters.AddWithValue("@registro_afetado", "ID: " + idUsuarioCriado + " - User: " + username);
                        command.Parameters.AddWithValue("@valor_anterior", DBNull.Value);

                        // Password is never recorded in the audit log
                        string novoValor = "Name: " + nome +
                                           "; Username: " + username +
                                           "; Email: " + email +
                                           "; Type: " + tipoUsuario +
                                           "; Profile: " + perfilAcesso +
                                           "; Status: " + status +
                                           "; Avatar: " + avatar;

                        command.Parameters.AddWithValue("@novo_valor", novoValor);
                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show("Administrator registered successfully!", "Registration", MessageBoxButton.OK, MessageBoxImage.Information);

                    TelaLogin telaLogin = new TelaLogin();
                    telaLogin.Show();

                    this.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering administrator:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
```
