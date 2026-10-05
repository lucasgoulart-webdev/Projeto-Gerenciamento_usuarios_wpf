
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para Update.xaml
    /// </summary>
    public partial class Update : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        private int idlogin;
        private string avatarSelecionado = "";
        private int idUpdate;

        // Dados anteriores do usuário
        private string nomeAnterior = "";
        private string usernameAnterior = "";
        private string emailAnterior = "";
        private string statusAnterior = "";
        private string perfilAnterior = "";
        private string avatarAnterior = "";

        public Update(int idLogado)
        {
            InitializeComponent();
            idlogin = idLogado;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(avatarSelecionado))
            {
                MessageBox.Show("Select a profile image.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string nameUpdate = name.Text.Trim();
            string userUpdate = user.Text.Trim();
            string emailUpdate = email.Text.Trim();
            string statusUpdate = cmbStatus.Text.Trim();
            string profileUpdate = cmbProfile.Text.Trim();

            try
            {
                using (MySqlConnection coon = new MySqlConnection(conexao))
                {
                    coon.Open();

                    string sql = @"
                        UPDATE usuarios
                        SET
                            nome_completo = @nome,
                            username = @username,
                            email = @email,
                            avatar = @avatar,
                            tipo_usuario = @tipo_usuario,
                            perfil_acesso = @perfil_acesso,
                            status = @status,
                            data_alteracao = UTC_TIMESTAMP()
                        WHERE id = @id";

                    using (MySqlCommand command = new MySqlCommand(sql, coon))
                    {
                        command.Parameters.AddWithValue("@nome", nameUpdate);
                        command.Parameters.AddWithValue("@username", userUpdate);
                        command.Parameters.AddWithValue("@email", emailUpdate);
                        command.Parameters.AddWithValue("@avatar", avatarSelecionado);
                        command.Parameters.AddWithValue("@tipo_usuario", profileUpdate);
                        command.Parameters.AddWithValue("@perfil_acesso", profileUpdate);
                        command.Parameters.AddWithValue("@status", statusUpdate);
                        command.Parameters.AddWithValue("@id", idUpdate);

                        int linhasAlteradas = command.ExecuteNonQuery();

                        if (linhasAlteradas > 0)
                        {
                            // ==============================
                            // BUSCA O ADMIN RESPONSÁVEL
                            // ==============================

                            string adminUsername = "";

                            string buscarAdmin = @"
                                SELECT username
                                FROM usuarios
                                WHERE id = @idAdmin";

                            using (MySqlCommand commandAdmin = new MySqlCommand(buscarAdmin, coon))
                            {
                                commandAdmin.Parameters.AddWithValue("@idAdmin", idlogin);

                                object resultado = commandAdmin.ExecuteScalar();

                                if (resultado != null)
                                {
                                    adminUsername = resultado.ToString();
                                }
                            }

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

                            // Alteração de nome
                            if (nomeAnterior != nameUpdate)
                            {
                                RegistrarAuditoria(coon, inserirAuditoria, adminUsername, "User update", "Name: " + nomeAnterior, "Name: " + nameUpdate, userUpdate);
                            }

                            // Alteração de username
                            if (usernameAnterior != userUpdate)
                            {
                                RegistrarAuditoria(coon, inserirAuditoria, adminUsername, "User update", "Username: " + usernameAnterior, "Username: " + userUpdate, userUpdate);
                            }

                            // Alteração de e-mail
                            if (emailAnterior != emailUpdate)
                            {
                                RegistrarAuditoria(coon, inserirAuditoria, adminUsername, "User update", "Email: " + emailAnterior, "Email: " + emailUpdate, userUpdate);
                            }

                            // Alteração de perfil/permissões
                            if (perfilAnterior != profileUpdate)
                            {
                                RegistrarAuditoria(coon, inserirAuditoria, adminUsername, "Profile update", "Profile: " + perfilAnterior, "Profile: " + profileUpdate, userUpdate);
                            }

                            // Ativação ou desativação
                            if (statusAnterior != statusUpdate)
                            {
                                string operacaoStatus = statusUpdate == "Ativo" ? "User activation" : "User deactivation";

                                RegistrarAuditoria(coon, inserirAuditoria, adminUsername, operacaoStatus, "Status: " + statusAnterior, "Status: " + statusUpdate, userUpdate);
                            }

                            // Alteração da imagem de perfil
                            if (avatarAnterior != avatarSelecionado)
                            {
                                RegistrarAuditoria(coon, inserirAuditoria, adminUsername, "Profile image update", "Avatar: " + avatarAnterior, "Avatar: " + avatarSelecionado, userUpdate);
                            }

                            MessageBox.Show("User updated successfully!", "Update", MessageBoxButton.OK, MessageBoxImage.Information);

                            HubAdmin novo = new HubAdmin(idlogin);
                            novo.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("Unable to update the user.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Error updating user:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // REGISTRAR AUDITORIA
        // ==============================

        private void RegistrarAuditoria(MySqlConnection connection, string sql, string adminUsername, string operacao, string valorAnterior, string novoValor, string usuarioAfetado)
        {
            using (MySqlCommand commandAuditoria = new MySqlCommand(sql, connection))
            {
                commandAuditoria.Parameters.AddWithValue("@usuario_responsavel_id", idlogin);
                commandAuditoria.Parameters.AddWithValue("@usuario_responsavel", adminUsername);
                commandAuditoria.Parameters.AddWithValue("@operacao", operacao);
                commandAuditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + idUpdate + " - User: " + usuarioAfetado);
                commandAuditoria.Parameters.AddWithValue("@valor_anterior", valorAnterior);
                commandAuditoria.Parameters.AddWithValue("@novo_valor", novoValor);

                commandAuditoria.ExecuteNonQuery();
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            carregarUser();
        }

        private void carregarUser()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(conexao))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            id,
                            nome_completo,
                            username,
                            email,
                            status,
                            perfil_acesso,
                            avatar
                        FROM usuarios 
                        WHERE email = @email";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", emailSearch.Text);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                MessageBox.Show("Email not found in the database!", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);

                                emailSearch.Clear();
                                return;
                            }

                            int idUsuario = Convert.ToInt32(reader["id"]);

                            if (idlogin == idUsuario)
                            {
                                MessageBox.Show("You cannot update your own account.", "Update", MessageBoxButton.OK, MessageBoxImage.Warning);

                                emailSearch.Clear();
                                return;
                            }
                            else
                            {
                                idUpdate = Convert.ToInt32(reader["id"]);

                                name.Text = reader["nome_completo"].ToString();
                                user.Text = reader["username"].ToString();
                                email.Text = reader["email"].ToString();
                                cmbStatus.Text = reader["status"].ToString();
                                cmbProfile.Text = reader["perfil_acesso"].ToString();

                                // Guarda os valores anteriores para a auditoria
                                nomeAnterior = reader["nome_completo"].ToString();
                                usernameAnterior = reader["username"].ToString();
                                emailAnterior = reader["email"].ToString();
                                statusAnterior = reader["status"].ToString();
                                perfilAnterior = reader["perfil_acesso"].ToString();
                                avatarAnterior = reader["avatar"].ToString();

                                // Mantém o avatar atual caso o administrador não queira alterá-lo
                                avatarSelecionado = avatarAnterior;
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
                MessageBox.Show("Error loading user:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void emailSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (emailSearch.Text == "")
            {
                name.Clear();
                user.Clear();
                email.Clear();

                idUpdate = 0;
                avatarSelecionado = "";

                nomeAnterior = "";
                usernameAnterior = "";
                emailAnterior = "";
                statusAnterior = "";
                perfilAnterior = "";
                avatarAnterior = "";
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            HubAdmin abrir = new HubAdmin(idlogin);
            abrir.Show();
            this.Close();
        }
    }
}

