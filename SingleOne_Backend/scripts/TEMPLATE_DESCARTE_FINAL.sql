-- =====================================================
-- Template de Protocolo de Descarte - VERSÃO FINAL
-- Usa marcadores @ (padrão do sistema)
-- Otimizado para iText7 (tabelas HTML, sem CSS Grid/border-radius)
-- =====================================================

UPDATE templates 
SET conteudo = '<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <title>Termo de Descarte de Equipamentos</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 40px;
            color: #2c3e50;
            line-height: 1.6;
        }
        .header {
            text-align: center;
            border-bottom: 4px solid #FF3A0F;
            padding-bottom: 20px;
            margin-bottom: 30px;
        }
        .header h1 {
            color: #FF3A0F;
            font-size: 28px;
            margin: 0 0 10px 0;
            text-transform: uppercase;
        }
        .header .protocolo {
            font-size: 18px;
            font-weight: bold;
            color: #34495e;
        }
        .section {
            margin: 30px 0;
            padding: 20px;
            background: #f8f9fa;
            border-left: 4px solid #FF3A0F;
        }
        .section h2 {
            color: #FF3A0F;
            font-size: 18px;
            margin: 0 0 15px 0;
            border-bottom: 2px solid #e9ecef;
            padding-bottom: 10px;
        }
        .info-table {
            width: 100%;
            margin: 15px 0;
            border-collapse: collapse;
        }
        .info-table td {
            padding: 8px 10px;
            vertical-align: top;
        }
        .info-item {
            margin: 8px 0;
        }
        .label {
            font-weight: bold;
            color: #495057;
        }
        .value {
            color: #2c3e50;
        }
        .logistica-reversa {
            background: #d4edda;
            border-left-color: #28a745;
        }
        .mtr-section {
            background: #fff3cd;
            border-left-color: #ffc107;
        }
        .mtr-section h2 {
            color: #856404;
        }
        .footer {
            margin-top: 60px;
            text-align: center;
            padding-top: 20px;
            border-top: 2px solid #e9ecef;
        }
        .assinatura {
            margin-top: 50px;
            text-align: center;
        }
        .assinatura-linha {
            border-top: 2px solid #2c3e50;
            width: 300px;
            margin: 0 auto 10px auto;
        }
    </style>
</head>
<body>
    <div class="header">
        <h1>TERMO DE DESCARTE DE EQUIPAMENTOS</h1>
        <div class="protocolo">Protocolo: @numeroProtocolo</div>
    </div>

    <div class="section">
        <h2>Informações do Protocolo</h2>
        <table class="info-table">
            <tr>
                <td width="50%">
                    <div class="info-item"><span class="label">Cliente:</span> <span class="value">@cliente</span></div>
                </td>
                <td width="50%">
                    <div class="info-item"><span class="label">Tipo de Descarte:</span> <span class="value">@tipoDescarte</span></div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="info-item"><span class="label">Responsável:</span> <span class="value">@responsavel</span></div>
                </td>
                <td>
                    <div class="info-item"><span class="label">Data de Criação:</span> <span class="value">@dataCriacao</span></div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="info-item"><span class="label">Data de Conclusão:</span> <span class="value">@dataConclusao</span></div>
                </td>
                <td>
                    <div class="info-item"><span class="label">Total de Equipamentos:</span> <span class="value">@quantidadeEquipamentos</span></div>
                </td>
            </tr>
        </table>
        <div class="info-item"><span class="label">Motivo:</span> <span class="value">@motivoDescarte</span></div>
        <div class="info-item"><span class="label">Destino:</span> <span class="value">@destinoFinal</span></div>
    </div>

    <div class="section logistica-reversa">
        <h2>Logística Reversa (Lei 12.305/2010)</h2>
        <div class="info-item">
            <span class="label">Empresa Destino:</span>
            <span class="value">@empresaDestino</span>
        </div>
        <div class="info-item">
            <span class="label">CNPJ:</span>
            <span class="value">@cnpjDestino</span>
        </div>
        <div class="info-item">
            <span class="label">Certificado:</span>
            <span class="value">@certificado</span>
        </div>
        <p style="font-size: 12px; color: #155724; margin-top: 15px; font-style: italic;">
            Em conformidade com a Política Nacional de Resíduos Sólidos
        </p>
    </div>

    <div class="section mtr-section">
        <h2>📋 MTR - Manifesto de Transporte de Resíduos</h2>
        <table class="info-table">
            <tr>
                <td width="50%">
                    <div class="info-item"><span class="label">MTR Obrigatório:</span> <span class="value">@mtrObrigatorio</span></div>
                </td>
                <td width="50%">
                    <div class="info-item"><span class="label">Número do MTR:</span> <span class="value">@mtrNumero</span></div>
                </td>
            </tr>
            <tr>
                <td>
                    <div class="info-item"><span class="label">Emitido Por:</span> <span class="value">@mtrEmitidoPor</span></div>
                </td>
                <td>
                    <div class="info-item"><span class="label">Data de Emissão:</span> <span class="value">@mtrDataEmissao</span></div>
                </td>
            </tr>
            <tr>
                <td colspan="2">
                    <div class="info-item"><span class="label">Validade do MTR:</span> <span class="value">@mtrValidade</span></div>
                </td>
            </tr>
        </table>
        @mtrDadosTransportadora
        <p style="font-size: 12px; color: #856404; margin-top: 15px; font-style: italic;">
            O MTR (Manifesto de Transporte de Resíduos) é obrigatório conforme Resolução CONAMA nº 313/2002
        </p>
    </div>

    <div class="section">
        <h2>Equipamentos Descartados</h2>
        @listaEquipamentos
    </div>

    <div class="section">
        <h2>Observações Gerais</h2>
        <p>@observacoes</p>
    </div>

    <div class="assinatura">
        <div class="assinatura-linha"></div>
        <p style="margin: 5px 0; font-weight: bold;">@responsavel</p>
        <p style="margin: 5px 0; font-size: 14px; color: #6c757d;">Responsável pelo Processo de Descarte</p>
    </div>

    <div class="footer">
        <p style="font-size: 11px; color: #6c757d;">Documento gerado pelo Sistema SingleOne</p>
    </div>
</body>
</html>',
    dataalteracao = NOW(),
    versao = versao + 1
WHERE id = 5;

-- Verificar
SELECT id, titulo, CASE WHEN conteudo LIKE '%@numeroProtocolo%' THEN 'SIM - USA MARCADORES @' ELSE 'NAO' END as usa_marcadores FROM templates WHERE id = 5;

