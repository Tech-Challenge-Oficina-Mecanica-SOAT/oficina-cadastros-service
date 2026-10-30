# language: pt
Funcionalidade: Cadastro de clientes e veículos
  Para que a oficina possa abrir ordens de serviço
  Como atendente
  Quero cadastrar os clientes e os veículos deles

  Cenário: Cadastro completo de cliente com veículo
    Dado que a API de cadastros está disponível
    Quando cadastro o cliente "Maria da Silva" com CPF "11122233344" e e-mail "maria@email.com"
    E cadastro o veículo de placa "bdd1a23" modelo "Onix" marca "Chevrolet" e ano 2021 para esse cliente
    Então o cliente é criado e fica ativo
    E o veículo é criado com a placa "BDD1A23"
    E consigo encontrar o cliente pelo CPF "11122233344"
    E consigo encontrar o veículo pela placa "bdd1a23"
    E o veículo encontrado pertence a esse cliente

  Cenário: Não é possível cadastrar dois clientes com o mesmo CPF
    Dado que a API de cadastros está disponível
    E que já existe um cliente cadastrado com o CPF "33344455566"
    Quando cadastro o cliente "Outra Pessoa" com CPF "33344455566" e e-mail "outra@email.com"
    Então a requisição é recusada por conflito com o código "CPF_JA_CADASTRADO"
