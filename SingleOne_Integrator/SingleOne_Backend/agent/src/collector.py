"""
SingleOne Agent - Coletor de informações do sistema
"""

import platform
import psutil
import socket
import logging
from typing import Dict, Any, List
from datetime import datetime


try:
    import cpuinfo
    HAS_CPUINFO = True
except ImportError:
    HAS_CPUINFO = False

try:
    import distro
    HAS_DISTRO = True
except ImportError:
    HAS_DISTRO = False

try:
    import netifaces
    HAS_NETIFACES = True
except ImportError:
    HAS_NETIFACES = False

# WMI apenas no Windows
if platform.system() == 'Windows':
    try:
        import wmi
        HAS_WMI = True
    except ImportError:
        HAS_WMI = False
else:
    HAS_WMI = False


class SystemCollector:
    """Coletor de informações do sistema"""
    
    def __init__(self, config: Dict[str, Any]):
        """
        Inicializa o coletor
        
        Args:
            config: Configuração do agente
        """
        self.config = config
        self.logger = logging.getLogger(__name__)
        self.platform = platform.system()
        
        # Inicializar WMI se disponível
        if HAS_WMI:
            try:
                self.wmi = wmi.WMI()
            except Exception as e:
                self.logger.warning(f"Erro ao inicializar WMI: {e}")
                self.wmi = None
        else:
            self.wmi = None
    
    def collect_hardware(self) -> Dict[str, Any]:
        """
        Coleta informações de hardware
        
        Returns:
            Dicionário com informações de hardware
        """
        hardware = {
            'cpu': self._collect_cpu(),
            'memory': self._collect_memory(),
            'disks': self._collect_disks(),
            'motherboard': self._collect_motherboard()
        }
        
        return hardware
    
    def _collect_cpu(self) -> Dict[str, Any]:
        """Coleta informações da CPU"""
        cpu_info = {
            'physical_cores': psutil.cpu_count(logical=False),
            'logical_cores': psutil.cpu_count(logical=True),
            'max_frequency': psutil.cpu_freq().max if psutil.cpu_freq() else 0,
            'current_frequency': psutil.cpu_freq().current if psutil.cpu_freq() else 0,
            'usage_percent': psutil.cpu_percent(interval=1)
        }
        
        # Informações detalhadas com py-cpuinfo
        if HAS_CPUINFO:
            try:
                cpu_detailed = cpuinfo.get_cpu_info()
                cpu_info.update({
                    'model': cpu_detailed.get('brand_raw', 'Unknown'),
                    'vendor': cpu_detailed.get('vendor_id_raw', 'Unknown'),
                    'architecture': cpu_detailed.get('arch', 'Unknown'),
                    'bits': cpu_detailed.get('bits', 0),
                    'flags': cpu_detailed.get('flags', [])
                })
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações detalhadas da CPU: {e}")
        
        # Informações via WMI (Windows)
        if HAS_WMI and self.wmi:
            try:
                for processor in self.wmi.Win32_Processor():
                    cpu_info.update({
                        'model': processor.Name.strip(),
                        'manufacturer': processor.Manufacturer.strip(),
                        'max_clock_speed': processor.MaxClockSpeed,
                        'l2_cache': processor.L2CacheSize,
                        'l3_cache': processor.L3CacheSize
                    })
                    break  # Usar apenas o primeiro processador
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações da CPU via WMI: {e}")
        
        return cpu_info
    
    def _collect_memory(self) -> Dict[str, Any]:
        """Coleta informações de memória"""
        mem = psutil.virtual_memory()
        swap = psutil.swap_memory()
        
        memory_info = {
            'total': mem.total,
            'available': mem.available,
            'used': mem.used,
            'percent': mem.percent,
            'swap_total': swap.total,
            'swap_used': swap.used,
            'swap_percent': swap.percent
        }
        
        # Informações detalhadas via WMI (Windows)
        if HAS_WMI and self.wmi:
            try:
                memory_modules = []
                for mem_module in self.wmi.Win32_PhysicalMemory():
                    memory_modules.append({
                        'capacity': int(mem_module.Capacity) if mem_module.Capacity else 0,
                        'speed': mem_module.Speed,
                        'manufacturer': mem_module.Manufacturer.strip() if mem_module.Manufacturer else 'Unknown',
                        'part_number': mem_module.PartNumber.strip() if mem_module.PartNumber else 'Unknown',
                        'serial_number': mem_module.SerialNumber.strip() if mem_module.SerialNumber else 'Unknown'
                    })
                memory_info['modules'] = memory_modules
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações de memória via WMI: {e}")
        
        return memory_info
    
    def _collect_disks(self) -> List[Dict[str, Any]]:
        """Coleta informações de discos"""
        disks = []
        
        # Partições
        for partition in psutil.disk_partitions():
            try:
                usage = psutil.disk_usage(partition.mountpoint)
                disk_info = {
                    'device': partition.device,
                    'mountpoint': partition.mountpoint,
                    'filesystem': partition.fstype,
                    'total': usage.total,
                    'used': usage.used,
                    'free': usage.free,
                    'percent': usage.percent
                }
                disks.append(disk_info)
            except PermissionError:
                # Ignorar discos sem permissão de acesso
                continue
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações do disco {partition.device}: {e}")
        
        # Informações físicas via WMI (Windows)
        if HAS_WMI and self.wmi:
            try:
                physical_disks = []
                for disk in self.wmi.Win32_DiskDrive():
                    physical_disks.append({
                        'model': disk.Model.strip() if disk.Model else 'Unknown',
                        'interface': disk.InterfaceType,
                        'size': int(disk.Size) if disk.Size else 0,
                        'serial_number': disk.SerialNumber.strip() if disk.SerialNumber else 'Unknown',
                        'media_type': disk.MediaType
                    })
                
                # Adicionar informações físicas aos discos lógicos
                if disks:
                    disks[0]['physical_disks'] = physical_disks
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações físicas de discos via WMI: {e}")
        
        return disks
    
    def _collect_motherboard(self) -> Dict[str, Any]:
        """Coleta informações da placa-mãe"""
        motherboard = {
            'manufacturer': 'Unknown',
            'model': 'Unknown',
            'serial_number': 'Unknown',
            'version': 'Unknown'
        }
        
        # Informações via WMI (Windows)
        if HAS_WMI and self.wmi:
            try:
                for board in self.wmi.Win32_BaseBoard():
                    motherboard.update({
                        'manufacturer': board.Manufacturer.strip() if board.Manufacturer else 'Unknown',
                        'model': board.Product.strip() if board.Product else 'Unknown',
                        'serial_number': board.SerialNumber.strip() if board.SerialNumber else 'Unknown',
                        'version': board.Version.strip() if board.Version else 'Unknown'
                    })
                    break
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações da placa-mãe via WMI: {e}")
        
        # Informações via DMI (Linux)
        elif self.platform == 'Linux':
            try:
                import subprocess
                result = subprocess.run(['dmidecode', '-t', 'baseboard'], 
                                        capture_output=True, text=True, timeout=5)
                if result.returncode == 0:
                    for line in result.stdout.split('\n'):
                        if 'Manufacturer:' in line:
                            motherboard['manufacturer'] = line.split(':', 1)[1].strip()
                        elif 'Product Name:' in line:
                            motherboard['model'] = line.split(':', 1)[1].strip()
                        elif 'Serial Number:' in line:
                            motherboard['serial_number'] = line.split(':', 1)[1].strip()
                        elif 'Version:' in line:
                            motherboard['version'] = line.split(':', 1)[1].strip()
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações da placa-mãe via DMI: {e}")
        
        return motherboard
    
    def collect_software(self) -> Dict[str, Any]:
        """
        Coleta informações de software
        
        Returns:
            Dicionário com informações de software
        """
        software = {
            'os': self._collect_os_info(),
            'installed_programs': self._collect_installed_programs()
        }
        
        return software
    
    def _collect_os_info(self) -> Dict[str, Any]:
        """Coleta informações do sistema operacional"""
        os_info = {
            'system': platform.system(),
            'release': platform.release(),
            'version': platform.version(),
            'machine': platform.machine(),
            'processor': platform.processor(),
            'hostname': socket.gethostname(),
            'boot_time': datetime.fromtimestamp(psutil.boot_time()).isoformat()
        }
        
        # Informações detalhadas do Linux
        if HAS_DISTRO and self.platform == 'Linux':
            try:
                os_info.update({
                    'distribution': distro.name(),
                    'distribution_version': distro.version(),
                    'distribution_codename': distro.codename()
                })
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações de distribuição Linux: {e}")
        
        # Informações via WMI (Windows)
        if HAS_WMI and self.wmi:
            try:
                for os_item in self.wmi.Win32_OperatingSystem():
                    os_info.update({
                        'name': os_item.Caption.strip(),
                        'version': os_item.Version,
                        'build_number': os_item.BuildNumber,
                        'serial_number': os_item.SerialNumber,
                        'install_date': os_item.InstallDate,
                        'registered_user': os_item.RegisteredUser
                    })
                    break
            except Exception as e:
                self.logger.warning(f"Erro ao obter informações do SO via WMI: {e}")
        
        return os_info
    
    def _collect_installed_programs(self) -> List[Dict[str, Any]]:
        """Coleta lista de programas instalados"""
        programs = []
        
        try:
            # Windows - via WMI
            if HAS_WMI and self.wmi:
                for product in self.wmi.Win32_Product():
                    programs.append({
                        'name': product.Name.strip() if product.Name else 'Unknown',
                        'version': product.Version if product.Version else 'Unknown',
                        'vendor': product.Vendor.strip() if product.Vendor else 'Unknown',
                        'install_date': product.InstallDate if product.InstallDate else 'Unknown'
                    })
            
            # Linux - via dpkg ou rpm
            elif self.platform == 'Linux':
                import subprocess
                
                # Tentar dpkg (Debian/Ubuntu)
                try:
                    result = subprocess.run(['dpkg', '-l'], 
                                            capture_output=True, text=True, timeout=30)
                    if result.returncode == 0:
                        for line in result.stdout.split('\n'):
                            if line.startswith('ii'):
                                parts = line.split()
                                if len(parts) >= 3:
                                    programs.append({
                                        'name': parts[1],
                                        'version': parts[2],
                                        'vendor': 'Unknown'
                                    })
                except FileNotFoundError:
                    pass
                
                # Tentar rpm (RedHat/CentOS/Fedora)
                if not programs:
                    try:
                        result = subprocess.run(['rpm', '-qa'], 
                                                capture_output=True, text=True, timeout=30)
                        if result.returncode == 0:
                            for line in result.stdout.split('\n'):
                                if line.strip():
                                    programs.append({
                                        'name': line.strip(),
                                        'version': 'Unknown',
                                        'vendor': 'Unknown'
                                    })
                    except FileNotFoundError:
                        pass
            
            # macOS - via system_profiler
            elif self.platform == 'Darwin':
                import subprocess
                try:
                    result = subprocess.run(['system_profiler', 'SPApplicationsDataType', '-xml'], 
                                            capture_output=True, text=True, timeout=30)
                    # Parsear XML aqui se necessário
                    # Por simplicidade, não implementado nesta versão
                except Exception as e:
                    self.logger.warning(f"Erro ao obter programas instalados no macOS: {e}")
        
        except Exception as e:
            self.logger.error(f"Erro ao coletar programas instalados: {e}", exc_info=True)
        
        return programs
    
    def collect_network(self) -> Dict[str, Any]:
        """
        Coleta informações de rede
        
        Returns:
            Dicionário com informações de rede
        """
        network = {
            'hostname': socket.gethostname(),
            'fqdn': socket.getfqdn(),
            'interfaces': self._collect_network_interfaces()
        }
        
        # Adicionar gateway e DNS se disponível
        if HAS_NETIFACES:
            try:
                gateways = netifaces.gateways()
                network['default_gateway'] = gateways.get('default', {}).get(netifaces.AF_INET, ['Unknown'])[0]
            except Exception as e:
                self.logger.warning(f"Erro ao obter gateway padrão: {e}")
        
        return network
    
    def _collect_network_interfaces(self) -> List[Dict[str, Any]]:
        """Coleta informações das interfaces de rede"""
        interfaces = []
        
        net_if_addrs = psutil.net_if_addrs()
        net_if_stats = psutil.net_if_stats()
        
        for interface_name, addresses in net_if_addrs.items():
            interface_info = {
                'name': interface_name,
                'addresses': []
            }
            
            # Status da interface
            if interface_name in net_if_stats:
                stats = net_if_stats[interface_name]
                interface_info['is_up'] = stats.isup
                interface_info['speed'] = stats.speed
                interface_info['mtu'] = stats.mtu
            
            # Endereços
            for addr in addresses:
                addr_info = {
                    'family': str(addr.family),
                    'address': addr.address
                }
                
                if addr.netmask:
                    addr_info['netmask'] = addr.netmask
                if addr.broadcast:
                    addr_info['broadcast'] = addr.broadcast
                
                interface_info['addresses'].append(addr_info)
            
            interfaces.append(interface_info)
        
        return interfaces
    
    def collect_processes(self, limit: int = 50) -> List[Dict[str, Any]]:
        """
        Coleta lista de processos em execução
        
        Args:
            limit: Número máximo de processos a retornar
            
        Returns:
            Lista de processos
        """
        processes = []
        
        try:
            for proc in psutil.process_iter(['pid', 'name', 'username', 'cpu_percent', 'memory_percent']):
                try:
                    processes.append(proc.info)
                except (psutil.NoSuchProcess, psutil.AccessDenied):
                    pass
            
            # Ordenar por uso de CPU e limitar
            processes.sort(key=lambda x: x.get('cpu_percent', 0), reverse=True)
            processes = processes[:limit]
            
        except Exception as e:
            self.logger.error(f"Erro ao coletar processos: {e}", exc_info=True)
        
        return processes
    
    def collect_users(self) -> List[Dict[str, Any]]:
        """
        Coleta lista de usuários logados
        
        Returns:
            Lista de usuários
        """
        users = []
        
        try:
            for user in psutil.users():
                users.append({
                    'name': user.name,
                    'terminal': user.terminal,
                    'host': user.host,
                    'started': datetime.fromtimestamp(user.started).isoformat()
                })
        except Exception as e:
            self.logger.error(f"Erro ao coletar usuários: {e}", exc_info=True)
        
        return users

