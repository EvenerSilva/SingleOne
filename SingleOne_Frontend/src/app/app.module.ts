import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

// import { NgxSpinnerModule } from 'ngx-spinner';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatToolbarModule } from '@angular/material/toolbar';
import {MatButtonModule} from '@angular/material/button';
import {MatIconModule} from '@angular/material/icon';
import {MatSidenavModule} from '@angular/material/sidenav';
import {MatDividerModule} from '@angular/material/divider';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatListModule} from '@angular/material/list';
import {MatCardModule} from '@angular/material/card';
import {MatInputModule} from '@angular/material/input';
import {MatStepperModule} from '@angular/material/stepper';
import {MatGridListModule} from '@angular/material/grid-list';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatDialogModule} from '@angular/material/dialog';
import {MatSnackBarModule} from '@angular/material/snack-bar';
import {MatFormFieldModule} from '@angular/material/form-field';
import {MatSlideToggleModule} from '@angular/material/slide-toggle';
import {MatTableModule} from '@angular/material/table';
import {MatSelectModule} from '@angular/material/select';
import {MatBadgeModule}  from '@angular/material/badge';
import {MatChipsModule}  from '@angular/material/chips';
import {MatPaginatorModule} from '@angular/material/paginator'
import {MatDatepickerModule} from '@angular/material/datepicker';
import {MatTabsModule} from '@angular/material/tabs';
import {MatTooltipModule} from '@angular/material/tooltip';
import {MatAutocompleteModule} from '@angular/material/autocomplete';
import {MatRadioModule} from '@angular/material/radio';
import {MatProgressBarModule} from '@angular/material/progress-bar';
import {MatProgressSpinnerModule} from '@angular/material/progress-spinner';
import {MatMenuModule} from '@angular/material/menu';
import { MatNativeDateModule, MAT_DATE_LOCALE } from '@angular/material/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MeusPipesModule } from './pipes/meus-pipes.module';
import { SpinnersAngularModule } from 'spinners-angular';
import { NgxSpinnerModule } from 'ngx-spinner';
import { LoginComponent } from './pages/usuarios/login/login.component';
import { EsqueciSenhaComponent } from './pages/usuarios/esqueci-senha/esqueci-senha.component';
import { UsuarioComponent } from './pages/usuarios/usuario/usuario.component';
import { ValidarPalavraChaveComponent } from './pages/usuarios/validar-palavra-chave/validar-palavra-chave.component';
import { CadastrosComponent } from './pages/cadastros/cadastros/cadastros.component';
import { ClientesComponent } from './pages/cadastros/clientes/clientes/clientes.component';
import { ClienteComponent } from './pages/cadastros/clientes/cliente/cliente.component';
import { EmpresasComponent } from './pages/cadastros/empresas/empresas/empresas.component';
import { EmpresaComponent } from './pages/cadastros/empresas/empresa/empresa.component';
import { EmpresaWizardComponent } from './pages/cadastros/empresas/empresa-wizard/empresa-wizard.component';
import { FilialComponent } from './pages/cadastros/filiais/filial/filial.component';
import { FiliaisComponent } from './pages/cadastros/filiais/filiais/filiais.component';
import { LocalidadesModule } from './pages/cadastros/localidades/localidades.module';
import { CentroCustosComponent } from './pages/cadastros/centroCustos/centro-custos/centro-custos.component';
import { CentroCustoComponent } from './pages/cadastros/centroCustos/centro-custo/centro-custo.component';
import { FornecedoresComponent } from './pages/cadastros/fornecedores/fornecedores/fornecedores.component';
import { FornecedorComponent } from './pages/cadastros/fornecedores/fornecedor/fornecedor.component';
import { TiposRecursosComponent } from './pages/cadastros/tiposRecursos/tipos-recursos/tipos-recursos.component';
import { TipoRecursoComponent } from './pages/cadastros/tiposRecursos/tipo-recurso/tipo-recurso.component';
import { FabricantesComponent } from './pages/cadastros/fabricantes/fabricantes/fabricantes.component';
import { FabricanteComponent } from './pages/cadastros/fabricantes/fabricante/fabricante.component';
import { ModelosComponent } from './pages/cadastros/modelos/modelos/modelos.component';
import { ModeloComponent } from './pages/cadastros/modelos/modelo/modelo.component';
import { TelefoniaComponent } from './pages/cadastros/telefonia/telefonia.component';
import { TelecomComponent } from './pages/cadastros/telecom/telecom.component';
import { OperadorasComponent } from './pages/cadastros/telefonia/operadoras/operadoras/operadoras.component';
import { OperadoraComponent } from './pages/cadastros/telefonia/operadoras/operadora/operadora.component';
import { PlanosComponent } from './pages/cadastros/telefonia/planos/planos/planos.component';
import { PlanoComponent } from './pages/cadastros/telefonia/planos/plano/plano.component';
import { LinhasComponent } from './pages/cadastros/telefonia/linhas/linhas/linhas.component';
import { LinhaComponent } from './pages/cadastros/telefonia/linhas/linha/linha.component';
import { ImportarLinhasComponent } from './pages/telefonia/importar-linhas/importar-linhas.component';
import { NotasFiscaisComponent } from './pages/cadastros/notasFiscais/notas-fiscais/notas-fiscais.component';
import { NotaFiscalComponent } from './pages/cadastros/notasFiscais/nota-fiscal/nota-fiscal.component';
import { LaudosComponent } from './pages/cadastros/laudos/laudos/laudos.component';
import { LaudoComponent } from './pages/cadastros/laudos/laudo/laudo.component';
import { EquipamentosComponent } from './pages/equipamentos/equipamentos/equipamentos.component';
import { EquipamentoComponent } from './pages/equipamentos/equipamento/equipamento.component';
import { BoComponent } from './pages/equipamentos/bo/bo.component';
import { ColaboradoresComponent } from './pages/colaboradores/colaboradores/colaboradores.component';
import { ImportacaoColaboradoresComponent } from './pages/colaboradores/importacao-colaboradores/importacao-colaboradores.component';
import { ModalImportarColaboradoresComponent } from './pages/colaboradores/importacao-colaboradores/modal-importar-colaboradores/modal-importar-colaboradores.component';
import { ModalExportarColaboradoresComponent } from './pages/colaboradores/importacao-colaboradores/modal-exportar-colaboradores/modal-exportar-colaboradores.component';
import { ColaboradorComponent } from './pages/colaboradores/colaborador/colaborador.component';
import { TemplatesComponent } from './pages/cadastros/templates/templates/templates.component';
import { TemplateComponent } from './pages/cadastros/templates/template/template.component';
import { CKEditorModule } from '@ckeditor/ckeditor5-angular';
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { MovimentacoesComponent } from './pages/requisicoes/movimentacoes/movimentacoes.component';
import { RequisicoesComponent } from './pages/requisicoes/requisicoes/requisicoes/requisicoes.component';
import { RequisicaoComponent } from './pages/requisicoes/requisicoes/requisicao/requisicao.component';
import { EntregasDevolucoesComponent } from './pages/requisicoes/entregas-devolucoes/entregas-devolucoes.component';
import { EntregasDevolucoesFinalComponent } from './pages/requisicoes/entregas-devolucoes-final/entregas-devolucoes-final.component';
import { AdicionarObservacaoComponent } from './pages/requisicoes/adicionar-observacao/adicionar-observacao.component';
import { AgendamentoComponent } from './pages/requisicoes/agendamento/agendamento.component';
import { TermosComponent } from './pages/requisicoes/termos/termos.component';
import { TimelapsesComponent } from './pages/timelapses/timelapses.component';
import { TimelapseRecursosComponent } from './pages/relatorios/timelapse-recursos/timelapse-recursos.component';
import { TimelapseColaboradoresComponent } from './pages/relatorios/timelapse-colaboradores/timelapse-colaboradores.component';
import { MovimentacoesColaboradoresComponent } from './pages/relatorios/movimentacoes-colaboradores/movimentacoes-colaboradores.component';
import { AngularXTimelineModule } from 'angularx-timeline';
import { TermoEletronicoComponent } from './pages/colaboradores/termo-eletronico/termo-eletronico.component';
import { NadaConstaComponent } from './pages/requisicoes/nada-consta/nada-consta.component';
import { UsuariosComponent } from './pages/usuarios/usuarios/usuarios.component';
import { LaudoFinalComponent } from './pages/cadastros/laudos/laudo-final/laudo-final.component';
import { EquipamentosStatusDetalheComponent } from './pages/relatorios/equipamentos-status-detalhe/equipamentos-status-detalhe.component';
import { AuditoriaAcessosComponent } from './pages/relatorios/auditoria-acessos/auditoria-acessos.component';
import { GestaoGarantiasComponent } from './pages/relatorios/gestao-garantias/gestao-garantias.component';
import { ColaboradoresSemRecursosComponent } from './pages/relatorios/colaboradores-sem-recursos/colaboradores-sem-recursos.component';
import { LaudoVisualizarComponent } from './pages/cadastros/laudos/laudo-visualizar/laudo-visualizar.component';
import { NgxCurrencyModule } from 'ngx-currency';
import { ParametrosComponent } from './pages/cadastros/parametros/parametros.component';
import { CategoriasComponent } from './pages/cadastros/categorias/categorias/categorias.component';
import { CategoriaFormComponent } from './pages/cadastros/categorias/categoria-form/categoria-form.component';
import { CargosconfiancaComponent } from './pages/parametros/cargosconfianca/cargosconfianca.component';
import { PatrimonioComponent } from './pages/patrimonio/patrimonio.component';

