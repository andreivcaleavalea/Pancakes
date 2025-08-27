###############################
# Frontend Production Dockerfile
# - Multi-stage build (Node build -> Nginx serve)
# - Consumes VITE_* build args (passed at docker build)
# - Outputs static assets to dist and serves via Nginx on port 80
###############################

# ---- Build stage ----
FROM node:20-alpine AS build
WORKDIR /app

# Accept Vite build-time variables (optional defaults to empty)
ARG VITE_USER_API_URL
ARG VITE_BLOG_API_URL
ARG VITE_API_BASE_URL
ARG VITE_GOOGLE_CLIENT_ID
ARG VITE_GITHUB_CLIENT_ID

ENV VITE_USER_API_URL=${VITE_USER_API_URL} \
    VITE_BLOG_API_URL=${VITE_BLOG_API_URL} \
    VITE_API_BASE_URL=${VITE_API_BASE_URL} \
    VITE_GOOGLE_CLIENT_ID=${VITE_GOOGLE_CLIENT_ID} \
    VITE_GITHUB_CLIENT_ID=${VITE_GITHUB_CLIENT_ID}

COPY package*.json ./
RUN npm ci --no-audit --no-fund

COPY . .

# Build production bundle
RUN npx --yes vite build

# ---- Runtime stage ----
FROM nginx:alpine AS runtime

# Copy custom nginx config (provides SPA fallback)
COPY nginx.conf /etc/nginx/nginx.conf

# Copy built assets
COPY --from=build /app/dist /usr/share/nginx/html

# (Optional) Basic health endpoint
RUN printf 'HTTP/1.1 200 OK\nContent-Type: text/plain\n\nhealthy' > /tmp/health && \
    mkdir -p /usr/share/nginx/html/__health && \
    cp /tmp/health /usr/share/nginx/html/__health/index.txt

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]