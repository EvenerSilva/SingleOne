#!/bin/bash
# SingleOne Agent - Instalador Linux
# Execute como root: sudo bash install_linux.sh

set -e

echo "=================================="
echo "SingleOne Agent - Instalador Linux"
echo "=================================="
echo ""

# Verificar se está rodando como root
if [ "$EUID" -ne 0 ]; then 
    echo "ERRO: Este script deve ser executado como root (sudo)"
    exit 1
fi

# Verificar se Python está instalado
echo "Verificando Python..."
if command -v python3 &> /dev/null; then
    PYTHON_VERSION=$(python3 --version)
    echo "✓ Python encontrado: $PYTHON_VERSION"
else
    echo "✗ Python 3 não encontrado!"
    echo "Instalando Python 3..."
    
    if command -v apt-get &> /dev/null; then
        apt-get update
        apt-get install -y python3 python3-pip
    elif command -v yum &> /dev/null; then
        yum install -y python3 python3-pip
    elif command -v dnf &> /dev/null; then
        dnf install -y python3 python3-pip
    else
        echo "Gerenciador de pacotes não suportado. Instale Python 3 manualmente."
        exit 1
    fi
fi

# Verificar se pip está instalado
if ! command -v pip3 &> /dev/null; then
    echo "Instalando pip..."
    python3 -m ensurepip --upgrade
fi

# Diretório de instalação
INSTALL_DIR="/opt/singleone-agent"
echo ""
echo "Instalando em: $INSTALL_DIR"

# Criar diretório
mkdir -p "$INSTALL_DIR"
echo "✓ Diretório criado"

# Copiar arquivos
echo ""
echo "Copiando arquivos..."
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && cd .. && pwd )"
cp -r "$SCRIPT_DIR"/* "$INSTALL_DIR/" 2>/dev/null || true
echo "✓ Arquivos copiados"

# Criar diretório de logs
mkdir -p "$INSTALL_DIR/logs"

# Definir permissões
chown -R root:root "$INSTALL_DIR"
chmod -R 755 "$INSTALL_DIR"

# Instalar dependências Python
echo ""
echo "Instalando dependências Python..."
cd "$INSTALL_DIR"
python3 -m pip install --upgrade pip --quiet
python3 -m pip install -r requirements.txt --quiet
echo "✓ Dependências instaladas"

# Verificar se arquivo de configuração existe
CONFIG_FILE="$INSTALL_DIR/config/agent.yaml"
if [ ! -f "$CONFIG_FILE" ]; then
    echo ""
    echo "ATENÇÃO: Arquivo de configuração não encontrado!"
    echo "Copiando arquivo de exemplo..."
    cp "$INSTALL_DIR/config/agent.example.yaml" "$CONFIG_FILE"
    echo "✓ Arquivo de configuração criado: $CONFIG_FILE"
    echo ""
    echo "IMPORTANTE: Edite o arquivo de configuração antes de iniciar o agente!"
    echo "Configure a URL do servidor e a API Key em: $CONFIG_FILE"
fi

# Criar serviço systemd
echo ""
echo "Criando serviço systemd..."

cat > /etc/systemd/system/singleone-agent.service << EOF
[Unit]
Description=SingleOne Agent - Agente de Inventário de Ativos
After=network.target

[Service]
Type=simple
User=root
WorkingDirectory=$INSTALL_DIR
ExecStart=/usr/bin/python3 $INSTALL_DIR/main.py --daemon
Restart=always
RestartSec=60
StandardOutput=journal
StandardError=journal

[Install]
WantedBy=multi-user.target
EOF

# Recarregar systemd
systemctl daemon-reload
systemctl enable singleone-agent.service
echo "✓ Serviço criado e habilitado"

# Testar conexão
echo ""
echo "Testando conexão com o servidor..."
if python3 "$INSTALL_DIR/main.py" --test; then
    echo "✓ Conexão bem-sucedida!"
else
    echo "✗ Falha na conexão"
    echo "Verifique a configuração em: $CONFIG_FILE"
fi

# Resumo
echo ""
echo "=================================="
echo "Instalação Concluída!"
echo "=================================="
echo ""
echo "Próximos passos:"
echo "1. Edite o arquivo de configuração: $CONFIG_FILE"
echo "2. Inicie o serviço: systemctl start singleone-agent"
echo "3. Verifique os logs: journalctl -u singleone-agent -f"
echo ""
echo "Comandos úteis:"
echo "  Iniciar:  systemctl start singleone-agent"
echo "  Parar:    systemctl stop singleone-agent"
echo "  Status:   systemctl status singleone-agent"
echo "  Logs:     journalctl -u singleone-agent -f"
echo "  Teste:    python3 $INSTALL_DIR/main.py --test"
echo ""

