```csharp
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
    /// Lógica interna para Create.xaml
    /// </summary>
    public partial class Create : Window
    {
        private string conexao = "Server=localhost;Database=projeto_usuarios;Uid=root;Pwd=;";

        // Guarda o avatar escolhido
        private string avatarSelecionado = "";
        private int AdminLogado;

        public Create(int idUsuario)
        {
            InitializeComponent();
            AdminLogado = idUsuario;
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

        // ==============================
        // BOTÃO CREATE
        // ==============================

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            string nome = TxtNome.Text.Trim();
            string username = TxtUsername.Text.Trim();
            string email = TxtEmail.Text.Trim();

            string senha = TxtPassword.Password;
            string confirmarSenha = TxtConfirmPassword.Password;

            // ==============================
            // VALIDAÇÕES
            // ==============================

            // Nome obrigatório
            if (string.IsNullOrWhiteSpace(nome))
            {
                MessageBox.Show("O nome é obrigatório.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Username obrigatório
            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show("O nome de usuário é obrigatório.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Username mínimo de 3 caracteres
            if (username.Length < 3)
            {
                MessageBox.Show("O nome de usuário deve possuir no mínimo 3 caracteres.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // E-mail obrigatório
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("O e-mail é obrigatório.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Formato do e-mail
            string padraoEmail = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

            if (!Regex.IsMatch(email, padraoEmail))
            {
                MessageBox.Show("Digite um e-mail válido.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Senha obrigatória
            if (string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("A senha é obrigatória.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Senha mínima de 8 caracteres
            if (senha.Length < 8)
            {
                MessageBox.Show("A senha deve possuir no mínimo 8 caracteres.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Confirmação obrigatória
            if (string.IsNullOrWhiteSpace(confirmarSenha))
            {
                MessageBox.Show("A confirmação da senha é obrigatória.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Senhas iguais
            if (senha != confirmarSenha)
            {
                MessageBox.Show("A senha e a confirmação devem ser iguais.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Avatar obrigatório
            if (string.IsNullOrWhiteSpace(avatarSelecionado))
            {
                MessageBox.Show("Selecione uma imagem de perfil.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Perfil obrigatório
            if (ComboPerfil.SelectedIndex == -1)
            {
                MessageBox.Show("Selecione o nivel de perfil.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // ==============================
            // BANCO DE DADOS
            // ==============================

            try
            {
                using (MySqlConnection connection = new MySqlConnection(conexao))
                {
                    connection.Open();

                    // Verifica username e e-mail duplicados
                    string verificar = @"
                        SELECT COUNT(*)
                        FROM usuarios
                        WHERE username = @username
                        OR email = @email";

                    using (MySqlCommand command = new MySqlCommand(verificar, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@email", email);

                        int quantidade = Convert.ToInt32(command.ExecuteScalar());

                        if (quantidade > 0)
                        {
                            MessageBox.Show("O nome de usuário ou e-mail já está cadastrado.", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }

                    // ==============================
                    // HASH DA SENHA
                    // ==============================

                    string senhaHash = BCrypt.Net.BCrypt.HashPassword(senha);

                    // ==============================
                    // DADOS AUTOMÁTICOS DO USUÁRIO
                    // ==============================

                    string tipoUsuario = "Usuário";
                    string perfilAcesso = "Usuário";

                    if (ComboPerfil.Text == "Administrador")
                    {
                        tipoUsuario = "Administrador";
                        perfilAcesso = "Administrador";
                    }

                    string status = "Ativo";

                    // ==============================
                    // INSERE NO BANCO
                    // ==============================

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
                        );

                        SELECT LAST_INSERT_ID();";

                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@nome", nome);
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@email", email);
                        command.Parameters.AddWithValue("@senha", senhaHash);
                        command.Parameters.AddWithValue("@avatar", avatarSelecionado);
                        command.Parameters.AddWithValue("@tipo_usuario", tipoUsuario);
                        command.Parameters.AddWithValue("@perfil_acesso", perfilAcesso);
                        command.Parameters.AddWithValue("@status", status);

                        long idNovoUsuario = Convert.ToInt64(command.ExecuteScalar());

                        // ==============================
                        // AUDITORIA
                        // ==============================

                        // Busca o username do administrador logado
                        string adminUsername = "";

                        string buscarAdmin = @"
                            SELECT username
                            FROM usuarios
                            WHERE id = @idAdmin";

                        using (MySqlCommand commandAdmin = new MySqlCommand(buscarAdmin, connection))
                        {
                            commandAdmin.Parameters.AddWithValue("@idAdmin", AdminLogado);

                            object resultado = commandAdmin.ExecuteScalar();

                            if (resultado != null)
                            {
                                adminUsername = resultado.ToString();
                            }
                        }

                        // Registra o cadastro na auditoria
                        string inserirAuditoria = @"
                            INSERT INTO auditoria
                            (
                                usuario_responsavel_id,
                                usuario_responsavel,
                                operacao,
                                registro_afetado,
                                valor_anterior,
                                novo_valor
                            )
                            VALUES
                            (
                                @usuario_responsavel_id,
                                @usuario_responsavel,
                                @operacao,
                                @registro_afetado,
                                @valor_anterior,
                                @novo_valor
                            )";

                        using (MySqlCommand commandAuditoria = new MySqlCommand(inserirAuditoria, connection))
                        {
                            commandAuditoria.Parameters.AddWithValue("@usuario_responsavel_id", AdminLogado);
                            commandAuditoria.Parameters.AddWithValue("@usuario_responsavel", adminUsername);
                            commandAuditoria.Parameters.AddWithValue("@operacao", "Cadastro de usuário");
                            commandAuditoria.Parameters.AddWithValue("@registro_afetado", "ID: " + idNovoUsuario + " - Usuário: " + username);
                            commandAuditoria.Parameters.AddWithValue("@valor_anterior", DBNull.Value);

                            // A senha nunca é registrada na auditoria
                            string novoValor = "Nome: " + nome +
                                               "; Username: " + username +
                                               "; E-mail: " + email +
                                               "; Tipo: " + tipoUsuario +
                                               "; Perfil: " + perfilAcesso +
                                               "; Status: " + status +
                                               "; Avatar: " + avatarSelecionado;

                            commandAuditoria.Parameters.AddWithValue("@novo_valor", novoValor);
                            commandAuditoria.ExecuteNonQuery();
                        }

                        MessageBox.Show("Usuário cadastrado com sucesso!", "Cadastro", MessageBoxButton.OK, MessageBoxImage.Information);

                        HubAdmin novo = new HubAdmin(Admin
