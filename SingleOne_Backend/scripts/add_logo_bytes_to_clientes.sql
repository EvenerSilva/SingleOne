ALTER TABLE clientes ADD COLUMN IF NOT EXISTS logo_bytes bytea;
ALTER TABLE clientes ADD COLUMN IF NOT EXISTS logo_content_type varchar(100);
