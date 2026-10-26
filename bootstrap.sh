#!/usr/bin/env bash
# Cria a solution e liga os projetos. Rode uma única vez, na raiz do repositório.
set -euo pipefail
dotnet new sln -n OficinaCadastros
dotnet sln add src/OficinaCadastros.Domain/OficinaCadastros.Domain.csproj
dotnet sln add src/OficinaCadastros.Application/OficinaCadastros.Application.csproj
dotnet sln add src/OficinaCadastros.Infrastructure/OficinaCadastros.Infrastructure.csproj
dotnet sln add src/OficinaCadastros.API/OficinaCadastros.API.csproj
dotnet sln add tests/OficinaCadastros.UnitTests/OficinaCadastros.UnitTests.csproj
printf '\n# Terraform\n.terraform/\n*.tfstate\n*.tfstate.*\n*.tfvars\n' >> .gitignore
echo "Solution criada. Próximo: dotnet build && dotnet test"
