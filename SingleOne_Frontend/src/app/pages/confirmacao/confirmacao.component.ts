import { Component, OnInit } from '@angular/core';
import { MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-confirmacao',
  templateUrl: './confirmacao.component.html',
  styleUrls: ['./confirmacao.component.scss']
})
export class ConfirmacaoComponent implements OnInit {

  public data:any = {};
  constructor(public dialogRef: MatDialogRef<ConfirmacaoComponent>,) { }

  ngOnInit(): void {
  }

  onNoClick() {

  }

}
