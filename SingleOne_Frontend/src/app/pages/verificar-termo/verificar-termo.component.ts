import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TermosPublicosService, TermoValidacaoResponse } from '../../api/termos-publicos/termos-publicos.service';

@Component({
  selector: 'app-verificar-termo',
  templateUrl: './verificar-termo.component.html',
  styleUrls: ['./verificar-termo.component.scss']
})
export class VerificarTermoComponent implements OnInit {
  hash = '';
  loading = false;
  erro = '';
  resultado: TermoValidacaoResponse | null = null;

  constructor(private route: ActivatedRoute, private termosService: TermosPublicosService) {}

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      let h = params.get('hash') || '';
      // Sanitizar: remover dois-pontos iniciais (caso rota tenha sido chamada como ":hash") e espaços
      if (h.startsWith(':')) h = h.substring(1);
      this.hash = decodeURIComponent(h).trim();
      if (this.hash) this.validar();
    });
  }

  validar(): void {
    this.loading = true;
    this.erro = '';
    this.resultado = null;
    this.termosService.validar(this.hash).subscribe({
      next: (res) => {
        this.resultado = res;
        this.loading = false;
      },
      error: (err) => {
        this.erro = err?.error?.mensagem || 'Erro ao validar termo';
        this.loading = false;
      }
    });
  }
}

