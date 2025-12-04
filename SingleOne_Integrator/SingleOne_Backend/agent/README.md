# SingleOne Agent - Agente de Inventário

## Visão Geral

O SingleOne Agent é um agente de inventário de ativos de TI inspirado no OCS Inventory, desenvolvido especificamente para o sistema SingleOne. Ele coleta informações de hardware, software e rede dos dispositivos e envia para o backend da SingleOne.

## Características

- 🖥️ **Coleta de Hardware**: CPU, memória, discos, placas de rede
- 💿 **Coleta de Software**: Programas instalados, versões, licenças
- 🌐 **Informações de Rede**: IP, MAC address, hostname
- 📊 **Envio Automático**: Comunicação segura com o backend
- ⚡ **Leve e Eficiente**: Mínimo impacto no sistema
- 🔄 **Execução Periódica**: Atualização automática do inventário
- 🪟🐧🍎 **Multiplataforma**: Windows, Linux e macOS

## Estrutura do Projeto

```
agent/
├── README.md                 # Este arquivo
├── requirements.txt          # Dependências Python
├── config/
│   ├── agent.yaml           # Configuração do agente
│   └── agent.example.yaml   # Exemplo de configuração
├── src/
│   ├── __init__.py
│   ├── agent.py             # Classe principal do agente
│   ├── collector.py         # Coleta de informações
│   ├── sender.py            # Envio de dados
│   └── scheduler.py         # Agendamento de tarefas
├── install/
│   ├── install_windows.ps1  # Instalador Windows
│   ├── install_linux.sh     # Instalador Linux
│   └── install_macos.sh     # Instalador macOS
└── main.py                  # Ponto de entrada
```

## Instalação

### Windows
```powershell
.\install\install_windows.ps1
```

### Linux
```bash
sudo bash install/install_linux.sh
```

### macOS
```bash
sudo bash install/install_macos.sh
```

## Configuração

Edite o arquivo `config/agent.yaml`:

```yaml
server:
  url: "https://seu-servidor.com/api"
  api_key: "sua-api-key"
  
agent:
  interval: 3600  # Intervalo em segundos (1 hora)
  client_id: 1    # ID do cliente no sistema
  
collection:
  hardware: true
  software: true
  network: true
```

## Uso Manual

```bash
# Executar coleta única
python main.py --once

# Executar em modo daemon
python main.py --daemon

# Testar conexão
python main.py --test
```

## Dados Coletados

### Hardware
- Processador (modelo, cores, frequência)
- Memória RAM (total, disponível)
- Discos (capacidade, uso, tipo)
- Placa-mãe (fabricante, modelo)
- Placas de rede (tipo, velocidade)

### Software
- Sistema operacional (nome, versão, build)
- Programas instalados
- Atualizações pendentes

### Rede
- Endereço IP (IPv4, IPv6)
- MAC Address
- Hostname
- Gateway padrão
- Servidores DNS

## API do Backend

O agente se comunica com os seguintes endpoints:

```
POST /api/agent/register      # Registrar novo agente
POST /api/agent/inventory     # Enviar inventário
GET  /api/agent/check-update  # Verificar atualização do agente
```

## Segurança

- ✅ Comunicação HTTPS obrigatória
- ✅ Autenticação por API Key
- ✅ Logs de auditoria
- ✅ Dados criptografados em trânsito

## Desenvolvimento

### Requisitos
- Python 3.8+
- pip

### Instalação para desenvolvimento
```bash
pip install -r requirements.txt
```

### Executar testes
```bash
pytest tests/
```

## Licença

Proprietário - SingleOne © 2025

