FROM node:26-slim@sha256:abe7e5a82b61a8ef67b315b85497c9e65173387ccf2f4a029248184b8e020be5 AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci --omit=dev

FROM node:26-slim@sha256:abe7e5a82b61a8ef67b315b85497c9e65173387ccf2f4a029248184b8e020be5
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
