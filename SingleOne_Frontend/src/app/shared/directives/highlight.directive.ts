import { Directive, ElementRef, Input, OnInit } from '@angular/core';

@Directive({
  selector: '[appHighlight]'
})
export class HighlightDirective implements OnInit {
  @Input() appHighlight: string = '';
  @Input() highlightColor: string = 'yellow';

  constructor(private el: ElementRef) {}

  ngOnInit() {
    if (this.appHighlight) {
      this.highlightText(this.appHighlight);
    }
  }

  private highlightText(searchText: string) {
    if (!searchText) return;

    const text = this.el.nativeElement.textContent;
    const regex = new RegExp(`(${searchText})`, 'gi');
    const highlightedText = text.replace(regex, `<mark style="background-color: ${this.highlightColor}">$1</mark>`);
    
    this.el.nativeElement.innerHTML = highlightedText;
  }
}
