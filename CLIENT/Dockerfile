# stage 1: build angular client
FROM node:22 AS build
WORKDIR /app
COPY ./CLIENT.ProcureAccess/package*.json ./
RUN npm ci
COPY ./CLIENT.ProcureAccess/ .
RUN npm run build --omit=dev

# stage 2: serve with nginx
FROM nginx:stable-alpine
COPY --from=build /app/dist/procure-access/browser /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
