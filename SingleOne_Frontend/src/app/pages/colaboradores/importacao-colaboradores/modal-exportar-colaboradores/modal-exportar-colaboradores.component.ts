import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { UtilService } from 'src/app/util/util.service';
import { ColaboradorApiService } from 'src/app/api/colaboradores/colaborador-api.service';
import * as XLSX from 'xlsx';

@Component({
  selector: 'app-modal-exportar-colaboradores',
  templateUrl: './modal-exportar-colaboradores.component.html',
  styleUrls: ['./modal-exportar-colaboradores.component.scss']
})
export class ModalExportarColaboradoresComponent {
  cliente: number;
  session: any;

  constructor(
    public dialogRef: MatDialogRef<ModalExportarColaboradoresComponent>,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private util: UtilService,
    private colaboradorApi: ColaboradorApiService
  ) {
    this.cliente = data?.cliente || 0;
    this.session = data?.session || this.util.getSession('usuario');
  }

  fechar(): void {
    this.dialogRef.close();
  }

  async exportarExcel(): Promise<void> {
    try {
      this.util.aguardar(true);
      const dados = await this.carregarTodosColaboradores();

      if (dados.length === 0) {
        this.util.exibirMensagemToast('⚠️ Nenhum dado disponível para exportar', 3000);
        return;
      }

      const dadosExportacao = this.prepararDadosColaboradores(dados);

      const wb = XLSX.utils.book_new();
      const ws = XLSX.utils.json_to_sheet(dadosExportacao);
      XLSX.utils.book_append_sheet(wb, ws, 'Colaboradores');
      XLSX.writeFile(wb, `colaboradores_${new Date().toISOString().slice(0, 10)}.xlsx`);

      this.util.exibirMensagemToast('✅ Exportação Excel concluída com sucesso!', 3000);
      this.fechar();
    } catch (error) {
      console.error('[EXPORTAÇÃO] Erro na exportação Excel:', error);
      this.util.exibirMensagemToast('❌ Erro na exportação Excel', 3000);
    } finally {
      this.util.aguardar(false);
    }
  }

  async exportarCSV(): Promise<void> {
    try {
      this.util.aguardar(true);
      const dados = await this.carregarTodosColaboradores();

      if (dados.length === 0) {
        this.util.exibirMensagemToast('⚠️ Nenhum dado disponível para exportar', 3000);
        return;
      }

      const dadosExportacao = this.prepararDadosColaboradores(dados);

      const csv = this.converterParaCSV(dadosExportacao);
      const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
      const link = document.createElement('a');
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `colaboradores_${new Date().toISOString().slice(0, 10)}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);

      this.util.exibirMensagemToast('✅ Exportação CSV concluída com sucesso!', 3000);
      this.fechar();
    } catch (error) {
      console.error('[EXPORTAÇÃO] Erro na exportação CSV:', error);
      this.util.exibirMensagemToast('❌ Erro na exportação CSV', 3000);
    } finally {
      this.util.aguardar(false);
    }
  }

  private async carregarTodosColaboradores(): Promise<any[]> {
    const token = this.session?.token;
    if (!token) {
      this.util.exibirMensagemToast('Sessão expirada. Faça login novamente.', 3000);
      return [];
    }

    let paginaAtual = 1;
    let todosColaboradores: any[] = [];
    let continuarBuscando = true;

    while (continuarBuscando) {
      try {
        const res = await this.colaboradorApi.listarColaboradores("null", this.cliente, paginaAtual, token);

        if (res.status === 200 || res.status === 204) {
          const resultados = res.data.results || [];

          if (resultados.length > 0) {
            todosColaboradores = todosColaboradores.concat(resultados);
            paginaAtual++;

            if (resultados.length < 10) {
              continuarBuscando = false;
            }
          } else {
            continuarBuscando = false;
          }
        } else {
          continuarBuscando = false;
        }
      } catch (error) {
        console.error('[EXPORTAÇÃO] Erro ao buscar colaboradores para exportação', error);
        continuarBuscando = false;
      }
    }

    return todosColaboradores;
  }

  private prepararDadosColaboradores(dados: any[]): any[] {
    return dados.map(col => ({
      'Matrícula': col.matricula || 'N/A',
      'Nome': col.nome || 'N/A',
      'CPF': col.cpf || 'N/A',
      'Email': col.email || 'N/A',
      'Cargo': col.cargo || 'N/A',
      'Setor': col.setor || 'N/A',
      'Tipo': col.tipoColaborador || 'N/A',
      'Empresa': col.empresa || 'N/A',
      'Centro de Custo': col.nomeCentroCusto || 'N/A',
      'Código Centro Custo': col.codigoCentroCusto || 'N/A',
      'Localidade': col.localidadeDescricao || 'N/A',
      'Data Admissão': col.dtadmissao ? new Date(col.dtadmissao).toLocaleDateString('pt-BR') : 'N/A',
      'Data Demissão': col.dtdemissao ? new Date(col.dtdemissao).toLocaleDateString('pt-BR') : 'N/A',
      'Situação': col.situacao || 'N/A',
      'Matrícula Superior': col.matriculasuperior || 'N/A'
    }));
  }

  private converterParaCSV(dados: any[]): string {
    if (dados.length === 0) {
      return '';
    }

    const headers = Object.keys(dados[0]);
    const csvRows = [
      headers.join(','),
      ...dados.map(row => headers.map(header => {
        const value = row[header] || '';
        return `"${String(value).replace(/"/g, '""')}"`;
      }).join(','))
    ];

    return csvRows.join('\n');
  }
}

