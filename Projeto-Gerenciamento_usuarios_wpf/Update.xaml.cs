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
                MessageBox.Show(
                    "Selecione uma imagem de perfil.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }
            string nameUpdate = name.Text.Trim();
            string userUpdate = user.Text.Trim();
            string emailUpdate = email.Text.Trim();
            string statusUpdate = cmbStatus.Text.Trim();
            string profileUpdate = cmbProfile.Text.Trim();

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

                using (MySqlCommand command =
                    new MySqlCommand(sql, coon))
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
                        MessageBox.Show(
                            "Usuário atualizado com sucesso!",
                            "Update",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);

                        HubAdmin novo = new HubAdmin(idlogin);
                        novo.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show(
                            "Não foi possível atualizar o usuário.",
                            "Update",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            carregarUser();
        }

        private void carregarUser()
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
                        perfil_acesso 
                    FROM usuarios 
                    WHERE email = @email";

                using (MySqlCommand cmd =
                    new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@email",
                        emailSearch.Text);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show(
                                "Email não encontrado no banco!",
                                "Update",
                                MessageBoxButton.OK);

                            emailSearch.Clear();
                            return;
                        }

                        int idUuario =
                            Convert.ToInt32(reader["id"]);

                        if (idlogin == idUuario)
                        {
                            MessageBox.Show(
                                "Você não pode alterar o seu próprio cadastro.",
                                "Update",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);

                            emailSearch.Clear();
                            return;
                        }
                        else
                        {
                            idUpdate =
                                Convert.ToInt32(reader["id"]);

                            name.Text =
                                reader["nome_completo"].ToString();

                            user.Text =
                                reader["username"].ToString();

                            email.Text =
                                reader["email"].ToString();

                            cmbStatus.Text =
                                reader["status"].ToString();

                            cmbProfile.Text =
                                reader["perfil_acesso"].ToString();
                        }
                    }
                }
            }
        }

        private void emailSearch_TextChanged(
            object sender,
            TextChangedEventArgs e)
        {
            if (emailSearch.Text == "")
            {
                name.Clear();
                user.Clear();
                email.Clear();
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