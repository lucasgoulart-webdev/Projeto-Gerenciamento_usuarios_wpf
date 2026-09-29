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
using static Mysqlx.Datatypes.Scalar.Types;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para HubAdmin.xaml
    /// </summary>
    public partial class HubAdmin : Window
    {
        private string conexao =
            "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";
        public HubAdmin(int idUsuario)
        {
            InitializeComponent();
            carregar(idUsuario);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Create criar = new Create();
            criar.Show();
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Read criar1 = new Read();
            criar1.Show();
            this.Close();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Update criar2 = new Update();
            criar2.Show();
            this.Close();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
           //tela delete tem que passar argumento
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            Unable criar4 = new Unable();
            criar4.Show();
            this.Close();
        }

        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            Logs criar5 = new Logs();
            criar5.Show();
            this.Close();
        }

        private void carregar(int idUsuario)
        {
            string sql = @"
        SELECT nome_completo, email, perfil_acesso, avatar
        FROM usuarios
        WHERE id = @id";

            using (MySqlConnection connection = new MySqlConnection(conexao))
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@id", idUsuario);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Nome.Text = reader["nome_completo"].ToString();
                            email.Text = reader["email"].ToString();
                            perfil.Text = reader["perfil_acesso"].ToString();
                            string avatar = reader["avatar"].ToString();

                            imagem.Source = new BitmapImage(
                                new Uri(
                                    $"pack://application:,,,/{avatar}",
                                    UriKind.Absolute));
                        }
                    }
                }
            }
        }
    }
}
