## CRIAÇÃO DO BANCO



### 1 - Adicionar pacotes necessários

Abra o terminal no diretório do projeto e rode:

* dotnet add package Microsoft.EntityFrameworkCore.SqlServer
* dotnet add package Microsoft.EntityFrameworkCore.Design
* dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
* dotnet add package Swashbuckle.AspNetCore


### 2 - Criar Migration inicial

dotnet ef migrations add InitialCreate


### 3 - Aplicar Migration

dotnet ef database update


Isso vai gerar as tabelas Alunos, Turmas, Matriculas no banco.



## CRIAÇÃO DO USUARIO ADMIN



No Program.cs procure o trecho:

"Descomente ABAIXO para rodar apenas UMA VEZ na criação do usuario ADMIN"

e descomente o trecho abaixo dessa linha.