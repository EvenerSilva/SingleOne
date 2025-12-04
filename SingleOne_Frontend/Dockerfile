# ==========================================
# Dockerfile - SingleOne Frontend
# Angular 10+ com Nginx
# ==========================================

# Stage 1: Build
FROM node:14-alpine AS build
WORKDIR /app

# Copiar package.json e package-lock.json
COPY package*.json ./

# Instalar dependências
RUN npm install --legacy-peer-deps

# Copiar código fonte
COPY . .

# Build de produção
RUN npm run build -- --configuration=production

# Stage 2: Nginx
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Remover arquivos padrão do nginx
RUN rm -rf ./*

# Copiar build do Angular
COPY --from=build /app/dist/SingleOne .

# Copiar configuração customizada do nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copiar entrypoint.sh e torná-lo executável
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# Expor porta 80
EXPOSE 80

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost/ || exit 1

# Usar entrypoint.sh para injetar variáveis de ambiente
ENTRYPOINT ["/entrypoint.sh"]
CMD ["nginx", "-g", "daemon off;"]
