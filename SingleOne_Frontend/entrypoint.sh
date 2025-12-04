#!/bin/sh
# ============================================
# Entrypoint - SingleOne Frontend
# Injeta variáveis de ambiente no runtime
# ============================================

# Substituir variáveis de ambiente no env.js
cat > /usr/share/nginx/html/assets/env.js << EOF
(function(window) {
  window.__env = window.__env || {};
  window.__env.apiUrl = "${API_URL:-http://localhost:5000/api/}";
}(this));
EOF

echo "🚀 Frontend configurado com API_URL: ${API_URL:-http://localhost:5000/api/}"

# Executar comando passado (nginx)
exec "$@"
