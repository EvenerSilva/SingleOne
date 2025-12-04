import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { CadastrosComponent } from './pages/cadastros/cadastros/cadastros.component';
import { CentroCustoComponent } from './pages/cadastros/centroCustos/centro-custo/centro-custo.component';
import { CentroCustosComponent } from './pages/cadastros/centroCustos/centro-custos/centro-custos.component';
import { ClienteComponent } from './pages/cadastros/clientes/cliente/cliente.component';
import { ClientesComponent } from './pages/cadastros/clientes/clientes/clientes.component';
import { EmpresaComponent } from './pages/cadastros/empresas/empresa/empresa.component';
import { EmpresasComponent } from './pages/cadastros/empresas/empresas/empresas.component';
import { EmpresaWizardComponent } from './pages/cadastros/empresas/empresa-wizard/empresa-wizard.component';
import { FilialComponent } from './pages/cadastros/filiais/filial/filial.component';
import { FiliaisComponent } from './pages/cadastros/filiais/filiais/filiais.component';
import { LocalidadeComponent } from './pages/cadastros/localidades/localidade/localidade.component';
import { LocalidadesComponent } from './pages/cadastros/localidades/localidades/localidades.component';

import { FabricantesComponent } from './pages/cadastros/fabricantes/fabricantes/fabricantes.component';
import { FornecedorComponent } from './pages/cadastros/fornecedores/fornecedor/fornecedor.component';
import { FornecedoresComponent } from './pages/cadastros/fornecedores/fornecedores/fornecedores.component';
import { ModeloComponent } from './pages/cadastros/modelos/modelo/modelo.component';
import { ModelosComponent } from './pages/cadastros/modelos/modelos/modelos.component';
import { NotasFiscaisComponent } from './pages/cadastros/notasFiscais/notas-fiscais/notas-fiscais.component';
import { NotaFiscalComponent } from './pages/cadastros/notasFiscais/nota-fiscal/nota-fiscal.component';
import { LinhaComponent } from './pages/cadastros/telefonia/linhas/linha/linha.component';
import { LinhasComponent } from './pages/cadastros/telefonia/linhas/linhas/linhas.component';
import { OperadoraComponent } from './pages/cadastros/telefonia/operadoras/operadora/operadora.component';
import { OperadorasComponent } from './pages/cadastros/telefonia/operadoras/operadoras/operadoras.component';
import { PlanoComponent } from './pages/cadastros/telefonia/planos/plano/plano.component';
import { PlanosComponent } from './pages/cadastros/telefonia/planos/planos/planos.component';
import { TelefoniaComponent } from './pages/cadastros/telefonia/telefonia.component';