// Core Module
import { CoreModule } from './core/core.module';

// Shared Module
import { SharedModule } from './shared/shared.module';
import { TinOneModule } from './tinone/tinone.module';

// CoreUI Components - Implementação personalizada com Bootstrap
import { TwoFactorVerificationComponent } from './pages/auth/two-factor-verification/two-factor-verification.component';
import { DescarteComponent } from './pages/requisicoes/descarte/descarte.component';
import { ModalEvidenciasComponent } from './pages/requisicoes/descarte/modal-evidencias/modal-evidencias.component';
import { DescarteProtocoloComponent } from './pages/requisicoes/descarte/descarte-protocolo.component';
import { ModalProtocoloComponent } from './pages/requisicoes/descarte/modal-protocolo/modal-protocolo.component';
import { ConfirmacaoComponent } from './pages/confirmacao/confirmacao.component';
import { ModalSerialPatrimonioComponent } from './pages/equipamentos/modal-serial-patrimonio/modal-serial-patrimonio.component';
import { ModalCompartilharItemComponent } from './pages/requisicoes/entregas-devolucoes/modal-compartilhar-item/modal-compartilhar-item.component';

import { MessageboxComponent } from './pages/messagebox/messagebox.component';
import { ManutencoesComValorComponent } from './pages/relatorios/manutencoes-com-valor/manutencoes-com-valor.component';
import { DesligamentoProgramadoComponent } from './pages/colaboradores/desligamento-programado/desligamento-programado.component';
import { VisualizarComponent } from './pages/equipamentos/visualizar/visualizar.component';
import { HttpClientModule } from "@angular/common/http";
import { CnpjMaskDirective } from "./util/mask-cnpj"
import { CpfMaskDirective } from "./util/mask-cpf"

