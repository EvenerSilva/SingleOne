import { Directive, ElementRef, HostListener } from '@angular/core';

@Directive({
  selector: '[appCnpjMask]'
})
export class CnpjMaskDirective {
  constructor(private el: ElementRef) {}

  @HostListener('input', ['$event'])
  onInputChange(event: Event): void {
    const input = this.el.nativeElement;
    let value = input.value.replace(/\D/g, '');

    if (value.length > 14) {
      value = value.slice(0, 14);
    }

    if (value.length > 12) {
      input.value = value.replace(/^(\d{2})(\d{3})(\d{3})(\d{4})(\d{2}).*/, '$1.$2.$3/$4-$5');
    } else if (value.length > 8) {
      input.value = value.replace(/^(\d{2})(\d{3})(\d{3})(\d{4}).*/, '$1.$2.$3/$4');
    } else if (value.length > 5) {
      input.value = value.replace(/^(\d{2})(\d{3})(\d{3}).*/, '$1.$2.$3');
    } else if (value.length > 2) {
      input.value = value.replace(/^(\d{2})(\d{3}).*/, '$1.$2');
    } else {
      input.value = value;
    }
  }
}
