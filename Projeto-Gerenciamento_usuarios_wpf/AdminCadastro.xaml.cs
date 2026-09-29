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
using System.Text.RegularExpressions;
using MySql.Data.MySqlClient;
using BCrypt.Net;

namespace Projeto_Gerenciamento_usuarios_wpf
{
    /// <summary>
    /// Lógica interna para AdminCadastro.xaml
    /// </summary>
    public partial class AdminCadastro : Window
    {
        private string conexao =
            "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        public AdminCadastro()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            string nome = TxtNome.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();

            string senha = TxtPassword.Password;
            string confirmarSenha = TxtConfirmPassword.Password;

            string avatar = "Imagens/adminAvatar.jpg";

            // Nome obrigatório
            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show(
                    "O nome é obrigatório.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Username obrigatório
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "O nome de usuário é obrigatório.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Username mínimo de 3 caracteres
            if (username.Length < 3)
            {
                MessageBox.Show(
                    "O nome de usuário deve possuir no mínimo 3 caracteres.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // E-mail obrigatório
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show(
                    "O e-mail é obrigatório.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Formato do e-mail
            string padraoEmail =
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, padraoEmail))
            {
                MessageBox.Show(
                    "Digite um e-mail válido.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Senha obrigatória
            if (string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show(
                    "A senha é obrigatória.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Senha mínima de 8 caracteres
            if (senha.Length < 8)
            {
                MessageBox.Show(
                    "A senha deve possuir no mínimo 8 caracteres.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Confirmação obrigatória
            if (string.IsNullOrWhiteSpace(confirmarSenha))
            {
                MessageBox.Show(
                    "A confirmação da senha é obrigatória.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Senhas iguais
            if (senha != confirmarSenha)
            {
                MessageBox.Show(
                    "A senha e a confirmação devem ser iguais.",
                    "Cadastro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            // Avatar obrigatório
            if (string.IsNullOrWhiteSpace(avatar))
            {
                MessageBox.Show(
                    "Uma imagem de perfil deve ser selecionada.",
                    "Cadastro",
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

                    // Verifica username e e-mail duplicados
                    string verificar = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE username = @username
                           OR email = @email";

                    using (MySqlCommand command =
                        new MySqlCommand(verificar, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@username", username);

                        command.Parameters.AddWithValue(
                            "@email", email);

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

                    // Cria o hash da senha
                    string senhaHash =
                        BCrypt.Net.BCrypt.HashPassword(senha);

                    // Dados automáticos do administrador
                    string tipoUsuario = "Admin";
                    string perfilAcesso = "Administrador";
                    string status = "Ativo";

                    // Insere o administrador
                    string sql = @"
                        INSERT INTO usuarios
                        (
                            nome_completo,
                            username,
                            email,
                            senha,
                            avatar,
                            tipo_usuario,
                            perfil_acesso,
                            status,
                            tentativas_login,
                            bloqueado_ate,
                            data_criacao,
                            data_alteracao
                        )
                        VALUES
                        (
                            @nome,
                            @username,
                            @email,
                            @senha,
                            @avatar,
                            @tipo_usuario,
                            @perfil_acesso,
                            @status,
                            0,
                            NULL,
                            UTC_TIMESTAMP(),
                            UTC_TIMESTAMP()
                        )";

                    using (MySqlCommand command =
                        new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@nome", nome);

                        command.Parameters.AddWithValue(
                            "@username", username);

                        command.Parameters.AddWithValue(
                            "@email", email);

                        command.Parameters.AddWithValue(
                            "@senha", senhaHash);

                        command.Parameters.AddWithValue(
                            "@avatar", avatar);

                        command.Parameters.AddWithValue(
                            "@tipo_usuario", tipoUsuario);

                        command.Parameters.AddWithValue(
                            "@perfil_acesso", perfilAcesso);

                        command.Parameters.AddWithValue(
                            "@status", status);

                        command.ExecuteNonQuery();
                    }

                    MessageBox.Show(
                        "Administrador cadastrado com sucesso!",
                        "Cadastro",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    TelaLogin telaLogin = new TelaLogin();
                    telaLogin.Show();

                    this.Close();
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(
                    "Erro no banco de dados:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao cadastrar administrador:\n" + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
