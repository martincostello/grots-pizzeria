FROM node:26-slim@sha256:e6bedef217677b276c591795e169840edd9096dbf13f29df8ef8d1856b8820bc AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --omit=dev

FROM node:26-slim@sha256:e6bedef217677b276c591795e169840edd9096dbf13f29df8ef8d1856b8820bc
WORKDIR /app
ENV NODE_ENV=production \
    PORT=8081 \
    OTEL_SERVICE_NAME=grots-menu \
    OTEL_SEMCONV_STABILITY_OPT_IN=http
COPY --from=deps /app/node_modules ./node_modules
COPY package.json instrumentation.js ./
COPY src ./src
COPY public ./public
USER node
EXPOSE 8081
CMD ["node", "--import", "./instrumentation.js", "src/server.js"]
