## CRIAÇÃO DO BANCO

### 1 - Adicione a connection string

* Execute o dump.sql, isso ira criar o banco "FIAPDesafioPlenoDb" e cadastrar todos os dados
* No arquivo appsettings.json (da API) preencha a connection string.  

    Exemplo:

    "DefaultConnection": "Server=DESKTOP-P93N2NG\\SQLEXPRESS;Database=FIAPDesafioPlenoDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

### 2 - Adicionar pacotes necessários

Abra o terminal no diretório do projeto e rode:

* dotnet add package Microsoft.EntityFrameworkCore.SqlServer
* dotnet add package Microsoft.EntityFrameworkCore.Design
* dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
* dotnet add package Swashbuckle.AspNetCore


### 3 - Criar Migration inicial

* dotnet ef migrations add InitialCreate


### 4 - Aplicar Migration

* dotnet ef database update

Isso vai gerar/mapear as tabelas Alunos, Turmas, Matriculas no banco.


## CONFIGURAÇÃO DO MVC (EM CASO DE ERRO)

Caso a API rode em uma porta diferente de :7131 (https://localhost:7131) configure o endereço no appsettings.json do MVC

### ADMIN PADRÃO

admin@fiap.com
Admin@123
