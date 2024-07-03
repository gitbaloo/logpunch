FROM node:20 as build
WORKDIR /punchlog/frontend/

COPY package.json .

RUN npm install

COPY . .

RUN npm run build

FROM nginx:stable-alpine
COPY --from=build /punchlog/frontend/build /usr/share/nginx/html
COPY nginx/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]
