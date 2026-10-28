#!/usr/bin/env bash
# Roda os testes unitários e de integração, junta a cobertura e confere o mínimo.
# Uso (na raiz do repositório):  bash scripts/coverage.sh
# Requer: Docker em execução (testes de integração) e
#         dotnet tool install --global dotnet-reportgenerator-globaltool
set -euo pipefail

CONFIG="${CONFIG:-Release}"

rm -rf coverage
dotnet build -c "$CONFIG"

for projeto in UnitTests IntegrationTests; do
  dotnet test "tests/OficinaCadastros.${projeto}" --no-build -c "$CONFIG" \
    --settings coverlet.runsettings \
    --collect:"XPlat Code Coverage" \
    --results-directory "./coverage/${projeto}"
done

reportgenerator \
  -reports:"coverage/**/coverage.cobertura.xml" \
  -targetdir:coverage/report \
  -reporttypes:"Cobertura;TextSummary;MarkdownSummaryGithub;Html"

bash scripts/check-coverage.sh coverage/report/Cobertura.xml
