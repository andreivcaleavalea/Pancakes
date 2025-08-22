# Build stage
FROM node:18-alpine as build

WORKDIR /app

# Copy package files
COPY package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY . .

# Build-time environment for Vite
ARG VITE_ADMIN_API_URL
ARG VITE_FRONTEND_URL
ENV VITE_ADMIN_API_URL=$VITE_ADMIN_API_URL \
    VITE_FRONTEND_URL=$VITE_FRONTEND_URL

# Build the app
RUN npm run build

# Production stage
FROM nginx:alpine

# Copy built app to nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/nginx.conf

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]