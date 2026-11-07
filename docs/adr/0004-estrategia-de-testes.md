# ADR 0004: Estratégia de testes e cobertura somada

**Status:** Aceito · **Data:** 2026-11-03

## Contexto
O plano exige cobertura mínima de 80% somando testes unitários e de integração, além de cenários BDD. Testes só com
repositórios falsos não pegam erro de mapeamento, de SQL, de serialização nem de configuração da API.

## Decisão
Três projetos de teste. **Unitários:** domínio e casos de uso, sem banco. **Integração:** a API inteira em memória
(`WebApplicationFactory`) contra um PostgreSQL real em container (Testcontainers), com as migrations aplicadas e o
publicador do outbox trocado por um falso. **BDD:** cenários em Gherkin (Reqnroll) para o cadastro completo e a
desativação com evento. A cobertura dos unitários e dos de integração é mesclada com o ReportGenerator
(`scripts/coverage.sh`) e o CI reprova abaixo de 80% de linhas. As migrations ficam fora da conta (código gerado).

## Consequências
- Os testes de integração e o BDD exigem Docker e levam cerca de um minuto.
- Somar percentuais de dois relatórios distorceria o resultado, por isso a mescla.
- Medido na última execução: 93,6% de linhas.
- O SNS real nunca é exercitado pela suíte (usa publicador falso), então precisa de verificação no ambiente.
