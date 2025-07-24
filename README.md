# 📌 Projeto: Role-Based Task Manager
Descrição
Este é um sistema de gerenciamento de tarefas desenvolvido com ASP.NET Core, que implementa autenticação com JWT e controle de acesso baseado em papéis (Role-Based Access Control). O projeto utiliza SQL puro (SqlCommand / SqlConnection) para comunicação direta com o banco de dados, sem o uso de ORMs como o Entity Framework.

## Funcionalidades
🧾 CRUD de Tarefas

👤 Cadastro de Usuários

🔐 Login com autenticação JWT

🛡️ Autorização por papéis (Admin/User)

📊 Tarefas com status, prioridade, data de criação e conclusão

Perfis de Acesso
Admin

Pode criar, visualizar, atualizar e deletar qualquer tarefa

Pode cadastrar novos usuários

User

Pode gerenciar somente suas próprias tarefas

### Tecnologias Utilizadas
ASP.NET Core Web API

SQL Server

JWT (JSON Web Token)

Swagger para documentação de API

Dapper (opcional, se quiser facilitar consultas futuras)

SqlClient (System.Data.SqlClient)

_____________________________________________________________________________________________________________________

## Features
🧾 Task CRUD
👤 User Registration
🔐 JWT-based Login Authentication
🛡️ Role-Based Authorization (Admin/User)
📊 Tasks with status, priority, creation and completion dates

Access Profiles
Admin

Can create, view, update, and delete any task

Can register new users

User

Can manage only their own tasks

### Technologies Used
ASP.NET Core Web API

SQL Server

JWT (JSON Web Token)

Swagger for API documentation

Dapper (optional, to simplify future queries)

SqlClient (System.Data.SqlClient)