# stage 1: build angular client
FROM node:22.20.0-slim AS build
WORKDIR /app
RUN npm install -g @angular/cli
COPY package*.json ./
RUN npm ci
COPY . ./
RUN ng build --configuration=development

# stage 2: serve with nginx
FROM nginx:stable AS final
EXPOSE 80
COPY --from=build /app/dist/procure-access/browser  /usr/share/nginx/html
