FROM node:20 AS base
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
RUN chmod +x node_modules/.bin/* && \
    find node_modules -name "*.bin" -type d -exec chmod +x {}/* \; 2>/dev/null || true && \
    find node_modules -name "esbuild" -type f -exec chmod +x {} \; 2>/dev/null || true
EXPOSE 5173
CMD ["npm", "run", "dev"]