```csharp
using MySql.Data.MySqlClient;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Internal logic for Read.xaml
    /// </summary>
    public partial class Read : Window
    {
        public string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        private int idAdmin;
        private string perfilUsuarioLogado = "";

        public Read(int idLogado)
        {
            InitializeComponent();

            idAdmin = idLogado;

            CarregarPerfilUsuarioLogado();
            carregarUsuarios();
        }

        // ==============================
        // LOAD LOGGED USER PROFILE
        // ==============================

        private void CarregarPerfilUsuarioLogado()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT perfil_acesso
                        FROM usuarios
                        WHERE id = @id";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", idAdmin);

                        object resultado = command.ExecuteScalar();

                        if (resultado != null)
                        {
                            perfilUsuarioLogado = resultado.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading user profile:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // BACK BUTTON
        // ==============================

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (perfilUsuarioLogado == "Administrador")
            {
                HubAdmin abrir = new HubAdmin(idAdmin);
                abrir.Show();
                this.Close();
            }
            else
            {
                MainWindow abrir = new MainWindow(idAdmin);
                abrir.Show();
                this.Close();
            }
        }

        // ==============================
        // CREATE USER CARD
        // ==============================

        private void CriarCard(string nomeCompleto, string user, string email, string avatar, string tipoUser, string dataCriacao, string lastLogin, string status)
        {
            if (string.IsNullOrWhiteSpace(lastLogin))
            {
                lastLogin = "Login not performed";
            }

            if (string.IsNullOrWhiteSpace(status))
            {
                status = "Unknown";
            }

            string statusExibicao = status;

            if (status == "Ativo")
            {
                statusExibicao = "ACTIVE";
            }
            else if (status == "Desativado")
            {
                statusExibicao = "DISABLED";
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
                imagemAvatar.Source = new BitmapImage(new Uri(avatar, UriKind.RelativeOrAbsolute));
            }
            catch
            {
                // Keeps the card without an image if the path is invalid
            }

            TextBlock nomeText = new TextBlock
            {
                Text = nomeCompleto,
                FontSize = 18,
                FontWeight = FontWeights.Bold
            };

            TextBlock usuarioText = new TextBlock
            {
                Text = "User: " + user
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
                Text = "Last login: " + lastLogin
            };

            TextBlock statusText = new TextBlock
            {
                Text = "Status: " + statusExibicao
            };

            conteudo.Children.Add(imagemAvatar);
            conteudo.Children.Add(nomeText);
            conteudo.Children.Add(usuarioText);
            conteudo.Children.Add(emailText);
            conteudo.Children.Add(tipoText);
            conteudo.Children.Add(loginText);
            conteudo.Children.Add(statusText);

            card.Child = conteudo;

            PainelUsuarios.Children.Add(card);
        }

        // ==============================
        // LOAD ALL USERS
        // ==============================

        private void carregarUsuarios()
        {
            PainelUsuarios.Children.Clear();

            try
            {
                using (MySqlConnection coon = new MySqlConnection(conexao))
                {
                    coon.Open();

                    string sql = @"
                        SELECT
                            nome_completo,
                            username,
                            email,
                            avatar,
                            tipo_usuario,
                            data_criacao,
                            ultimo_login,
                            status
                        FROM usuarios";

                    using (MySqlCommand cmd = new MySqlCommand(sql, coon))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            CriarCard(
                                reader["nome_completo"].ToString(),
                                reader["username"].ToString(),
                                reader["email"].ToString(),
                                reader["avatar"].ToString(),
                                reader["tipo_usuario"].ToString(),
                                reader["data_criacao"].ToString(),
                                reader["ultimo_login"].ToString(),
                                reader["status"].ToString()
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading users:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // SEARCH
        // ==============================

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            string nomeBusca = TxtPesquisa.Text.Trim();

            PainelUsuarios.Children.Clear();

            try
            {
                using (MySqlConnection coon = new MySqlConnection(conexao))
                {
                    coon.Open();

                    string sql = @"
                        SELECT
                            nome_completo,
                            username,
                            email,
                            avatar,
                            tipo_usuario,
                            data_criacao,
                            ultimo_login,
                            status
                        FROM usuarios
                        WHERE nome_completo LIKE @nome";

                    using (MySqlCommand cmd = new MySqlCommand(sql, coon))
                    {
                        cmd.Parameters.AddWithValue("@nome", "%" + nomeBusca + "%");

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
                                    reader["status"].ToString()
                                );
                            }

                            if (!encontrou)
                            {
                                MessageBox.Show("No users found!", "Search", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error searching users:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ==============================
        // FILTER USERS
        // ==============================

        private void FiltrarUsuarios()
        {
            PainelUsuarios.Children.Clear();

            try
            {
                using (MySqlConnection coon = new MySqlConnection(conexao))
                {
                    coon.Open();

                    string sql = @"
                        SELECT
                            nome_completo,
                            username,
                            email,
                            avatar,
                            tipo_usuario,
                            data_criacao,
                            ultimo_login,
                            status
                        FROM usuarios
                        WHERE 1=1";

                    using (MySqlCommand cmd = new MySqlCommand())
                    {
                        cmd.Connection = coon;

                        // PROFILE
                        if (CmbPerfil.SelectedItem != null)
                        {
                            string perfil = ((ComboBoxItem)CmbPerfil.SelectedItem).Content.ToString();

                            if (perfil != "Todos" && perfil != "All")
                            {
                                sql += " AND tipo_usuario = @perfil";
                                cmd.Parameters.AddWithValue("@perfil", perfil);
                            }
                        }

                        // STATUS
                        if (CmbStatus.SelectedItem != null)
                        {
                            string status = ((ComboBoxItem)CmbStatus.SelectedItem).Content.ToString();

                            if (status == "Ativo" || status == "Active")
                            {
                                sql += " AND status = 'Ativo'";
                            }
                            else if (status == "Desativado" || status == "Disabled" || status == "Bloqueado" || status == "Blocked")
                            {
                                sql += " AND status = 'Desativado'";
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
                                    reader["status"].ToString()
                                );
                            }

                            if (!encontrou)
                            {
                                MessageBox.Show("No users found!", "Filter", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error filtering users:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
```
