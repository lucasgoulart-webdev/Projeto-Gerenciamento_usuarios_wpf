
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

        public Update(int idLogado)
        {
            InitializeComponent();
            idlogin = idLogado;
        }

        private void Avatar1_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Avatar2_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Avatar3_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Avatar4_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Avatar5_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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

                string sql = @"SELECT id, nome_completo, username, email, status, perfil_acesso FROM usuarios WHERE email=@email";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("email", emailSearch.Text);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                        {
                            MessageBox.Show("Email não encontrado no banco!", "update", MessageBoxButton.OK);
                            emailSearch.Clear();
                        }
                        else
                        {
                            int id =
                               Convert.ToInt32(reader["id"]);

                            name.Text = reader["nome_completo"].ToString();
                            user.Text = reader["username"].ToString();
                            email.Text = reader["email"].ToString();
                            cmbStatus.Text = reader["status"].ToString();
                            cmbProfile.Text = reader["perfil_acesso"].ToString();


                        }
                    }
                }
            }
        }

        private void emailSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (emailSearch.Text=="")
            {
                name.Clear();
                user.Clear();
                email.Clear();
            }
        }
    }
}

