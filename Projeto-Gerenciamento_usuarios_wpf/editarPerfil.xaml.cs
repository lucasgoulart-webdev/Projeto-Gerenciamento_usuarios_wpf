```csharp
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para editarPerfil.xaml
    /// </summary>
    public partial class editarPerfil : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        public int idLog;
        public string avatarSelecionado;

        // Valores anteriores para auditoria
        private string nomeAnterior = "";
        private string usernameAnterior = "";
        private string emailAnterior = "";
        private string avatarAnterior = "";

        public editarPerfil(int idLogado)
        {
            InitializeComponent();
            idLog = idLogado;
            carregarUser();
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

        void carregarUser()
        {
            try
            {
                using (MySqlConnection coon = new MySqlConnection(conexao))
                {
                    coon.Open();

                    string query = "SELECT id, nome_completo, username, email, avatar FROM usuarios WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, coon))
                    {
                        cmd.Parameters.AddWithValue("@id", idLog);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                name.Text = reader["nome_completo"].ToString();
                                username.Text = reader["username"].ToString();
                                email.Text = reader["email"].ToString();
                                avatarSelecionado = reader["avatar"].ToString();

                                // Guarda os valores originais
                                nomeAnterior = reader["nome_completo"].ToString();
                                usernameAnterior = reader["username"].ToString();
                                emailAnterior = reader["email"].ToString();
                                avatarAnterior = reader["avatar"].ToString();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading profile:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(name.Text) ||
                string.IsNullOrWhiteSpace(username.Text) ||
                string.IsNullOrWhiteSpace(email.Text) ||
                string.IsNullOrWhiteSpace(avatarSelecionado))
            {
                MessageBox.Show("All fields must be filled in!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    // Verifica username e e-mail duplicados
                    string verificar = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE id != @id
                        AND (username = @username OR email = @email)";

                    using (MySqlCommand command = new MySqlCommand(verificar, connection))
                    {
                        command.Parameters.AddWithValue("@id", idLog);
                        command.Parameters.AddWithValue("@username", username.Text);
                        command.Parameters.AddWithValue("@email", email.Text);

                        int quantidade = Convert.ToInt32(command.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show("The username or email is already registered.", "Profile Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // Atualiza o perfil
                    string query = @"
                        UPDATE usuarios
                        SET
                            nome_completo = @nome_completo,
                            username = @username,
                            email = @email,
                            avatar = @avatar,
                            data_alteracao = UTC_TIMESTAMP()
                        WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@id", idLog);
                        cmd.Parameters.AddWithValue("@nome_completo", name.Text.Trim());
                        cmd.Parameters.AddWithValue("@username", username.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", email.Text.Trim());
                        cmd.Parameters.AddWithValue("@avatar", avatarSelecionado);

                        int linhasAlteradas = cmd.ExecuteNonQuery();

                        if (linhasAlteradas > 0)
                        {
                            string novoNome = name.Text.Trim();
                            string novoUsername = username.Text.Trim();
                            string novoEmail = email.Text.Trim();

                            // ==============================
                            // AUDITORIA
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

                            // Nome alterado
                            if (nomeAnterior != novoNome)
                            {
                                RegistrarAuditoria(connection, inserirAuditoria, "Profile update", "Name: " + nomeAnterior, "Name: " + novoNome, novoUsername);
                            }

                            // Username alterado
                            if (usernameAnterior != novoUsername)
                            {
                                RegistrarAuditoria(connection, inserirAuditoria, "Profile update", "Username: " + usernameAnterior, "Username: " + novoUsername, novoUsername);
                            }

                            // E-mail alterado
                            if (emailAnterior != novoEmail)
                            {
                                RegistrarAuditoria(connection, inserirAuditoria, "Profile update", "Email: " + emailAnterior, "Email: " + novoEmail, novoUsername);
                            }

                            // Avatar alterado
                            if (avatarAnterior != avatarSelecionado)
                            {
                                RegistrarAuditoria(connection, inserirAuditoria, "Profile image update", "Avatar: " + avatarAnterior, "Avatar: " + avatarSelecionado, novoUsername);
                            }

                            MessageBox.Show("Profile updated successfully!", "Profile Update", MessageBoxButton.OK, MessageBoxImage.Information);

                            MainWindow abrir = new MainWindow(idLog);
                            abrir.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Unable to update the profile.", "Profile Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Database error:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating profile:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // REGISTRAR AUDITORIA
        // ==============================

        private void RegistrarAuditoria(MySqlConnection connection, string sql, string operacao, string valorAnterior, string novoValor, string usuarioAfetado)
        {
            using (MySqlCommand commandAuditoria = new MySqlCommand(sql, connection))
            {
                commandAuditoria.Parameters.AddWithValue("@usuario_responsavel_id", idLog);
                commandAuditoria.Parameters.AddWithValue("@usuario_responsavel", usuarioAfetado);
                commandAuditoria.Parameters.AddWithValue("@operacao", operacao);
                commandAuditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + idLog + " - User: " + usuarioAfetado);
                commandAuditoria.Parameters.AddWithValue("@valor_anterior", valorAnterior);
                commandAuditoria.Parameters.AddWithValue("@novo_valor", novoValor);

                commandAuditoria.ExecuteNonQuery();
            }
        }
    }
}
```
