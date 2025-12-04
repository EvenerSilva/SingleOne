#!/usr/bin/env python3
"""
SingleOne Agent - Ponto de entrada principal
"""

import sys
import argparse
import os
from pathlib import Path

# Adicionar src ao path
sys.path.insert(0, str(Path(__file__).parent / "src"))

from src.agent import SingleOneAgent


def main():
    """Função principal"""
    parser = argparse.ArgumentParser(
        description='SingleOne Agent - Agente de Inventário de Ativos',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
Exemplos de uso:
  python main.py --once              # Executar coleta única
  python main.py --daemon            # Executar em modo daemon
  python main.py --test              # Testar conexão com servidor
  python main.py --config custom.yaml  # Usar arquivo de configuração customizado
        """
    )
    
    parser.add_argument(
        '--once',
        action='store_true',
        help='Executar coleta única e sair'
    )
    
    parser.add_argument(
        '--daemon',
        action='store_true',
        help='Executar em modo daemon (loop contínuo)'
    )
    
    parser.add_argument(
        '--test',
        action='store_true',
        help='Testar conexão com o servidor'
    )
    
    parser.add_argument(
        '--config',
        type=str,
        help='Caminho para arquivo de configuração'
    )
    
    parser.add_argument(
        '--version',
        action='version',
        version='SingleOne Agent 1.0.0'
    )
    
    args = parser.parse_args()
    
    # Verificar se nenhum argumento foi fornecido
    if not (args.once or args.daemon or args.test):
        parser.print_help()
        sys.exit(1)
    
    # Inicializar agente
    try:
        agent = SingleOneAgent(config_path=args.config)
    except Exception as e:
        print(f"ERRO: Falha ao inicializar agente: {e}")
        sys.exit(1)
    
    # Executar ação solicitada
    try:
        if args.test:
            # Testar conexão
            success = agent.test_connection()
            sys.exit(0 if success else 1)
        
        elif args.once:
            # Executar coleta única
            success = agent.run_once()
            sys.exit(0 if success else 1)
        
        elif args.daemon:
            # Executar em modo daemon
            agent.run_daemon()
            sys.exit(0)
            
    except KeyboardInterrupt:
        print("\nInterrompido pelo usuário")
        sys.exit(0)
    except Exception as e:
        print(f"ERRO: {e}")
        sys.exit(1)


if __name__ == '__main__':
    main()