import { TiposRecursosComponent } from './pages/cadastros/tiposRecursos/tipos-recursos/tipos-recursos.component';
import { CategoriasComponent } from './pages/cadastros/categorias/categorias/categorias.component';
import { CategoriaFormComponent } from './pages/cadastros/categorias/categoria-form/categoria-form.component';
import { EquipamentosWizardComponent } from './pages/cadastros/equipamentos-wizard/equipamentos-wizard.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { LoginComponent } from './pages/usuarios/login/login.component';
import { LaudosComponent } from './pages/cadastros/laudos/laudos/laudos.component';
import { LaudoComponent } from './pages/cadastros/laudos/laudo/laudo.component';
import { EquipamentosComponent } from './pages/equipamentos/equipamentos/equipamentos.component';
import { EquipamentoComponent } from './pages/equipamentos/equipamento/equipamento.component';
import { BoComponent } from './pages/equipamentos/bo/bo.component';
import { TemplatesComponent } from './pages/cadastros/templates/templates/templates.component';
import { TemplateComponent } from './pages/cadastros/templates/template/template.component';
import { ColaboradoresComponent } from './pages/colaboradores/colaboradores/colaboradores.component';
import { ImportacaoColaboradoresComponent } from './pages/colaboradores/importacao-colaboradores/importacao-colaboradores.component';
import { ColaboradorComponent } from './pages/colaboradores/colaborador/colaborador.component';
import { MovimentacoesComponent } from './pages/requisicoes/movimentacoes/movimentacoes.component';
import { RequisicoesComponent } from './pages/requisicoes/requisicoes/requisicoes/requisicoes.component';
import { RequisicaoComponent } from './pages/requisicoes/requisicoes/requisicao/requisicao.component';
import { EntregasDevolucoesComponent } from './pages/requisicoes/entregas-devolucoes/entregas-devolucoes.component';
import { EntregasDevolucoesFinalComponent } from './pages/requisicoes/entregas-devolucoes-final/entregas-devolucoes-final.component';
import { TermosComponent } from './pages/requisicoes/termos/termos.component';
import { TimelapsesComponent } from './pages/timelapses/timelapses.component';
import { TimelapseRecursosComponent } from './pages/relatorios/timelapse-recursos/timelapse-recursos.component';
import { TimelapseColaboradoresComponent } from './pages/relatorios/timelapse-colaboradores/timelapse-colaboradores.component';
import { MovimentacoesColaboradoresComponent } from './pages/relatorios/movimentacoes-colaboradores/movimentacoes-colaboradores.component';
import { TermoEletronicoComponent } from './pages/colaboradores/termo-eletronico/termo-eletronico.component';
import { NadaConstaComponent } from './pages/requisicoes/nada-consta/nada-consta.component';
import { UsuariosComponent } from './pages/usuarios/usuarios/usuarios.component';
import { UsuarioComponent } from './pages/usuarios/usuario/usuario.component';
import { LaudoFinalComponent } from './pages/cadastros/laudos/laudo-final/laudo-final.component';
import { EsqueciSenhaComponent } from './pages/usuarios/esqueci-senha/esqueci-senha.component';
import { ValidarPalavraChaveComponent } from './pages/usuarios/validar-palavra-chave/validar-palavra-chave.component';
import { EquipamentosStatusDetalheComponent } from './pages/relatorios/equipamentos-status-detalhe/equipamentos-status-detalhe.component';
import { AuditoriaAcessosComponent } from './pages/relatorios/auditoria-acessos/auditoria-acessos.component';
import { GestaoGarantiasComponent } from './pages/relatorios/gestao-garantias/gestao-garantias.component';
import { SinalizacoesSuspeitasComponent } from './pages/relatorios/sinalizacoes-suspeitas/sinalizacoes-suspeitas.component';
import { ParametrosComponent } from './pages/cadastros/parametros/parametros.component';
import { TwoFactorVerificationComponent } from './pages/auth/two-factor-verification/two-factor-verification.component';
import { DescarteComponent } from './pages/requisicoes/descarte/descarte.component';
import { DescarteProtocoloComponent } from './pages/requisicoes/descarte/descarte-protocolo.component';
import { ManutencoesComValorComponent } from './pages/relatorios/manutencoes-com-valor/manutencoes-com-valor.component';
import { VisualizarComponent } from './pages/equipamentos/visualizar/visualizar.component';
import { ContratoTelefoniaComponent } from './pages/cadastros/telefonia/contratos-telefonia/contrato-telefonia/contrato-telefonia.component';
import { ContratosTelefoniaComponent } from './pages/cadastros/telefonia/contratos-telefonia/contratos-telefonia/contratos-telefonia.component';
import { ContratosComponent } from './pages/contratos/contratos/contratos.component';
import { ContratoComponent } from './pages/contratos/contrato/contrato.component';
import { VisualizarNotafiscalComponent } from './pages/cadastros/notasFiscais/visualizar-notafiscal/visualizar-notafiscal.component';
import { ConfiguracoesComponent } from './pages/configuracoes/configuracoes/configuracoes.component';
import { TelecomComponent } from './pages/cadastros/telecom/telecom.component';
import { CargosconfiancaComponent } from './pages/parametros/cargosconfianca/cargosconfianca.component';
import { PatrimonioComponent } from './pages/patrimonio/patrimonio.component';
import { PortariaComponent } from './pages/portaria/portaria.component';
import { VerificarTermoComponent } from './pages/verificar-termo/verificar-termo.component';
import { ContestacoesComponent } from './pages/contestacoes/contestacoes/contestacoes.component';
import { ContestacaoComponent } from './pages/contestacoes/contestacao/contestacao.component';
import { AuthGuard } from './core/guards/auth.guard';
import { PoliticasElegibilidadeComponent } from './pages/configuracoes/politicas-elegibilidade/politicas-elegibilidade.component';
import { NaoConformidadeElegibilidadeComponent } from './pages/relatorios/nao-conformidade-elegibilidade/nao-conformidade-elegibilidade.component';
import { ColaboradoresSemRecursosComponent } from './pages/relatorios/colaboradores-sem-recursos/colaboradores-sem-recursos.component';
import { TinOneConfigComponent } from './pages/configuracoes/tinone/tinone-config.component';
import { ImportarLinhasComponent } from './pages/telefonia/importar-linhas/importar-linhas.component';
import { MapaRecursosComponent } from './pages/relatorios/mapa-recursos/mapa-recursos.component';

