#!/usr/bin/env bash
# Roda os testes unitários e de integração, junta a cobertura e confere o mínimo.
# Uso (na raiz do repositório):  bash scripts/coverage.sh
# Requer: Docker em execução (testes de integração) e
#         dotnet tool install --global dotnet-reportgenerator-globaltool
set -euo pipefail

# Docker Desktop (Windows) costuma falhar na primeira criação de container depois de ocioso;
# um aquecimento com nova tentativa evita derrubar a suíte inteira por isso. No CI não roda.
if [[ -z "${CI:-}" ]]; then
  for tentativa in 1 2 3; do
    if docker run --rm hello-world >/dev/null 2>&1; then break; fi
    echo "Docker ainda nao respondeu (tentativa $tentativa/3), aguardando..."
    sleep 5
  done
fi

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
