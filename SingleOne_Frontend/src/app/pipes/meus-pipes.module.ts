import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DateFormatPipe } from './date-format.pipe';
import { CurrencyFormatPipe } from './currency-format.pipe';
import { CnpjFormatPipe } from './cpnj-format.pipe';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [DateFormatPipe, CurrencyFormatPipe, CnpjFormatPipe],
  exports: [DateFormatPipe, CurrencyFormatPipe, CnpjFormatPipe]
})
export class MeusPipesModule { }