using System;
using System.Windows;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para TelaLogin.xaml
    /// </summary>
    public partial class TelaLogin : Window
    {
        private string conexao =
            "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        public TelaLogin()
        {
            InitializeComponent();

            VerificarAdministrador();
        }

        // Verifica se já existe algum administrador cadastrado
        private void VerificarAdministrador()
        {
            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE tipo_usuario = 'Admin'";

                    using (MySqlCommand command =
                        new MySqlCommand(sql, connection))
                    {
                        int quantidadeAdmins =
                            Convert.ToInt32(command.ExecuteScalar());

                        // Se não existir nenhum administrador
                        if (quantidadeAdmins == 0)
                        {
                            MessageBox.Show(
                                "Nenhum administrador foi cadastrado ainda.\n" +
                                "Cadastre o primeiro administrador para continuar.",
                                "Primeiro acesso",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                            AdminCadastro adminCadastro =
                                new AdminCadastro();

                            adminCadastro.Show();

                            this.Close();
                        }
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
            }
        }

        // BOTÃO LOGIN
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = TxtUsername.Text.Trim();
            string senha = TxtPassword.Password;

            // Verifica campos vazios
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show(
                    "Usuário ou senha inválidos.",
                    "Login",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                return;
            }

            try
            {
                using (MySqlConnection connection =
                    new MySqlConnection(conexao))
                {
                    connection.Open();

                    string sql = @"
                        SELECT
                            id,
                            nome_completo,
                            username,
                            senha,
                            avatar,
                            tipo_usuario,
                            perfil_acesso,
                            status,
                            tentativas_login,
                            bloqueado_ate
                        FROM usuarios
                        WHERE username = @username
                        LIMIT 1";

                    using (MySqlCommand command =
                        new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@username", username);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            // Usuário não existe
                            if (!reader.Read())
                            {
                                MessageBox.Show(
                                    "Usuário ou senha inválidos.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            int id =
                                Convert.ToInt32(reader["id"]);

                            string senhaHash =
                                reader["senha"].ToString();

                            string status =
                                reader["status"].ToString();

                            string tipoUsuario =
                                reader["tipo_usuario"].ToString();

                            string perfilAcesso =
                                reader["perfil_acesso"].ToString();

                            int tentativas =
                                Convert.ToInt32(
                                    reader["tentativas_login"]);

                            DateTime? bloqueadoAte = null;

                            if (reader["bloqueado_ate"] != DBNull.Value)
                            {
                                bloqueadoAte =
                                    Convert.ToDateTime(
                                        reader["bloqueado_ate"]);
                            }

                            // Verifica bloqueio temporário
                            if (bloqueadoAte.HasValue &&
                                bloqueadoAte.Value > DateTime.UtcNow)
                            {
                                MessageBox.Show(
                                    "Usuário ou senha inválidos.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            // Verifica se está ativo
                            if (status != "Ativo")
                            {
                                MessageBox.Show(
                                    "Usuário ou senha inválidos.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            // Compara a senha com o HASH
                            bool senhaCorreta =
                                BCrypt.Net.BCrypt.Verify(
                                    senha,
                                    senhaHash);

                            // SENHA INCORRETA
                            if (!senhaCorreta)
                            {
                                reader.Close();

                                tentativas++;

                                // 5 tentativas = bloqueio por 5 minutos
                                if (tentativas >= 5)
                                {
                                    string bloquear = @"
                                        UPDATE usuarios
                                        SET tentativas_login = @tentativas,
                                            bloqueado_ate =
                                                DATE_ADD(
                                                    UTC_TIMESTAMP(),
                                                    INTERVAL 5 MINUTE)
                                        WHERE id = @id";

                                    using (MySqlCommand update =
                                        new MySqlCommand(
                                            bloquear,
                                            connection))
                                    {
                                        update.Parameters.AddWithValue(
                                            "@tentativas",
                                            tentativas);

                                        update.Parameters.AddWithValue(
                                            "@id",
                                            id);

                                        update.ExecuteNonQuery();
                                    }
                                }
                                else
                                {
                                    string atualizarTentativas = @"
                                        UPDATE usuarios
                                        SET tentativas_login = @tentativas
                                        WHERE id = @id";

                                    using (MySqlCommand update =
                                        new MySqlCommand(
                                            atualizarTentativas,
                                            connection))
                                    {
                                        update.Parameters.AddWithValue(
                                            "@tentativas",
                                            tentativas);

                                        update.Parameters.AddWithValue(
                                            "@id",
                                            id);

                                        update.ExecuteNonQuery();
                                    }
                                }

                                MessageBox.Show(
                                    "Usuário ou senha inválidos.",
                                    "Login",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);

                                return;
                            }

                            // LOGIN CORRETO
                            reader.Close();

                            string atualizarLogin = @"
                                UPDATE usuarios
                                SET tentativas_login = 0,
                                    bloqueado_ate = NULL,
                                    status = 'Ativo',
                                    ultimo_login = UTC_TIMESTAMP()
                                WHERE id = @id";

                            using (MySqlCommand update =
                                new MySqlCommand(
                                    atualizarLogin,
                                    connection))
                            {
                                update.Parameters.AddWithValue(
                                    "@id", id);

                                update.ExecuteNonQuery();
                            }

                            // IDENTIFICA AS PERMISSÕES

                            if (tipoUsuario == "Admin" ||
                                perfilAcesso == "Administrador")
                            {
                                HubAdmin hubAdmin =
                                    new HubAdmin();

                                hubAdmin.Show();
                            }
                            else
                            {
                                MainWindow hubUsuario =
                                    new MainWindow();

                                hubUsuario.Show();
                            }

                            this.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao realizar o login:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
