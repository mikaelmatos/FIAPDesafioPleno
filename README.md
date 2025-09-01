## CRIAÇÃO DO BANCO

### 1 - Adicione a connection string

* Crie um novo banco de dados no seu SQLServer com nome "FIAPDesafioPlenoDb"
* No aquivo appsettings.json (da API) preencha a connection string. 

    Exemplo:

    "DefaultConnection": "Server=DESKTOP-P93N2NG\\SQLEXPRESS;Database=FIAPDesafioPlenoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

### 2 - Adicionar pacotes necessários

Abra o terminal no diretório do projeto e rode:

dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Swashbuckle.AspNetCore


### 3 - Criar Migration inicial

* dotnet ef migrations add InitialCreate


### 4 - Aplicar Migration

* dotnet ef database update


Isso vai gerar as tabelas Alunos, Turmas, Matriculas no banco.



## CRIAÇÃO DO USUARIO ADMIN

No Program.cs (API) procure o trecho:

"Descomente ABAIXO para rodar apenas UMA VEZ na criação do usuario ADMIN"

(apenas durante a instalação).

Obs: Não fiz um INSERT por conta da senha em HASH


## CONFIGURAÇÃO DO MVC (EM CASO DE ERRO)

Caso a API rode em uma porta diferente de :7131 (https://localhost:7131) configure o endereço no appsettings.json do MVC


