"""
SingleOne Agent - Classe principal do agente de inventário
"""

import os
import sys
import time
import yaml
import logging
import socket
from pathlib import Path
from typing import Dict, Any, Optional

from .collector import SystemCollector
from .sender import DataSender
from .scheduler import AgentScheduler


class SingleOneAgent:
    """Agente principal de inventário da SingleOne"""
    
    def __init__(self, config_path: str = None):
        """
        Inicializa o agente
        
        Args:
            config_path: Caminho para o arquivo de configuração
        """
        self.config_path = config_path or self._get_default_config_path()
        self.config = self._load_config()
        self._setup_logging()
        
        self.logger = logging.getLogger(__name__)
        self.hostname = socket.gethostname()
        
        # Componentes do agente
        self.collector = SystemCollector(self.config)
        self.sender = DataSender(self.config)
        self.scheduler = AgentScheduler(self.config)
        
        self.logger.info(f"SingleOne Agent inicializado - Hostname: {self.hostname}")
    
    def _get_default_config_path(self) -> str:
        """Retorna o caminho padrão do arquivo de configuração"""
        base_dir = Path(__file__).parent.parent
        return str(base_dir / "config" / "agent.yaml")
    
    def _load_config(self) -> Dict[str, Any]:
        """Carrega a configuração do arquivo YAML"""
        try:
            with open(self.config_path, 'r', encoding='utf-8') as f:
                config = yaml.safe_load(f)
            return config
        except FileNotFoundError:
            print(f"ERRO: Arquivo de configuração não encontrado: {self.config_path}")
            print("Use o arquivo config/agent.example.yaml como base.")
            sys.exit(1)
        except yaml.YAMLError as e:
            print(f"ERRO: Erro ao ler arquivo de configuração: {e}")
            sys.exit(1)
    
    def _setup_logging(self):
        """Configura o sistema de logging"""
        log_config = self.config.get('logging', {})
        log_level = getattr(logging, log_config.get('level', 'INFO').upper())
        log_file = log_config.get('file', 'logs/agent.log')
        
        # Criar diretório de logs se não existir
        log_dir = Path(log_file).parent
        log_dir.mkdir(parents=True, exist_ok=True)
        
        # Configurar logging
        logging.basicConfig(
            level=log_level,
            format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
            handlers=[
                logging.FileHandler(log_file, encoding='utf-8'),
                logging.StreamHandler(sys.stdout)
            ]
        )
    
    def collect_inventory(self) -> Optional[Dict[str, Any]]:
        """
        Coleta inventário completo do sistema
        
        Returns:
            Dicionário com os dados coletados ou None em caso de erro
        """
        self.logger.info("Iniciando coleta de inventário...")
        
        try:
            inventory = {
                'agent': {
                    'version': '1.0.0',
                    'hostname': self.hostname,
                    'client_id': self.config['agent']['client_id'],
                    'timestamp': time.time()
                }
            }
            
            collection_config = self.config.get('collection', {})
            
            # Coletar hardware
            if collection_config.get('hardware', True):
                self.logger.info("Coletando informações de hardware...")
                inventory['hardware'] = self.collector.collect_hardware()
            
            # Coletar software
            if collection_config.get('software', True):
                self.logger.info("Coletando informações de software...")
                inventory['software'] = self.collector.collect_software()
            
            # Coletar rede
            if collection_config.get('network', True):
                self.logger.info("Coletando informações de rede...")
                inventory['network'] = self.collector.collect_network()
            
            # Coletar processos (opcional)
            if collection_config.get('processes', False):
                self.logger.info("Coletando processos...")
                inventory['processes'] = self.collector.collect_processes()
            
            # Coletar usuários (opcional)
            if collection_config.get('users', False):
                self.logger.info("Coletando usuários...")
                inventory['users'] = self.collector.collect_users()
            
            self.logger.info("Coleta de inventário concluída com sucesso")
            return inventory
            
        except Exception as e:
            self.logger.error(f"Erro ao coletar inventário: {e}", exc_info=True)
            return None
    
    def send_inventory(self, inventory: Dict[str, Any]) -> bool:
        """
        Envia inventário para o servidor
        
        Args:
            inventory: Dados do inventário
            
        Returns:
            True se enviado com sucesso, False caso contrário
        """
        self.logger.info("Enviando inventário para o servidor...")
        
        try:
            success = self.sender.send_inventory(inventory)
            
            if success:
                self.logger.info("Inventário enviado com sucesso")
            else:
                self.logger.error("Falha ao enviar inventário")
            
            return success
            
        except Exception as e:
            self.logger.error(f"Erro ao enviar inventário: {e}", exc_info=True)
            return False
    
    def run_once(self) -> bool:
        """
        Executa uma coleta única
        
        Returns:
            True se executado com sucesso, False caso contrário
        """
        self.logger.info("=== Executando coleta única ===")
        
        # Coletar
        inventory = self.collect_inventory()
        if not inventory:
            return False
        
        # Enviar
        success = self.send_inventory(inventory)
        
        self.logger.info("=== Coleta única finalizada ===")
        return success
    
    def run_daemon(self):
        """Executa o agente em modo daemon (loop contínuo)"""
        self.logger.info("=== Iniciando agente em modo daemon ===")
        
        # Executar na inicialização se configurado
        if self.config['agent'].get('run_on_startup', True):
            self.run_once()
        
        # Agendar execução periódica
        interval = self.config['agent'].get('interval', 3600)
        self.scheduler.schedule_task(self.run_once, interval)
        
        # Loop principal
        try:
            self.logger.info(f"Agente agendado para executar a cada {interval} segundos")
            self.scheduler.run()
        except KeyboardInterrupt:
            self.logger.info("Agente interrompido pelo usuário")
        except Exception as e:
            self.logger.error(f"Erro no loop principal: {e}", exc_info=True)
        finally:
            self.shutdown()
    
    def test_connection(self) -> bool:
        """
        Testa a conexão com o servidor
        
        Returns:
            True se conectado com sucesso, False caso contrário
        """
        self.logger.info("Testando conexão com o servidor...")
        
        success = self.sender.test_connection()
        
        if success:
            self.logger.info("✓ Conexão estabelecida com sucesso!")
        else:
            self.logger.error("✗ Falha ao conectar com o servidor")
        
        return success
    
    def shutdown(self):
        """Encerra o agente graciosamente"""
        self.logger.info("Encerrando agente...")
        self.scheduler.shutdown()
        self.logger.info("Agente encerrado")