import { ContratoTelefoniaComponent } from './pages/cadastros/telefonia/contratos-telefonia/contrato-telefonia/contrato-telefonia.component';
import { ContratosTelefoniaComponent } from './pages/cadastros/telefonia/contratos-telefonia/contratos-telefonia/contratos-telefonia.component';
import { ContratosComponent } from './pages/contratos/contratos/contratos.component';
import { ContratoComponent } from './pages/contratos/contrato/contrato.component';
import { VisualizarNotafiscalComponent } from './pages/cadastros/notasFiscais/visualizar-notafiscal/visualizar-notafiscal.component';
import { VisualizarNotafiscalModalComponent } from './pages/cadastros/notasFiscais/visualizar-notafiscal-modal/visualizar-notafiscal-modal.component';
import { EquipamentosWizardComponent } from './pages/cadastros/equipamentos-wizard/equipamentos-wizard.component';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { DesligadosModalComponent } from './pages/dashboard/modals/desligados-modal/desligados-modal.component';
import { DevolucoesProgramadasModalComponent } from './pages/dashboard/modals/devolucoes-programadas-modal/devolucoes-programadas-modal.component';
import { ConfiguracoesComponent } from './pages/configuracoes/configuracoes/configuracoes.component';
import { ContestacoesComponent } from './pages/contestacoes/contestacoes/contestacoes.component';
import { ContestacaoComponent } from './pages/contestacoes/contestacao/contestacao.component';
import { PortariaComponent } from './pages/portaria/portaria.component';
import { VerificarTermoComponent } from './pages/verificar-termo/verificar-termo.component';
import { MinhasContestacoesComponent } from './pages/patrimonio/minhas-contestacoes.component';
import { SinalizacoesSuspeitasComponent } from './pages/relatorios/sinalizacoes-suspeitas/sinalizacoes-suspeitas.component';
import { PoliticasElegibilidadeComponent } from './pages/configuracoes/politicas-elegibilidade/politicas-elegibilidade.component';
import { NaoConformidadeElegibilidadeComponent } from './pages/relatorios/nao-conformidade-elegibilidade/nao-conformidade-elegibilidade.component';
import { TinOneConfigComponent } from './pages/configuracoes/tinone/tinone-config.component';
import { MapaRecursosComponent } from './pages/relatorios/mapa-recursos/mapa-recursos.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    EsqueciSenhaComponent,
    UsuarioComponent,
    ValidarPalavraChaveComponent,
    DashboardComponent,
    CadastrosComponent,
    ClientesComponent,
    ClienteComponent,
    EmpresasComponent,
    EmpresaComponent,
    EmpresaWizardComponent,
    CentroCustosComponent,
    CentroCustoComponent,
    FornecedoresComponent,
    FornecedorComponent,
    TiposRecursosComponent,
    TipoRecursoComponent,
    FabricantesComponent,
    FabricanteComponent,
    ModelosComponent,
    ModeloComponent,
    TelefoniaComponent,
    ContratosTelefoniaComponent,
    ContratoTelefoniaComponent,
    TelecomComponent,
    OperadorasComponent,
    OperadoraComponent,
    PlanosComponent,
    PlanoComponent,
    LinhasComponent,
    LinhaComponent,
    NotasFiscaisComponent,
    NotaFiscalComponent,
    LaudosComponent,
    LaudoComponent,
    EquipamentosComponent,
    EquipamentoComponent,
    VisualizarComponent,
    BoComponent,
    ColaboradoresComponent,
    ImportacaoColaboradoresComponent,
    ModalImportarColaboradoresComponent,
    ModalExportarColaboradoresComponent,
    ColaboradorComponent,
    TemplatesComponent,
    TemplateComponent,
    MovimentacoesComponent,
    RequisicoesComponent,
    RequisicaoComponent,
    EntregasDevolucoesComponent,
    EntregasDevolucoesFinalComponent,
    AdicionarObservacaoComponent,
    AgendamentoComponent,
    TermosComponent,
    TimelapsesComponent,
    TimelapseRecursosComponent,
    TimelapseColaboradoresComponent,
    MovimentacoesColaboradoresComponent,
    TermoEletronicoComponent,
    NadaConstaComponent,
    UsuariosComponent,
    LaudoFinalComponent,
    EquipamentosStatusDetalheComponent,
    AuditoriaAcessosComponent,
    GestaoGarantiasComponent,
    ColaboradoresSemRecursosComponent,
    LaudoVisualizarComponent,
    ParametrosComponent,
    CategoriasComponent,
    CategoriaFormComponent,
    TwoFactorVerificationComponent,
    DescarteComponent,
    ModalEvidenciasComponent,
    DescarteProtocoloComponent,
    ModalProtocoloComponent,
    ConfirmacaoComponent,
    ModalSerialPatrimonioComponent,
    ModalCompartilharItemComponent,

    MessageboxComponent,
    ManutencoesComValorComponent,
    DesligamentoProgramadoComponent,
    VisualizarComponent,
    CnpjMaskDirective,
    CpfMaskDirective,
    ContratosComponent,
    ContratoComponent,
    VisualizarNotafiscalComponent,
    VisualizarNotafiscalModalComponent,
    EquipamentosWizardComponent,
    ConfiguracoesComponent,
    CargosconfiancaComponent,
    PatrimonioComponent,
    ContestacoesComponent,
    ContestacaoComponent,
    PortariaComponent,
    VerificarTermoComponent,
    MinhasContestacoesComponent,
    SinalizacoesSuspeitasComponent,
    PoliticasElegibilidadeComponent,
    NaoConformidadeElegibilidadeComponent,
    TinOneConfigComponent,
    DesligadosModalComponent,
    DevolucoesProgramadasModalComponent,
    ImportarLinhasComponent,
    MapaRecursosComponent
  ],
  imports: [
    BrowserModule,
    CommonModule,
    RouterModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    CoreModule,
    // NgxSpinnerModule,
    NgxSpinnerModule,
    SpinnersAngularModule,
    MatToolbarModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatDividerModule,
    MatExpansionModule,
    MatListModule,
    MatCardModule,
    MatInputModule,
    MatStepperModule,
    MatGridListModule,
    MatCheckboxModule,
    MatDialogModule,
    MatSnackBarModule,
    MatFormFieldModule,
    MatSlideToggleModule,
    MatTableModule,
    MatTabsModule,
    MatListModule,
    MatGridListModule,
    MatBadgeModule,
    MatChipsModule,
    MatPaginatorModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTooltipModule,
    MatAutocompleteModule,
    MatDialogModule,
    MatSelectModule,
    MatRadioModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatMenuModule,
    ReactiveFormsModule,
    MeusPipesModule,
    CKEditorModule,
    FormsModule,
    NgxMatSelectSearchModule,
    AngularXTimelineModule,
    NgxCurrencyModule,
    LocalidadesModule,
    SharedModule,
    TinOneModule  // 🦉 Oni o Sábio - Assistente Virtual (isolado e opcional)
  ],
  providers: [{provide: MAT_DATE_LOCALE, useValue: 'pt-BR'}],
  entryComponents: [
    AdicionarObservacaoComponent,
    AgendamentoComponent,
    ModalCompartilharItemComponent,
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
