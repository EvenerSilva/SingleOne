-- =====================================================
-- SCRIPT PARA ADICIONAR CAPITAIS E PRINCIPAIS CIDADES
-- Complementar as tabelas de estados e cidades
-- =====================================================

-- Adicionar TODAS as capitais brasileiras
INSERT INTO cidades (nome, estado_id) VALUES
-- Acre
('Rio Branco', (SELECT id FROM estados WHERE sigla = 'AC')),

-- Alagoas
('Maceió', (SELECT id FROM estados WHERE sigla = 'AL')),

-- Amapá
('Macapá', (SELECT id FROM estados WHERE sigla = 'AP')),

-- Amazonas
('Manaus', (SELECT id FROM estados WHERE sigla = 'AM')),

-- Bahia
('Salvador', (SELECT id FROM estados WHERE sigla = 'BA')),
('Feira de Santana', (SELECT id FROM estados WHERE sigla = 'BA')),
('Vitória da Conquista', (SELECT id FROM estados WHERE sigla = 'BA')),
('Camaçari', (SELECT id FROM estados WHERE sigla = 'BA')),

-- Ceará
('Fortaleza', (SELECT id FROM estados WHERE sigla = 'CE')),
('Caucaia', (SELECT id FROM estados WHERE sigla = 'CE')),
('Juazeiro do Norte', (SELECT id FROM estados WHERE sigla = 'CE')),

-- Distrito Federal
('Brasília', (SELECT id FROM estados WHERE sigla = 'DF')),

-- Espírito Santo
('Vitória', (SELECT id FROM estados WHERE sigla = 'ES')),
('Vila Velha', (SELECT id FROM estados WHERE sigla = 'ES')),
('Serra', (SELECT id FROM estados WHERE sigla = 'ES')),
('Linhares', (SELECT id FROM estados WHERE sigla = 'ES')),

-- Goiás
('Goiânia', (SELECT id FROM estados WHERE sigla = 'GO')),
('Aparecida de Goiânia', (SELECT id FROM estados WHERE sigla = 'GO')),
('Anápolis', (SELECT id FROM estados WHERE sigla = 'GO')),
('Rio Verde', (SELECT id FROM estados WHERE sigla = 'GO')),

-- Maranhão
('São Luís', (SELECT id FROM estados WHERE sigla = 'MA')),
('Imperatriz', (SELECT id FROM estados WHERE sigla = 'MA')),
('Timon', (SELECT id FROM estados WHERE sigla = 'MA')),

-- Mato Grosso
('Cuiabá', (SELECT id FROM estados WHERE sigla = 'MT')),
('Várzea Grande', (SELECT id FROM estados WHERE sigla = 'MT')),
('Rondonópolis', (SELECT id FROM estados WHERE sigla = 'MT')),

-- Mato Grosso do Sul
('Campo Grande', (SELECT id FROM estados WHERE sigla = 'MS')),
('Dourados', (SELECT id FROM estados WHERE sigla = 'MS')),
('Três Lagoas', (SELECT id FROM estados WHERE sigla = 'MS')),

-- Minas Gerais
('Belo Horizonte', (SELECT id FROM estados WHERE sigla = 'MG')),
('Uberlândia', (SELECT id FROM estados WHERE sigla = 'MG')),
('Contagem', (SELECT id FROM estados WHERE sigla = 'MG')),
('Betim', (SELECT id FROM estados WHERE sigla = 'MG')),
('Montes Claros', (SELECT id FROM estados WHERE sigla = 'MG')),
('Ribeirão das Neves', (SELECT id FROM estados WHERE sigla = 'MG')),
('Uberaba', (SELECT id FROM estados WHERE sigla = 'MG')),
('Governador Valadares', (SELECT id FROM estados WHERE sigla = 'MG')),

