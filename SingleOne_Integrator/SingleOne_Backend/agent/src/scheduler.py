"""
SingleOne Agent - Agendador de tarefas
"""

import time
import logging
import threading
from typing import Callable
from datetime import datetime


class AgentScheduler:
    """Agendador de tarefas periódicas para o agente"""
    
    def __init__(self, config: dict):
        """
        Inicializa o agendador
        
        Args:
            config: Configuração do agente
        """
        self.config = config
        self.logger = logging.getLogger(__name__)
        self.running = False
        self.tasks = []
        self.lock = threading.Lock()
    
    def schedule_task(self, func: Callable, interval: int):
        """
        Agenda uma tarefa para executar periodicamente
        
        Args:
            func: Função a ser executada
            interval: Intervalo em segundos
        """
        task = {
            'func': func,
            'interval': interval,
            'last_run': 0,
            'name': func.__name__
        }
        
        with self.lock:
            self.tasks.append(task)
        
        self.logger.info(f"Tarefa '{task['name']}' agendada para executar a cada {interval}s")
    
    def run(self):
        """Executa o loop principal do agendador"""
        self.running = True
        self.logger.info("Agendador iniciado")
        
        try:
            while self.running:
                current_time = time.time()
                
                with self.lock:
                    for task in self.tasks:
                        # Verificar se é hora de executar a tarefa
                        if current_time - task['last_run'] >= task['interval']:
                            self._execute_task(task, current_time)
                
                # Aguardar um pouco antes de verificar novamente
                time.sleep(10)
                
        except KeyboardInterrupt:
            self.logger.info("Agendador interrompido pelo usuário")
        except Exception as e:
            self.logger.error(f"Erro no loop do agendador: {e}", exc_info=True)
        finally:
            self.running = False
    
    def _execute_task(self, task: dict, current_time: float):
        """
        Executa uma tarefa agendada
        
        Args:
            task: Informações da tarefa
            current_time: Timestamp atual
        """
        task_name = task['name']
        self.logger.info(f"Executando tarefa '{task_name}'...")
        
        try:
            # Executar em thread separada para não bloquear o agendador
            thread = threading.Thread(target=task['func'], name=f"Task-{task_name}")
            thread.daemon = True
            thread.start()
            
            # Atualizar última execução
            task['last_run'] = current_time
            
            next_run = datetime.fromtimestamp(current_time + task['interval'])
            self.logger.info(f"Próxima execução de '{task_name}': {next_run}")
            
        except Exception as e:
            self.logger.error(f"Erro ao executar tarefa '{task_name}': {e}", exc_info=True)
    
    def shutdown(self):
        """Para o agendador"""
        self.logger.info("Parando agendador...")
        self.running = False
    
    def is_running(self) -> bool:
        """
        Verifica se o agendador está rodando
        
        Returns:
            True se rodando, False caso contrário
        """
        return self.running

