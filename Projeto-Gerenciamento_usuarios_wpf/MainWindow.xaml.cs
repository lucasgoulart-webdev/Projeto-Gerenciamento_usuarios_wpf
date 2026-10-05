using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Interação lógica para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        public string Nome_userLogado;

        private int idLog;
        public MainWindow(int idLgado)
        {
            InitializeComponent();
            idLog = idLgado;
            carregarUsuario();
            txtInicial.Content = $"Hello {Nome_userLogado}, Welcome!";
        }



        void carregarUsuario()
        {
            string sql = @"
        SELECT nome_completo, username, email, perfil_acesso, avatar, ultimo_login
        FROM usuarios
        WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(conexao))
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", idLog);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Nome_userLogado = reader["nome_completo"].ToString();
                            username.Text = reader["username"].ToString();
                            email.Text = reader["email"].ToString();
                            tipoUser.Text = reader["perfil_acesso"].ToString();
                            lastLogin.Text = reader["ultimo_login"].ToString();
                            string avatar = reader["avatar"].ToString();
                            Nome_completo.Text= reader["nome_completo"].ToString();

                            imagem.Source = new BitmapImage(
                                new Uri(
                                    $"pack://application:,,,/{avatar}",
                                    UriKind.Absolute));
                        }
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Read abrir = new Read(idLog);
            abrir.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            editarPerfil abrir = new editarPerfil(idLog);
            abrir.Show();
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Redefinir_senha abrir = new Redefinir_senha(idLog);
            abrir.Show();
            this.Close();
        }
    }
}