const routes: Routes = [
  {
    path: '',
    redirectTo: 'login',
    pathMatch: 'full'
  },
  {
    path: 'login',
    component: LoginComponent
  },
  {
    path: 'two-factor-verification',
    component: TwoFactorVerificationComponent
  },
  {
    path: 'esqueci-senha',
    component: EsqueciSenhaComponent
  },
  {
    path: 'validar-token/:token',
    component: ValidarPalavraChaveComponent
  },
  {
    path: 'dashboard',
    component: DashboardComponent
  },
  {
    path: 'cadastros',
    component: CadastrosComponent
  },
  {
    path: 'equipamentos-wizard',
    component: EquipamentosWizardComponent
  },
  {
    path: 'clientes',
    component: ClientesComponent
  },
  {
    path: 'cliente',
    component: ClienteComponent
  },
  {
    path: 'cliente/:id',
    component: ClienteComponent
  },
  {
    path: 'empresas',
    component: EmpresasComponent
  },
  {
    path: 'empresa',
    component: EmpresaComponent
  },
  {
    path: 'empresa/:id',
    component: EmpresaComponent
  },
  {
    path: 'empresa-wizard',
    component: EmpresaWizardComponent
  },
  {
    path: 'filiais',
    component: FiliaisComponent
  },
  {
    path: 'filial',
    component: FilialComponent
  },
  {
    path: 'filial/:id',
    component: FilialComponent
  },
  {
    path: 'localidades',
    component: LocalidadesComponent
  },
  {
    path: 'localidade',
    component: LocalidadeComponent
  },
  {
    path: 'localidade/:id',
    component: LocalidadeComponent
  },
  {
    path: 'centros-custos',
    component: CentroCustosComponent
  },

  {
    path: 'fornecedores',
    component: FornecedoresComponent
  },
  {
    path: 'fornecedor',
    component: FornecedorComponent
  },
  {
    path: 'fornecedor/:id',
    component: FornecedorComponent
  },
  {
    path: 'tipos-recursos',
    component: TiposRecursosComponent
  },

  {
    path: 'categorias',
    component: CategoriasComponent
  },
  {
    path: 'fabricantes',
    component: FabricantesComponent
  },

  {
    path: 'modelos',
    component: ModelosComponent
  },
  {
    path: 'modelo',
    component: ModeloComponent
  },
  {
    path: 'modelo/:id',
    component: ModeloComponent
  },
  {
    path: 'telefonia',
    component: TelefoniaComponent
  },
  {
    path: 'operadoras',
    component: OperadorasComponent
  },
  {
    path: 'operadora',
    component: OperadoraComponent
  },
  {
    path: 'operadora/:id',
    component: OperadoraComponent
  },
  {
    path: 'contratos-telefonia',
    component: ContratosTelefoniaComponent
  },
  {
    path: 'contrato-telefonia',
    component: ContratoTelefoniaComponent
  },
  {
    path: 'contrato-telefonia/:id',
    component: ContratoTelefoniaComponent
  },
  {
    path: 'planos',
    component: PlanosComponent
  },
  {
    path: 'plano',
    component: PlanoComponent
  },
  {
    path: 'plano/:id',
    component: PlanoComponent
  },
  {
    path: 'linhas',
    component: LinhasComponent
  },
  {
    path: 'linha',
    component: LinhaComponent
  },
  {
    path: 'linha/:id',
    component: LinhaComponent
  },
  {
    path: 'telecom',
    component: TelecomComponent
  },
  {
    path: 'telecom/importar-linhas',
    component: ImportarLinhasComponent
  },
  {
    path: 'notas-fiscais',
    component: NotasFiscaisComponent
  },
  {
    path: 'nota-fiscal',
    component: NotaFiscalComponent
  },
  {
    path: 'nota-fiscal/:id',
    component: NotaFiscalComponent
  },
  //
  {
    path: 'notas-fiscais/visualizar-notafiscal/:id',
    component: VisualizarNotafiscalComponent
  },
  {
    path: 'laudos',
    component: LaudosComponent
  },
  {
    path: 'abrir-laudo',
    component: LaudoComponent
  },
  {
    path: 'encerrar-laudo/:id',
    component: LaudoFinalComponent
  },
  {
    path: 'templates',
    component: TemplatesComponent
  },
  {
    path: 'template',
    component: TemplateComponent
  },
  {
    path: 'template/:id',
    component: TemplateComponent
  },
  {
    path: 'recursos',
    component: EquipamentosComponent
  },
  {
    path: 'recursos/:idContrato',
    component: EquipamentosComponent
  },
  {
    path: 'recurso',
    component: EquipamentoComponent
  },
  {
    path: 'recurso/:id',
    component: EquipamentoComponent
  },
  {
    path: 'bo/:id',
    component: BoComponent
  },
  {
    path: 'colaboradores',
    component: ColaboradoresComponent
  },
  {
    path: 'colaboradores/importacoes',
    component: ImportacaoColaboradoresComponent
  },
  {
    path: 'colaborador',
    component: ColaboradorComponent
  },
  {
    path: 'colaborador/:id',
    component: ColaboradorComponent
  },
  {
    path: 'movimentacoes',
    component: MovimentacoesComponent
  },
  {
    path: 'nova-requisicao',
    component: RequisicaoComponent
  },
  {
    path: 'nova-requisicao/:id',
    component: RequisicaoComponent
  },
  {
    path: 'movimentacoes/requisicoes',
    component: RequisicoesComponent
  },
  {
    path: 'movimentacoes/requisicoes/requisicao',
    component: RequisicaoComponent
  },
  {
    path: 'movimentacoes/requisicoes/requisicao/:id',
    component: RequisicaoComponent
  },
  {
    path: 'movimentacoes/entregas-devolucoes',
    component: EntregasDevolucoesComponent
  },
  {
    path: 'movimentacoes/entregas-devolucoes/:id',
    component: EntregasDevolucoesComponent
  },
  {
    path: 'movimentacoes/entregas-devolucoes/entrega/:id',
    component: EntregasDevolucoesFinalComponent
  },
  {
    path: 'termos/:hash',
    component: TermosComponent
  },
  {
    path: 'termos/:hash/:isByod',
    component: TermosComponent
  },
  
  // {
  //   path: 'timelapses',
  //   component: TimelapsesComponent
  // },
  {
    path: 'relatorios',
    component: TimelapsesComponent
  },
  {
    // path: 'timelapses/timeline-recursos',
    path: 'relatorios/timeline-recursos',
    component: TimelapseRecursosComponent
  },
  {
    // path: 'timelapses/timeline-recursos',
    path: 'relatorios/timeline-recursos/:id',
    component: TimelapseRecursosComponent
  },
  {
    // path: 'timelapses/timeline-colaboradores',
    path: 'relatorios/timeline-colaboradores',
    component: TimelapseColaboradoresComponent
  },
  {
    // path: 'timelapses/timeline-colaboradores',
    path: 'relatorios/timeline-colaboradores/:id',
    component: TimelapseColaboradoresComponent
  },
  {
    // path: 'timelapses/movimentacoes-colaboradores',
    path: 'relatorios/movimentacoes-colaboradores',
    component: MovimentacoesColaboradoresComponent
  },
  {
    path: 'relatorios/custos-de-manutencao',
    component: ManutencoesComValorComponent
  },
  {
    path: 'relatorios/auditoria-acessos',
    component: AuditoriaAcessosComponent
  },
  {
    path: 'relatorios/gestao-garantias',
    component: GestaoGarantiasComponent
  },
  {
    path: 'relatorios/sinalizacoes-suspeitas',
    component: SinalizacoesSuspeitasComponent
  },
  {
    path: 'termo-eletronico',
    component: TermoEletronicoComponent
  },
  {
    path: 'nada-consta',
    component: NadaConstaComponent
  },
  {
    path: 'usuarios',
    component: UsuariosComponent
  },
  {
    path: 'usuario',
    component: UsuarioComponent
  },
  {
    path: 'usuario/:id',
    component: UsuarioComponent
  },
  {
    path: 'meu-usuario',
    component: UsuarioComponent
  },
  {
    path: 'equipamentos-status-detalhe',
    component: EquipamentosStatusDetalheComponent
  },
  {
    path: 'parametros',
    component: ParametrosComponent
  },
  {
    path: 'configuracoes/cargosconfianca',
    component: CargosconfiancaComponent
  },
  {
    path: 'patrimonio',
    component: PatrimonioComponent
  },
  {
    path: 'portaria',
    component: PortariaComponent
  },
  {
    path: 'verificar-termo/:hash',
    component: VerificarTermoComponent
  },
  {
    path: 'descarte',
    component: DescarteProtocoloComponent
  },
  {
    path: 'descarte-legado',
    component: DescarteComponent
  },
  {
    path: 'visualizar-recurso/:id',
    component: VisualizarComponent
  },
  {
    path: 'contratos',
    component: ContratosComponent
  },
  {
    path: 'contrato',
    component: ContratoComponent
  },
  {
    path: 'contrato/:id',
    component: ContratoComponent
  },
  {
    path: 'configuracoes',
    component: ConfiguracoesComponent
  },
  // 📋 ROTAS DAS CONTESTAÇÕES
  {
    path: 'movimentacoes/contestacoes',
    component: ContestacoesComponent
  },
  {
    path: 'movimentacoes/contestacoes/contestacao',
    component: ContestacaoComponent
  },
  {
    path: 'movimentacoes/contestacoes/contestacao/:id',
    component: ContestacaoComponent
  },
  // 📦 ROTAS DO ESTOQUE MÍNIMO
  {
    path: 'estoque-minimo',
    loadChildren: () => import('./pages/estoque-minimo/estoque-minimo.module').then(m => m.EstoqueMinimoModule)
  },
  // 🔐 ROTAS DE POLÍTICAS DE ELEGIBILIDADE
  {
    path: 'configuracoes/politicas-elegibilidade',
    component: PoliticasElegibilidadeComponent
  },
  {
    path: 'relatorios/nao-conformidade-elegibilidade',
    component: NaoConformidadeElegibilidadeComponent
  },
  // 👥 ROTA DE COLABORADORES SEM RECURSOS
  {
    path: 'relatorios/colaboradores-sem-recursos',
    component: ColaboradoresSemRecursosComponent
  },
  // 🗺️ ROTA DO MAPA DE RECURSOS
  {
    path: 'relatorios/mapa-recursos',
    component: MapaRecursosComponent
  },
  // 🤖 ROTA DO ASSISTENTE TINONE
  {
    path: 'configuracoes/tinone',
    component: TinOneConfigComponent
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
