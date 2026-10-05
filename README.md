# Projeto-Gerenciamento_usuarios_wpf


BASE DO BANCO DE DADOS.
use projeto_usuarios;







CREATE TABLE usuarios (

id INT AUTO_INCREMENT PRIMARY KEY,

nome_completo VARCHAR(100) NOT NULL,

username VARCHAR(50) NOT NULL UNIQUE,

email VARCHAR(150) NOT NULL UNIQUE,

senha VARCHAR(255) NOT NULL,

avatar VARCHAR(100) NOT NULL,

tipo_usuario VARCHAR(20) NOT NULL DEFAULT 'Usuário',

perfil_acesso VARCHAR(50) NOT NULL DEFAULT 'Usuário',

status VARCHAR(20) NOT NULL DEFAULT 'Ativo',

tentativas_login INT NOT NULL DEFAULT 0,

bloqueado_ate DATETIME NULL,

data_criacao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

data_alteracao DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP

ON UPDATE CURRENT_TIMESTAMP,

ultimo_login DATETIME NULL

);




CREATE TABLE auditoria (

id INT AUTO_INCREMENT PRIMARY KEY,

data_hora DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

usuario_responsavel_id INT NOT NULL,

usuario_responsavel VARCHAR(50) NOT NULL,

operacao VARCHAR(50) NOT NULL,

registro_afetado VARCHAR(100) NOT NULL,

valor_anterior TEXT NULL,

novo_valor TEXT NULL

);




CREATE TABLE eventos_autenticacao (

id INT AUTO_INCREMENT PRIMARY KEY,

data_hora DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,

usuario_id INT NULL,

usuario VARCHAR(50) NULL,

tipo_evento VARCHAR(50) NOT NULL,

resultado VARCHAR(50) NOT NULL

);







#Query de Consulta aos usuarios;

select * from usuarios;

select * from auditoria;

select * from eventos_autenticacao;




drop table usuarios;




set time_zone = '+00:00';
