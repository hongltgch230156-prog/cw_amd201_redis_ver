# Stage build
FROM node:20-alpine AS build
WORKDIR /app

COPY package*.json ./
RUN npm install

COPY . .

# Truyền biến VITE khi build
ARG VITE_URL_SHORTEN_URL
ARG VITE_CRUD_URL
ARG VITE_IDENTITY_URL

RUN npm run build

# Stage production
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