-- Pará
('Belém', (SELECT id FROM estados WHERE sigla = 'PA')),
('Ananindeua', (SELECT id FROM estados WHERE sigla = 'PA')),
('Santarém', (SELECT id FROM estados WHERE sigla = 'PA')),
('Castanhal', (SELECT id FROM estados WHERE sigla = 'PA')),

-- Paraíba
('João Pessoa', (SELECT id FROM estados WHERE sigla = 'PB')),
('Campina Grande', (SELECT id FROM estados WHERE sigla = 'PB')),
('Santa Rita', (SELECT id FROM estados WHERE sigla = 'PB')),

-- Paraná
('Curitiba', (SELECT id FROM estados WHERE sigla = 'PR')),
('Londrina', (SELECT id FROM estados WHERE sigla = 'PR')),
('Maringá', (SELECT id FROM estados WHERE sigla = 'PR')),
('Ponta Grossa', (SELECT id FROM estados WHERE sigla = 'PR')),
('Cascavel', (SELECT id FROM estados WHERE sigla = 'PR')),
('São José dos Pinhais', (SELECT id FROM estados WHERE sigla = 'PR')),
('Foz do Iguaçu', (SELECT id FROM estados WHERE sigla = 'PR')),
('Colombo', (SELECT id FROM estados WHERE sigla = 'PR')),

-- Pernambuco
('Recife', (SELECT id FROM estados WHERE sigla = 'PE')),
('Jaboatão dos Guararapes', (SELECT id FROM estados WHERE sigla = 'PE')),
('Olinda', (SELECT id FROM estados WHERE sigla = 'PE')),
('Caruaru', (SELECT id FROM estados WHERE sigla = 'PE')),
('Petrolina', (SELECT id FROM estados WHERE sigla = 'PE')),
('Paulista', (SELECT id FROM estados WHERE sigla = 'PE')),

-- Piauí
('Teresina', (SELECT id FROM estados WHERE sigla = 'PI')),
('Parnaíba', (SELECT id FROM estados WHERE sigla = 'PI')),
('Picos', (SELECT id FROM estados WHERE sigla = 'PI')),

-- Rio de Janeiro
('Rio de Janeiro', (SELECT id FROM estados WHERE sigla = 'RJ')),
('São Gonçalo', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Duque de Caxias', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Nova Iguaçu', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Niterói', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Belford Roxo', (SELECT id FROM estados WHERE sigla = 'RJ')),
('São João de Meriti', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Petrópolis', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Campos dos Goytacazes', (SELECT id FROM estados WHERE sigla = 'RJ')),
('Volta Redonda', (SELECT id FROM estados WHERE sigla = 'RJ')),

-- Rio Grande do Norte
('Natal', (SELECT id FROM estados WHERE sigla = 'RN')),
('Mossoró', (SELECT id FROM estados WHERE sigla = 'RN')),
('Parnamirim', (SELECT id FROM estados WHERE sigla = 'RN')),

-- Rio Grande do Sul
('Porto Alegre', (SELECT id FROM estados WHERE sigla = 'RS')),
('Caxias do Sul', (SELECT id FROM estados WHERE sigla = 'RS')),
('Pelotas', (SELECT id FROM estados WHERE sigla = 'RS')),
('Canoas', (SELECT id FROM estados WHERE sigla = 'RS')),
('Santa Maria', (SELECT id FROM estados WHERE sigla = 'RS')),
('Gravataí', (SELECT id FROM estados WHERE sigla = 'RS')),
('Viamão', (SELECT id FROM estados WHERE sigla = 'RS')),
('Novo Hamburgo', (SELECT id FROM estados WHERE sigla = 'RS')),
('São Leopoldo', (SELECT id FROM estados WHERE sigla = 'RS')),
('Alvorada', (SELECT id FROM estados WHERE sigla = 'RS')),

-- Rondônia
('Porto Velho', (SELECT id FROM estados WHERE sigla = 'RO')),
('Ji-Paraná', (SELECT id FROM estados WHERE sigla = 'RO')),
('Ariquemes', (SELECT id FROM estados WHERE sigla = 'RO')),

