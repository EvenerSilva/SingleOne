import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'formatDate'
})
export class FormatDatePipe implements PipeTransform {
  transform(value: string | Date, format: string = 'dd/MM/yyyy'): string {
    if (!value) return '';
    
    const date = typeof value === 'string' ? new Date(value) : value;
    
    if (isNaN(date.getTime())) return '';
    
    switch (format) {
      case 'dd/MM/yyyy':
        return date.toLocaleDateString('pt-BR');
      case 'dd/MM/yyyy HH:mm':
        return date.toLocaleDateString('pt-BR') + ' ' + date.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
      case 'yyyy-MM-dd':
        return date.toISOString().split('T')[0];
      default:
        return date.toLocaleDateString('pt-BR');
    }
  }
}
