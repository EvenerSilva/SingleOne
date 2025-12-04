import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[appCpfMask]'
})
export class CpfMaskDirective {
  constructor(private el: ElementRef<HTMLInputElement>) {}

  private format(value: string): string {
    // Remover não dígitos e limitar a 11 dígitos
    let digits = value.replace(/\D/g, '').slice(0, 11);

    if (digits.length > 9) {
      return digits.replace(/^(\d{3})(\d{3})(\d{3})(\d{2}).*/, '$1.$2.$3-$4');
    } else if (digits.length > 6) {
      return digits.replace(/^(\d{3})(\d{3})(\d{0,3}).*/, '$1.$2.$3');
    } else if (digits.length > 3) {
      return digits.replace(/^(\d{3})(\d{0,3}).*/, '$1.$2');
    }
    return digits;
  }

  @HostListener('input', ['$event'])
  onInput(event: Event): void {
    const input = this.el.nativeElement;
    const formatted = this.format(input.value);
    input.value = formatted;
  }

  @HostListener('paste', ['$event'])
  onPaste(event: ClipboardEvent): void {
    // Aguardar colagem e aplicar máscara
    setTimeout(() => this.onInput(new Event('input')));
  }

  @HostListener('keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    const allowedKeys = [
      'Backspace', 'Delete', 'Tab', 'Escape', 'Enter', 'ArrowLeft', 'ArrowRight', 'Home', 'End'
    ];
    if (allowedKeys.indexOf(event.key) !== -1 || (event.ctrlKey || event.metaKey)) {
      return;
    }
    // Permitir apenas números
    if (!/\d/.test(event.key)) {
      event.preventDefault();
    }
  }
}

