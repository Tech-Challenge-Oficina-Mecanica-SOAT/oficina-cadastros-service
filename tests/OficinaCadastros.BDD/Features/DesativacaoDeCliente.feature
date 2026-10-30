# language: pt
Funcionalidade: Desativação de cliente
  Para que os demais serviços reajam à saída de um cliente
  Como atendente
  Quero que a desativação de um cliente publique um evento

  Cenário: Desativação de cliente publica evento
    Dado que a API de cadastros está disponível
    E que existe um cliente ativo com CPF "55566677788"
    Quando desativo o cliente informando o motivo "inadimplencia"
    Então o cliente passa a constar como inativo
    E o evento "cliente.desativado" é publicado com o motivo "inadimplencia"
    E o evento publicado identifica o cliente desativado
    E o evento fica registrado como publicado no outbox

  Cenário: Desativar um cliente já desativado não gera novo evento
    Dado que a API de cadastros está disponível
    E que existe um cliente ativo com CPF "99988877766"
    E o cliente já foi desativado
    Quando tento desativar o cliente novamente
    Então a requisição é recusada por conflito com o código "CLIENTE_JA_DESATIVADO"
    E nenhum novo evento é publicado para esse cliente
