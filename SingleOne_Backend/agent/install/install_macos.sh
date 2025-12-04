#!/bin/bash
# SingleOne Agent - Instalador macOS
# Execute como root: sudo bash install_macos.sh

set -e

echo "=================================="
echo "SingleOne Agent - Instalador macOS"
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
    echo "Por favor, instale Python 3 de https://www.python.org/downloads/"
    echo "Ou use Homebrew: brew install python3"
    exit 1
fi

# Verificar se pip está instalado
if ! command -v pip3 &> /dev/null; then
    echo "Instalando pip..."
    python3 -m ensurepip --upgrade
fi

# Diretório de instalação
INSTALL_DIR="/usr/local/opt/singleone-agent"
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
chown -R root:wheel "$INSTALL_DIR"
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

# Criar LaunchDaemon
echo ""
echo "Criando LaunchDaemon..."

cat > /Library/LaunchDaemons/com.singleone.agent.plist << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.singleone.agent</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/bin/python3</string>
        <string>$INSTALL_DIR/main.py</string>
        <string>--daemon</string>
    </array>
    <key>WorkingDirectory</key>
    <string>$INSTALL_DIR</string>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <dict>
        <key>SuccessfulExit</key>
        <false/>
    </dict>
    <key>StandardOutPath</key>
    <string>$INSTALL_DIR/logs/stdout.log</string>
    <key>StandardErrorPath</key>
    <string>$INSTALL_DIR/logs/stderr.log</string>
</dict>
</plist>
EOF

# Definir permissões
chmod 644 /Library/LaunchDaemons/com.singleone.agent.plist
chown root:wheel /Library/LaunchDaemons/com.singleone.agent.plist

echo "✓ LaunchDaemon criado"

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
echo "2. Inicie o serviço: launchctl load /Library/LaunchDaemons/com.singleone.agent.plist"
echo "3. Verifique os logs: $INSTALL_DIR/logs/agent.log"
echo ""
echo "Comandos úteis:"
echo "  Iniciar:  launchctl load /Library/LaunchDaemons/com.singleone.agent.plist"
echo "  Parar:    launchctl unload /Library/LaunchDaemons/com.singleone.agent.plist"
echo "  Status:   launchctl list | grep singleone"
echo "  Logs:     tail -f $INSTALL_DIR/logs/agent.log"
echo "  Teste:    python3 $INSTALL_DIR/main.py --test"
echo ""

