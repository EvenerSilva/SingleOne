"""
SingleOne Agent - Enviador de dados para o servidor
"""

import requests
import logging
import time
from typing import Dict, Any, Optional
from urllib.parse import urljoin


class DataSender:
    """Responsável por enviar dados para o servidor SingleOne"""
    
    def __init__(self, config: Dict[str, Any]):
        """
        Inicializa o enviador
        
        Args:
            config: Configuração do agente
        """
        self.config = config
        self.logger = logging.getLogger(__name__)
        
        # Configurações do servidor
        server_config = config.get('server', {})
        self.base_url = server_config.get('url', 'http://localhost:5000/api')
        self.api_key = server_config.get('api_key', '')
        self.timeout = server_config.get('timeout', 30)
        self.retry_attempts = server_config.get('retry_attempts', 3)
        self.retry_delay = server_config.get('retry_delay', 60)
        
        # Validar configuração
        if not self.api_key:
            self.logger.warning("API Key não configurada! O servidor pode rejeitar as requisições.")
        
        # Session para reutilizar conexões
        self.session = requests.Session()
        self.session.headers.update({
            'User-Agent': 'SingleOne-Agent/1.0.0',
            'Content-Type': 'application/json',
            'X-API-Key': self.api_key
        })
    
    def _make_request(self, method: str, endpoint: str, data: Optional[Dict] = None, 
                      retry: bool = True) -> Optional[requests.Response]:
        """
        Faz uma requisição HTTP para o servidor
        
        Args:
            method: Método HTTP (GET, POST, etc.)
            endpoint: Endpoint da API
            data: Dados a enviar (para POST/PUT)
            retry: Se deve tentar novamente em caso de falha
            
        Returns:
            Response object ou None em caso de falha
        """
        url = urljoin(self.base_url, endpoint)
        attempts = self.retry_attempts if retry else 1
        
        for attempt in range(1, attempts + 1):
            try:
                self.logger.debug(f"Requisição {method} para {url} (tentativa {attempt}/{attempts})")
                
                if method.upper() == 'GET':
                    response = self.session.get(url, timeout=self.timeout)
                elif method.upper() == 'POST':
                    response = self.session.post(url, json=data, timeout=self.timeout)
                elif method.upper() == 'PUT':
                    response = self.session.put(url, json=data, timeout=self.timeout)
                else:
                    self.logger.error(f"Método HTTP não suportado: {method}")
                    return None
                
                # Verificar status
                if response.status_code == 200 or response.status_code == 201:
                    self.logger.debug(f"Requisição bem-sucedida: {response.status_code}")
                    return response
                elif response.status_code == 401:
                    self.logger.error("Erro de autenticação (401). Verifique a API Key.")
                    return None  # Não tentar novamente em caso de erro de autenticação
                elif response.status_code == 403:
                    self.logger.error("Acesso negado (403). Verifique as permissões.")
                    return None
                elif response.status_code >= 500:
                    self.logger.warning(f"Erro do servidor ({response.status_code}). Tentando novamente...")
                else:
                    self.logger.warning(f"Requisição falhou com status {response.status_code}: {response.text}")
                    return None
                    
            except requests.exceptions.Timeout:
                self.logger.warning(f"Timeout na requisição (tentativa {attempt}/{attempts})")
            except requests.exceptions.ConnectionError as e:
                self.logger.warning(f"Erro de conexão (tentativa {attempt}/{attempts}): {e}")
            except requests.exceptions.RequestException as e:
                self.logger.error(f"Erro na requisição: {e}", exc_info=True)
                return None
            except Exception as e:
                self.logger.error(f"Erro inesperado: {e}", exc_info=True)
                return None
            
            # Aguardar antes de tentar novamente
            if attempt < attempts:
                self.logger.info(f"Aguardando {self.retry_delay}s antes da próxima tentativa...")
                time.sleep(self.retry_delay)
        
        self.logger.error(f"Falha após {attempts} tentativas")
        return None
    
    def test_connection(self) -> bool:
        """
        Testa a conexão com o servidor
        
        Returns:
            True se conectado com sucesso, False caso contrário
        """
        self.logger.info("Testando conexão com o servidor...")
        
        try:
            # Tentar um endpoint de health check ou similar
            response = self._make_request('GET', '/health', retry=False)
            
            if response and response.status_code == 200:
                self.logger.info("✓ Servidor acessível")
                return True
            else:
                # Se não existir endpoint de health, tentar o registro
                self.logger.info("Tentando endpoint alternativo...")
                response = self._make_request('POST', '/agent/test', 
                                               data={'test': True}, retry=False)
                if response:
                    return True
                    
        except Exception as e:
            self.logger.error(f"Erro ao testar conexão: {e}", exc_info=True)
        
        return False
    
    def send_inventory(self, inventory: Dict[str, Any]) -> bool:
        """
        Envia inventário para o servidor
        
        Args:
            inventory: Dados do inventário coletado
            
        Returns:
            True se enviado com sucesso, False caso contrário
        """
        self.logger.info("Enviando inventário para o servidor...")
        
        try:
            # Adicionar metadados
            payload = {
                'timestamp': time.time(),
                'agent_version': '1.0.0',
                'client_id': self.config['agent']['client_id'],
                'data': inventory
            }
            
            # Enviar para o servidor
            response = self._make_request('POST', '/agent/inventory', data=payload)
            
            if response:
                try:
                    result = response.json()
                    self.logger.info(f"Inventário enviado com sucesso: {result}")
                    return True
                except Exception:
                    self.logger.info("Inventário enviado com sucesso")
                    return True
            else:
                self.logger.error("Falha ao enviar inventário")
                return False
                
        except Exception as e:
            self.logger.error(f"Erro ao enviar inventário: {e}", exc_info=True)
            return False
    
    def register_agent(self) -> bool:
        """
        Registra o agente no servidor
        
        Returns:
            True se registrado com sucesso, False caso contrário
        """
        self.logger.info("Registrando agente no servidor...")
        
        try:
            import socket
            
            payload = {
                'hostname': socket.gethostname(),
                'client_id': self.config['agent']['client_id'],
                'agent_version': '1.0.0',
                'platform': __import__('platform').system()
            }
            
            response = self._make_request('POST', '/agent/register', data=payload)
            
            if response:
                try:
                    result = response.json()
                    self.logger.info(f"Agente registrado com sucesso: {result}")
                    return True
                except Exception:
                    self.logger.info("Agente registrado com sucesso")
                    return True
            else:
                self.logger.error("Falha ao registrar agente")
                return False
                
        except Exception as e:
            self.logger.error(f"Erro ao registrar agente: {e}", exc_info=True)
            return False
    
    def check_updates(self) -> Optional[Dict[str, Any]]:
        """
        Verifica se há atualizações disponíveis para o agente
        
        Returns:
            Informações sobre atualização disponível ou None
        """
        self.logger.info("Verificando atualizações...")
        
        try:
            response = self._make_request('GET', '/agent/check-update', retry=False)
            
            if response:
                try:
                    result = response.json()
                    if result.get('update_available'):
                        self.logger.info(f"Atualização disponível: versão {result.get('latest_version')}")
                        return result
                    else:
                        self.logger.info("Agente está atualizado")
                        return None
                except Exception as e:
                    self.logger.warning(f"Erro ao parsear resposta de atualização: {e}")
            
        except Exception as e:
            self.logger.error(f"Erro ao verificar atualizações: {e}", exc_info=True)
        
        return None
    
    def send_heartbeat(self) -> bool:
        """
        Envia um heartbeat para indicar que o agente está ativo
        
        Returns:
            True se enviado com sucesso, False caso contrário
        """
        try:
            import socket
            
            payload = {
                'hostname': socket.gethostname(),
                'timestamp': time.time()
            }
            
            response = self._make_request('POST', '/agent/heartbeat', 
                                           data=payload, retry=False)
            return response is not None
            
        except Exception as e:
            self.logger.warning(f"Erro ao enviar heartbeat: {e}")
            return False
    
    def close(self):
        """Fecha a sessão HTTP"""
        try:
            self.session.close()
        except Exception as e:
            self.logger.warning(f"Erro ao fechar sessão: {e}")

