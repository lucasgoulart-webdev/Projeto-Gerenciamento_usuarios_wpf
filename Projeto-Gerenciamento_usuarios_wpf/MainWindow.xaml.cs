using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Interação lógica para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Conexão com o banco de dados
        private string conexao =
            "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        public MainWindow()
        {
            InitializeComponent();
        }

        // Verifica se existe algum administrador cadastrado
        private bool ExisteAdmin()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = "SELECT COUNT(*) FROM usuarios WHERE tipo_usuario = 'Admin'";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        int quantidade = Convert.ToInt32(command.ExecuteScalar());

                        return quantidade > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao verificar administrador:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                return false;
            }
        }

        // Botão de Login
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Primeiro verifica se existe administrador
            if (!ExisteAdmin())
            {
                MessageBox.Show(
                    "Não tem nenhum administrador cadastrado.",
                    "Administrador",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                AdminCadastro adminCadastro = new AdminCadastro();
                adminCadastro.Show();

                this.Close();

                return;
            }

            // Pega os dados digitados
            string username = TxtUsername.Text;
            string senha = TxtPassword.Password;

            // Verifica se os campos estão vazios
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show(
                    "Preencha o usuário e a senha.",
                    "Login",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT id, nome_completo, username, senha,
                               avatar, tipo_usuario, status, tentativas_login
                        FROM usuarios
                        WHERE username = @username
                        LIMIT 1";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            // Usuário não encontrado
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    "Usuário ou senha incorretos.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            int id = Convert.ToInt32(reader["id"]);
                            string senhaBanco = reader["senha"].ToString();
                            string status = reader["status"].ToString();
                            int tentativas = Convert.ToInt32(reader["tentativas_login"]);

                            // Usuário já bloqueado
                            if (status == "Bloqueado")
                            {
                                MessageBox.Show(
                                    "Este usuário está bloqueado.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            // Senha incorreta
                            if (senha != senhaBanco)
                            {
                                reader.Close();

                                tentativas++;

                                if (tentativas >= 5)
                                {
                                    string bloquear = @"
                                        UPDATE usuarios
                                        SET tentativas_login = @tentativas,
                                            status = 'Bloqueado'
                                        WHERE id = @id";

                                    using (MySqlCommand update =
                                        new MySqlCommand(bloquear, connection))
                                    {
                                        update.Parameters.AddWithValue("@tentativas", tentativas);
                                        update.Parameters.AddWithValue("@id", id);

                                        update.ExecuteNonQuery();
                                    }

                                    MessageBox.Show(
                                        "Senha incorreta 5 vezes.\nO usuário foi bloqueado.",
                                        "Usuário bloqueado",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                }
                                else
                                {
                                    string atualizar = @"
                                        UPDATE usuarios
                                        SET tentativas_login = @tentativas
                                        WHERE id = @id";

                                    using (MySqlCommand update =
                                        new MySqlCommand(atualizar, connection))
                                    {
                                        update.Parameters.AddWithValue("@tentativas", tentativas);
                                        update.Parameters.AddWithValue("@id", id);

                                        update.ExecuteNonQuery();
                                    }

                                    MessageBox.Show(
                                        "Usuário ou senha incorretos.",
                                        "Login",
                                        MessageBoxButton.OK,
                                        MessageBoxImage.Warning);
                                }

                                return;
                            }

                            // Login correto
                            string tipoUsuario = reader["tipo_usuario"].ToString();

                            reader.Close();

                            string loginCorreto = @"
                                UPDATE usuarios
                                SET tentativas_login = 0,
                                    status = 'Ativo',
                                    ultimo_login = UTC_TIMESTAMP()
                                WHERE id = @id";

                            using (MySqlCommand update =
                                new MySqlCommand(loginCorreto, connection))
                            {
                                update.Parameters.AddWithValue("@id", id);
                                update.ExecuteNonQuery();
                            }

                            MessageBox.Show(
                                "Login realizado com sucesso!",
                                "Login",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            // Se for administrador
                            if (tipoUsuario == "Admin")
                            {
                                HubAdmin hubAdmin = new HubAdmin();
                                hubAdmin.Show();
                            }
                            // Se for usuário normal
                            else
                            {
                                // Aqui vamos abrir o Hub do usuário
                                // quando terminarmos essa tela.
                            }

                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao realizar login:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
