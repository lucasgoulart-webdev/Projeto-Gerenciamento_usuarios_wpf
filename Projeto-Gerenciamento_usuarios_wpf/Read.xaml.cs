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
    /// Lógica interna para Read.xaml
    /// </summary>
    public partial class Read : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        private int idAdmin;
        public Read(int idLogado)
        {
            InitializeComponent();
            carregarUsuarios();


            idAdmin = idLogado;
        }

        

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            HubAdmin abrir = new HubAdmin(idAdmin);
            abrir.Show();
            this.Close();
        }



        private void CriarCard(string nomeCompleto, string user, string email, string avatar, string tipoUser, string dataCriacao, string lastLogin, string bloqueado)
        {
            if (lastLogin == "")
            {
                lastLogin = "Login não efetuado";
            }

            if (bloqueado=="")
            {
                bloqueado = "ATIVO";
            }

            Border card = new Border
            {
                Width = 250,
                Height = 210,
                Margin = new Thickness(10),
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Colors.White)
            };

            StackPanel conteudo = new StackPanel
            {
                Margin = new Thickness(15)
            };

            // Avatar
            Image imagemAvatar = new Image
            {
                Width = 50,
                Height = 50,
                Stretch = Stretch.UniformToFill,
                Margin = new Thickness(0, 0, 0, 5)
            };

            try
            {
                imagemAvatar.Source = new BitmapImage(
                    new Uri(avatar, UriKind.RelativeOrAbsolute)
                );
            }
            catch
            {
                // Caso o caminho da imagem esteja errado
            }

            
            TextBlock nomeText = new TextBlock
            {
                Text = nomeCompleto,
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };

        
            TextBlock usuarioText = new TextBlock
            {
                Text = "user: " + user
            };

         
            TextBlock emailText = new TextBlock
            {
                Text = email
            };

       
            TextBlock tipoText = new TextBlock
            {
                Text = tipoUser
            };

            TextBlock loginText = new TextBlock
            {

                Text = "Último login: " + lastLogin
            };

            TextBlock blockedText = new TextBlock
            {

                Text = bloqueado
            };


            conteudo.Children.Add(imagemAvatar);              // Adicionando os elementos no card
            conteudo.Children.Add(nomeText);
            conteudo.Children.Add(usuarioText);
            conteudo.Children.Add(emailText);
            conteudo.Children.Add(tipoText);
            conteudo.Children.Add(loginText);
            conteudo.Children.Add(blockedText);

            card.Child = conteudo;

            PainelUsuarios.Children.Add(card);
        }


        private void carregarUsuarios()
        {
            PainelUsuarios.Children.Clear();

            using (MySqlConnection coon = new MySqlConnection(conexao))
            {
                coon.Open();

                string sql = "SELECT nome_completo, username, email, avatar, tipo_usuario, data_criacao, ultimo_login, bloqueado_ate FROM usuarios";

                using (MySqlCommand cmd = new MySqlCommand(sql, coon))
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CriarCard(reader["nome_completo"].ToString(), reader["username"].ToString(), reader["email"].ToString(), reader["avatar"].ToString(), reader["tipo_usuario"].ToString(), reader["data_criacao"].ToString(), reader["ultimo_login"].ToString(), reader["bloqueado_ate"].ToString());   
                        
                    }
                }
                

                
            }

        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {

            string nomeBusca = TxtPesquisa.Text.Trim();

            PainelUsuarios.Children.Clear();

            using (MySqlConnection coon = new MySqlConnection(conexao))
            {
                coon.Open();

                string sql = "SELECT nome_completo, username, email, avatar, tipo_usuario, data_criacao, ultimo_login, bloqueado_ate FROM usuarios WHERE nome_completo LIKE @nome";

                using (MySqlCommand cmd = new MySqlCommand(sql, coon))
                {

                    cmd.Parameters.AddWithValue("@nome","%" + nomeBusca + "%");

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool encontrou = false;

                        while (reader.Read())
                        {
                            encontrou= true;
                            CriarCard(reader["nome_completo"].ToString(), reader["username"].ToString(), reader["email"].ToString(), reader["avatar"].ToString(), reader["tipo_usuario"].ToString(), reader["data_criacao"].ToString(), reader["ultimo_login"].ToString(), reader["bloqueado_ate"].ToString());

                        }
                        if (!encontrou)
                        {
                            MessageBox.Show("Nenhum usuário encontrado!");
                        }
                    }
                }

               



            }
        }


            private void FiltrarUsuarios()
            {
            PainelUsuarios.Children.Clear();

            using (MySqlConnection coon = new MySqlConnection(conexao))
            {
                coon.Open();

                string sql = @"SELECT nome_completo, username, email, avatar ,tipo_usuario, data_criacao, ultimo_login, bloqueado_ate FROM usuarios  WHERE 1=1";
            
                   
            
           

                using (MySqlCommand cmd = new MySqlCommand())
                {
                    cmd.Connection = coon;

                    //  PERFIL
                    if (CmbPerfil.SelectedItem != null)
                    {
                        string perfil = ((ComboBoxItem)CmbPerfil.SelectedItem).Content.ToString();

                        if (perfil != "Todos")
                        {
                            sql += " AND tipo_usuario = @perfil";
                            cmd.Parameters.AddWithValue("@perfil", perfil);
                        }
                    }

                    // STATUS
                    if (CmbStatus.SelectedItem != null)
                    {
                        string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                        if (status == "Ativo")
                        {
                            sql += " AND bloqueado_ate IS NULL";
                        }
                        else if (status == "Bloqueado")
                        {
                            sql += " AND bloqueado_ate IS NOT NULL";
                        }
                    }

                    cmd.CommandText = sql;

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        bool encontrou = false;

                        while (reader.Read())
                        {
                            encontrou = true;

                            CriarCard(
                                reader["nome_completo"].ToString(),
                                reader["username"].ToString(),
                                reader["email"].ToString(),
                                reader["avatar"].ToString(),
                                reader["tipo_usuario"].ToString(),
                                reader["data_criacao"].ToString(),
                                reader["ultimo_login"].ToString(),
                                reader["bloqueado_ate"].ToString()
                            );
                        }

                        if (!encontrou)
                        {
                            MessageBox.Show("Nenhum usuário encontrado!");
                        }
                    }
                }
            }
        }
        
        private void CmbPerfil_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarUsuarios();
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FiltrarUsuarios();
        }

        private void TxtPesquisa_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtPesquisa.Text == "")
            {
                carregarUsuarios();
            }
        }
    }
}