-- Roraima
('Boa Vista', (SELECT id FROM estados WHERE sigla = 'RR')),

-- Santa Catarina
('Florianópolis', (SELECT id FROM estados WHERE sigla = 'SC')),
('Joinville', (SELECT id FROM estados WHERE sigla = 'SC')),
('Blumenau', (SELECT id FROM estados WHERE sigla = 'SC')),
('São José', (SELECT id FROM estados WHERE sigla = 'SC')),
('Criciúma', (SELECT id FROM estados WHERE sigla = 'SC')),
('Itajaí', (SELECT id FROM estados WHERE sigla = 'SC')),
('Lages', (SELECT id FROM estados WHERE sigla = 'SC')),
('Jaraguá do Sul', (SELECT id FROM estados WHERE sigla = 'SC')),
('Palhoça', (SELECT id FROM estados WHERE sigla = 'SC')),
('Balneário Camboriú', (SELECT id FROM estados WHERE sigla = 'SC')),

-- São Paulo
('São Paulo', (SELECT id FROM estados WHERE sigla = 'SP')),
('Guarulhos', (SELECT id FROM estados WHERE sigla = 'SP')),
('Campinas', (SELECT id FROM estados WHERE sigla = 'SP')),
('São Bernardo do Campo', (SELECT id FROM estados WHERE sigla = 'SP')),
('Santo André', (SELECT id FROM estados WHERE sigla = 'SP')),
('Osasco', (SELECT id FROM estados WHERE sigla = 'SP')),
('Ribeirão Preto', (SELECT id FROM estados WHERE sigla = 'SP')),
('Sorocaba', (SELECT id FROM estados WHERE sigla = 'SP')),
('Mauá', (SELECT id FROM estados WHERE sigla = 'SP')),
('São José dos Campos', (SELECT id FROM estados WHERE sigla = 'SP')),
('Mogi das Cruzes', (SELECT id FROM estados WHERE sigla = 'SP')),
('Santos', (SELECT id FROM estados WHERE sigla = 'SP')),
('Diadema', (SELECT id FROM estados WHERE sigla = 'SP')),
('Jundiaí', (SELECT id FROM estados WHERE sigla = 'SP')),
('Piracicaba', (SELECT id FROM estados WHERE sigla = 'SP')),
('Carapicuíba', (SELECT id FROM estados WHERE sigla = 'SP')),
('Bauru', (SELECT id FROM estados WHERE sigla = 'SP')),
('Itaquaquecetuba', (SELECT id FROM estados WHERE sigla = 'SP')),
('São Vicente', (SELECT id FROM estados WHERE sigla = 'SP')),
('Franca', (SELECT id FROM estados WHERE sigla = 'SP')),

-- Sergipe
('Aracaju', (SELECT id FROM estados WHERE sigla = 'SE')),
('Nossa Senhora do Socorro', (SELECT id FROM estados WHERE sigla = 'SE')),
('Lagarto', (SELECT id FROM estados WHERE sigla = 'SE')),

-- Tocantins
('Palmas', (SELECT id FROM estados WHERE sigla = 'TO')),
('Araguaína', (SELECT id FROM estados WHERE sigla = 'TO')),
('Gurupi', (SELECT id FROM estados WHERE sigla = 'TO'))
ON CONFLICT DO NOTHING;

-- Verificar total de cidades
SELECT 'TOTAL DE CIDADES:' as info, COUNT(*) as quantidade FROM cidades;

-- Mostrar cidades por estado
SELECT 
    e.sigla as estado,
    e.nome as nome_estado,
    COUNT(c.id) as total_cidades
FROM estados e
LEFT JOIN cidades c ON e.id = c.estado_id
GROUP BY e.id, e.sigla, e.nome
ORDER BY e.nome;
