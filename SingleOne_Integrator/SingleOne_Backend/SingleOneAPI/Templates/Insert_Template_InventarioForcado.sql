-- =========================================================================
-- Script de Inserção do Template ID 6: Notificação de Inventário Forçado
-- =========================================================================
-- Data: 28/10/2025
-- Descrição: Template HTML para envio de e-mail quando um inventário é 
--            forçado para um colaborador sem recursos cadastrados
-- =========================================================================

-- IMPORTANTE: Substitua {CLIENTE_ID} pelo ID do seu cliente antes de executar!

-- 1) Inserir template HTML com variáveis dinâmicas
INSERT INTO templates (
    id,
    tipo,
    cliente,
    titulo,
    conteudo,
    ativo,
    versao,
    datacriacao,
    dataalteracao
) VALUES (
    NEXTVAL('templates_id_seq'), -- ou use o próximo ID disponível
    6, -- Tipo: Notificação de Inventário Forçado
    {CLIENTE_ID}, -- ⚠️ SUBSTITUA pelo ID do cliente
    'Levantamento de Recursos de TI - Ação Necessária',
    '<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Levantamento de Recursos de TI</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: ''Segoe UI'', Tahoma, Geneva, Verdana, sans-serif; line-height: 1.6; color: #333; background-color: #f4f4f4; padding: 20px; }
        .container { max-width: 700px; margin: 0 auto; background-color: #ffffff; border-radius: 8px; box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1); overflow: hidden; }
        .header { background: linear-gradient(135deg, #080039 0%, #1a1a2e 100%); color: white; padding: 30px; text-align: center; }
        .header h1 { font-size: 24px; margin-bottom: 10px; }
        .header p { font-size: 14px; opacity: 0.9; }
        .content { padding: 30px; }
        .greeting { font-size: 16px; margin-bottom: 20px; }
        .info-box { background-color: #f8f9fa; border-left: 4px solid #080039; padding: 15px 20px; margin: 20px 0; border-radius: 4px; }
        .info-box h3 { color: #080039; font-size: 16px; margin-bottom: 10px; display: flex; align-items: center; }
        .info-box h3::before { content: "🔍"; margin-right: 8px; font-size: 20px; }
        .info-box.preparacao h3::before { content: "📝"; }
        .info-box.prazo h3::before { content: "⏰"; }
        .info-box.contato h3::before { content: "📞"; }
        .info-box p { margin-bottom: 8px; color: #555; }
        .recursos-lista { list-style: none; padding-left: 0; margin: 15px 0; }
        .recursos-lista li { padding: 8px 0 8px 30px; position: relative; color: #555; }
        .recursos-lista li::before { content: "•"; color: #FF3A0F; font-weight: bold; font-size: 20px; position: absolute; left: 10px; }
        .prazo-destaque { background-color: #fff3cd; border: 2px solid #ffc107; padding: 15px; border-radius: 4px; margin: 20px 0; text-align: center; }
        .prazo-destaque strong { color: #856404; font-size: 18px; }
        .contato-info { margin-top: 10px; }
        .contato-info p { margin: 5px 0; color: #555; }
        .mensagem-adicional { background-color: #e7f3ff; border-left: 4px solid #2196F3; padding: 15px 20px; margin: 20px 0; border-radius: 4px; }
        .footer { background-color: #f8f9fa; padding: 20px 30px; text-align: center; border-top: 1px solid #e0e0e0; }
        .footer p { color: #666; font-size: 14px; margin: 5px 0; }
        .footer strong { color: #080039; }
    </style>
</head>
<body>
    <div class="container">
        <div class="header">
            <h1>📋 Levantamento de Recursos de TI</h1>
            <p>Ação Necessária - Por favor, leia com atenção</p>
        </div>
        <div class="content">
            <div class="greeting">Olá <strong>@nomeColaborador</strong>,</div>
            <p>Identificamos que você pode ter equipamentos ou recursos de TI sob sua responsabilidade que ainda não estão registrados em nosso sistema de controle patrimonial.</p>
            <div class="info-box">
                <h3>O que faremos:</h3>
                <p>Nossa equipe entrará em contato com você nos próximos dias para realizar um levantamento dos recursos que você possui.</p>
            </div>
            <div class="info-box preparacao">
                <h3>Como você pode se preparar:</h3>
                <p>Por favor, verifique se você tem algum dos seguintes itens:</p>
                <ul class="recursos-lista">
                    <li>Notebook/Desktop</li>
                    <li>Monitor(es)</li>
                    <li>Teclado e Mouse</li>
                    <li>Headset/Fone de ouvido</li>
                    <li>Webcam</li>
                    <li>Adaptadores e cabos</li>
                    <li>Celular corporativo</li>
                    <li>Tablet</li>
                    <li>Outros equipamentos</li>
                </ul>
            </div>
            <div class="prazo-destaque"><strong>⏰ Prazo: @dataLimite</strong></div>
            <div class="info-box contato">
                <h3>Dúvidas?</h3>
                <p>Entre em contato com a equipe de @nomeEquipe:</p>
                <div class="contato-info">
                    <p><strong>E-mail:</strong> @emailEquipe</p>
                    <p><strong>Ramal:</strong> @telefoneEquipe</p>
                </div>
            </div>
            @mensagemAdicional
        </div>
        <div class="footer">
            <p><strong>@nomeEmpresa</strong></p>
            <p>Equipe de Gestão de Patrimônio</p>
            <p style="margin-top: 15px; font-size: 12px; color: #999;">Inventário forçado por: @usuarioQueForçou em @dataForcado</p>
        </div>
    </div>
</body>
</html>',
    TRUE, -- ativo
    1, -- versão inicial
    NOW(), -- data de criação
    NULL -- data de alteração
);

-- =========================================================================
-- VARIÁVEIS DINÂMICAS DISPONÍVEIS NO TEMPLATE:
-- =========================================================================
-- @nomeColaborador   -> Nome do colaborador que receberá o e-mail
-- @cpf               -> CPF do colaborador (opcional)
-- @matricula         -> Matrícula do colaborador (opcional)
-- @cargo             -> Cargo do colaborador
-- @empresa           -> Nome da empresa do colaborador
-- @dataLimite        -> Data limite para resposta (formato: dd/MM/yyyy)
-- @prazoCalculado    -> Prazo em dias úteis (ex: "5 dias úteis")
-- @nomeEquipe        -> Nome da equipe responsável (ex: "TI/Patrimônio")
-- @emailEquipe       -> E-mail de contato da equipe
-- @telefoneEquipe    -> Telefone/ramal de contato
-- @nomeEmpresa       -> Nome da empresa/cliente
-- @mensagemAdicional -> Mensagem personalizada opcional do admin
-- @usuarioQueForçou  -> Nome do usuário que forçou o inventário
-- @dataForcado       -> Data em que o inventário foi forçado
-- =========================================================================

-- =========================================================================
-- EXEMPLO DE USO NO CÓDIGO C#:
-- =========================================================================
/*
var template = _templateRepository.Buscar(x => 
    x.Tipo == 6 && 
    x.Cliente == clienteId && 
    x.Ativo == true
).FirstOrDefault();

if (template != null)
{
    var conteudoEmail = template.Conteudo
        .Replace("@nomeColaborador", colaborador.Nome)
        .Replace("@cpf", colaborador.Cpf)
        .Replace("@matricula", colaborador.Matricula)
        .Replace("@cargo", colaborador.Cargo)
        .Replace("@empresa", colaborador.EmpresaNome)
        .Replace("@dataLimite", DateTime.Now.AddDays(5).ToString("dd/MM/yyyy"))
        .Replace("@prazoCalculado", "5 dias úteis")
        .Replace("@nomeEquipe", "TI/Patrimônio")
        .Replace("@emailEquipe", "patrimonio@empresa.com")
        .Replace("@telefoneEquipe", "4000")
        .Replace("@nomeEmpresa", cliente.Nome)
        .Replace("@mensagemAdicional", mensagemAdicional ?? "")
        .Replace("@usuarioQueForçou", usuarioLogado.Nome)
        .Replace("@dataForcado", DateTime.Now.ToString("dd/MM/yyyy"));
    
    // Enviar e-mail com o conteúdo processado
    _emailService.EnviarEmail(
        destinatario: colaborador.Email,
        assunto: template.Titulo,
        corpoHtml: conteudoEmail
    );
}
*/

-- =========================================================================
-- VERIFICAÇÃO PÓS-INSERÇÃO:
-- =========================================================================
-- SELECT * FROM templates WHERE tipo = 6 ORDER BY id DESC LIMIT 1;

