import { Pipe, PipeTransform } from '@angular/core';
import * as moment from 'moment'

@Pipe({
  name: 'dateFormat'
})
export class DateFormatPipe implements PipeTransform {

  //*************** Formatações aceitas *******************/
  //1: dd/MM/yyyy HH:mm ***********************************/
  //2: dd/MM/yyyy *****************************************/
  //3: Ha x tempos a frente *******************************/
  //***************************************************** */

  transform(Data: string, formato:number): string {
    moment.locale('pt-BR');
    // var dataFim = moment(Data).fromNow();
    var dataFim = '';
    if(Data != null) {
      if(formato == 1)
        dataFim = moment(Data).format('DD/MM/YYYY HH:mm');
      else if(formato == 2)
        dataFim = moment(Data).format('DD/MM/YYYY');
      else if(formato == 3)
        // dataFim = moment(Data).endOf('day').fromNow()
        dataFim = moment(Data).fromNow();
    } 
    return dataFim;
  }

}
