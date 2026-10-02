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
    /// Lógica interna para editarPerfil.xaml
    /// </summary>
    public partial class editarPerfil : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        public int idLog;
        public string avatarSelecionado;

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
            using (MySqlConnection coon = new MySqlConnection(conexao))
            {
                coon.Open();

                string query = "SELECT id, nome_completo, username, email, avatar FROM usuarios WHERE id=@id";

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
                        }
                        
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            using (MySqlConnection connection =
                new MySqlConnection(conexao))
            {
                connection.Open();

                // Verifica username e e-mail duplicados
                string verificar = @"
                    SELECT COUNT(*)
                    FROM usuarios
                    WHERE id != @id
                    AND (username = @username OR email = @email)";

                using (MySqlCommand command =
                    new MySqlCommand(verificar, connection))
                {
                    command.Parameters.AddWithValue("@id", idLog);

                    command.Parameters.AddWithValue(
                        "@username", username.Text);

                    command.Parameters.AddWithValue(
                        "@email", email.Text);

                    int quantidade =
                        Convert.ToInt32(command.ExecuteScalar());

                    if (quantidade > 0)
                    {
                        MessageBox.Show(
                            "O nome de usuário ou e-mail já está cadastrado.",
                            "Cadastro",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }
                }
            }

            using (MySqlConnection coon = new MySqlConnection(conexao))
            {
                coon.Open();

                string query = @"UPDATE usuarios SET nome_completo = @nome_completo, username = @username, email = @email, avatar = @avatar WHERE id = @id"; 

                using (MySqlCommand cmd = new MySqlCommand(query, coon))
                {

                    if (string.IsNullOrWhiteSpace(name.Text) ||
                        string.IsNullOrWhiteSpace(username.Text) ||
                        string.IsNullOrWhiteSpace(email.Text) ||
                        string.IsNullOrWhiteSpace(avatarSelecionado))
                    {
                        MessageBox.Show(
                            "Todos os campos devem estar preenchidos!",
                            "Atenção",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        return;
                    }

                    cmd.Parameters.AddWithValue("@id",idLog);
                    cmd.Parameters.AddWithValue("@nome_completo", name.Text);
                    cmd.Parameters.AddWithValue("@username", username.Text);
                    cmd.Parameters.AddWithValue("@email", email.Text);
                    cmd.Parameters.AddWithValue("@avatar", avatarSelecionado);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Cadastro realizado com sucesso");
                    MainWindow abrir = new MainWindow(idLog);
                    abrir.Show();
                    this.Close();
                }

            }
        }
    }
}