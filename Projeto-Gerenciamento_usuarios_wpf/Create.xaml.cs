
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MySql.Data.MySqlClient;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Internal logic for Create.xaml
    /// </summary>
    public partial class Create : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        // Stores the selected avatar
        private string avatarSelecionado = "";
        private int AdminLogado;

        public Create(int idUsuario)
        {
            InitializeComponent();
            AdminLogado = idUsuario;
        }

        private void SelecionarAvatar(Button selecionado)
        {
            Avatar1.Opacity = 1;
            Avatar2.Opacity = 1;
            Avatar3.Opacity = 1;
            Avatar4.Opacity = 1;
            Avatar5.Opacity = 1;

            selecionado.Opacity = 0.5;
        }

        private void Avatar1_Click(object sender, RoutedEventArgs e)
        {
            SelecionarAvatar(Avatar1);
            avatarSelecionado = "Imagens/avatar1.jpg";
        }

        private void Avatar2_Click(object sender, RoutedEventArgs e)
        {
            SelecionarAvatar(Avatar2);
            avatarSelecionado = "Imagens/avatar2.jpg";
        }

        private void Avatar3_Click(object sender, RoutedEventArgs e)
        {
            SelecionarAvatar(Avatar3);
            avatarSelecionado = "Imagens/avatar3.jpg";
        }

        private void Avatar4_Click(object sender, RoutedEventArgs e)
        {
            SelecionarAvatar(Avatar4);
            avatarSelecionado = "Imagens/avatar4.jpg";
        }

        private void Avatar5_Click(object sender, RoutedEventArgs e)
        {
            SelecionarAvatar(Avatar5);
            avatarSelecionado = "Imagens/avatar5.jpg";
        }

        // ==============================
        // CREATE BUTTON
        // ==============================

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string nome = TxtNome.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();

            string senha = TxtPassword.Password;
            string confirmarSenha = TxtConfirmPassword.Password;

            // ==============================
            // VALIDATIONS
            // ==============================

            // Name is required
            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("Name is required.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Username is required
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("Username is required.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Username must have at least 3 characters
            if (username.Length < 3)
            {
                MessageBox.Show("Username must contain at least 3 characters.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Email is required
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Email is required.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Email format
            string padraoEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, padraoEmail))
            {
                MessageBox.Show("Enter a valid email address.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password is required
            if (string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Password is required.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password must have at least 8 characters
            if (senha.Length < 8)
            {
                MessageBox.Show("Password must contain at least 8 characters.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Password confirmation is required
            if (string.IsNullOrWhiteSpace(confirmarSenha))
            {
                MessageBox.Show("Password confirmation is required.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Passwords must match
            if (senha != confirmarSenha)
            {
                MessageBox.Show("Password and confirmation must match.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Avatar is required
            if (string.IsNullOrWhiteSpace(avatarSelecionado))
            {
                MessageBox.Show("Select a profile image.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Profile is required
            if (ComboPerfil.SelectedIndex == -1)
            {
                MessageBox.Show("Select an access level.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ==============================
            // DATABASE
            // ==============================

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
                            MessageBox.Show("The username or email is already registered.", "Create User", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // ==============================
                    // PASSWORD HASH
                    // ==============================

                    string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

                    // ==============================
                    // AUTOMATIC USER DATA
                    // ==============================

                    string tipoUsuario = "Usuário";
                    string perfilAcesso = "Usuário";

                    if (ComboPerfil.Text == "Administrador")
                    {
                        tipoUsuario = "Administrador";
                        perfilAcesso = "Administrador";
                    }

                    string status = "Ativo";

                    // ==============================
                    // INSERT USER
                    // ==============================

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

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@senha", senhaHash);
                        command.Parameters.AddWithValue("@avatar", avatarSelecionado);
                        command.Parameters.AddWithValue("@tipo_usuario", tipoUsuario);
                        command.Parameters.AddWithValue("@perfil_acesso", perfilAcesso);
                        command.Parameters.AddWithValue("@status", status);

                        long idNovoUsuario = Convert.ToInt64(command.ExecuteScalar());

                        // ==============================
                        // AUDIT
                        // ==============================

                        // Gets the username of the logged administrator
                        string adminUsername = "";

                        string buscarAdmin = @"
                            SELECT username
                            FROM usuarios
                            WHERE id = @idAdmin";

                        using (MySqlCommand commandAdmin = new MySqlCommand(buscarAdmin, connection))
                        {
                            commandAdmin.Parameters.AddWithValue("@idAdmin", AdminLogado);

                            object resultado = commandAdmin.ExecuteScalar();

                            if (resultado != null)
                            {
                                adminUsername = resultado.ToString();
                            }
                        }

                        // Records the user creation in the audit table
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
                            commandAuditoria.Parameters.AddWithValue("@usuario_responsavel_id", AdminLogado);
                            commandAuditoria.Parameters.AddWithValue("@usuario_responsavel", adminUsername);
                            commandAuditoria.Parameters.AddWithValue("@operacao", "User creation");
                            commandAuditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + idNovoUsuario + " - User: " + username);
                            commandAuditoria.Parameters.AddWithValue("@valor_anterior", DBNull.Value);

                            // Password is never recorded in the audit log
                            string novoValor = "Name: " + nome +
                                               "; Username: " + username +
                                               "; Email: " + email +
                                               "; Type: " + tipoUsuario +
                                               "; Profile: " + perfilAcesso +
                                               "; Status: " + status +
                                               "; Avatar: " + avatarSelecionado;

                            commandAuditoria.Parameters.AddWithValue("@novo_valor", novoValor);
                            commandAuditoria.ExecuteNonQuery();
                        }

                        MessageBox.Show("User created successfully!", "Create User", MessageBoxButton.OK, MessageBoxImage.Information);

                        HubAdmin novo = new HubAdmin(AdminLogado);
                        novo.Show();
                        this.Close();
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating user:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            HubAdmin novo = new HubAdmin(AdminLogado);
            novo.Show();
            this.Close();
        }
    }
}

