FROM node:24-alpine AS build
WORKDIR /source

COPY src/smart-building-web/package.json src/smart-building-web/package-lock.json ./
RUN npm ci

COPY src/smart-building-web/ ./
RUN npm run build

FROM nginx:1.27-alpine
COPY infrastructure/docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /source/dist/smart-building-web/browser /usr/share/nginx/html
EXPOSE 8080
