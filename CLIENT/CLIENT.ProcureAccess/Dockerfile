# stage 1: build angular client
FROM node:22-alpine AS build
WORKDIR /app
RUN npm install -g @angular/cli
COPY package*.json ./
RUN npm ci
# --omit=dev
COPY . ./
RUN npm run build --configuration=development
# =production

# stage 2: serve with nginx
FROM nginx:stable-alpine AS final
EXPOSE 80
COPY --from=build /app/dist/procure-access/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
