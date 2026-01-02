-- Script para corrigir view EquipamentoHistoricoVM adicionando coluna equipamentoid
-- Executa: sudo -u postgres psql -d singleone -f corrigir_view_equipamentohistorico.sql

-- Recriar view com equipamentoid
DROP VIEW IF EXISTS EquipamentoHistoricoVM CASCADE;

CREATE OR REPLACE VIEW EquipamentoHistoricoVM AS
SELECT 
    e.id, 
    e.id AS equipamentoid,  -- ✅ ADICIONADO: equipamentoid para filtrar por equipamento
    te.id AS TipoequipamentoID, 
    te.descricao AS TipoEquipamento, 
    f.Id AS FabricanteId, 
    f.Descricao AS Fabricante, 
    m.Id AS ModeloId, 
    m.Descricao AS Modelo, 
    e.NumeroSerie, 
    e.Patrimonio, 
    es.Id AS EquipamentoStatusId, 
    es.Descricao AS EquipamentoStatus, 
    c.Id AS ColaboradorId, 
    c.Nome AS Colaborador, 
    eh.DtRegistro,
    u.Id AS UsuarioId, 
    u.Nome AS Usuario,
    NULL::integer AS tecnicoresponsavelid,  -- ✅ ADICIONADO: compatibilidade com model C#
    NULL::varchar AS tecnicoresponsavel     -- ✅ ADICIONADO: compatibilidade com model C#
FROM EquipamentoHistorico eh
    JOIN Equipamentos e ON eh.Equipamento = e.Id
    JOIN Usuarios u ON eh.Usuario = u.Id
    JOIN EquipamentosStatus es ON eh.EquipamentoStatus = es.Id
    JOIN Fabricantes f ON e.Fabricante = f.Id
    JOIN Modelos m ON e.Modelo = m.Id
    JOIN TipoEquipamentos te ON e.tipoequipamento = te.id
    LEFT JOIN Colaboradores c ON eh.Colaborador = c.Id;

-- Verificar se foi criada corretamente
SELECT column_name, data_type 
FROM information_schema.columns 
WHERE table_name = 'equipamentohistoricovm' 
  AND column_name = 'equipamentoid';

