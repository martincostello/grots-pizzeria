# Build context: services/orders
FROM eclipse-temurin:25-jdk@sha256:ce2ef9d2d67a68de5e196d4de9d743327c44a10b57fe02ec39f572c652cacfd3 AS build
WORKDIR /src

# Restore dependencies (including the OpenTelemetry Java agent) in their own layer.
COPY mvnw pom.xml ./
COPY .mvn .mvn
RUN chmod +x mvnw && ./mvnw -B -q dependency:go-offline

COPY src src
RUN ./mvnw -B -DskipTests package

FROM eclipse-temurin:25-jre@sha256:4ed132f6832e9804b123e89a849a7b4502756ae64a4ee08ebab939d9d5d0a9fa
WORKDIR /app

# curl is used by the container health check
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/* \
    && useradd --system --uid 10001 --no-create-home grot

COPY --from=build /src/target/app.jar app.jar
COPY --from=build /src/target/otel/opentelemetry-javaagent.jar opentelemetry-javaagent.jar
COPY --from=build /src/target/app-health-check-sampler.jar health-check-sampler.jar
USER 10001

ENV PORT=8082 \
    OTEL_SERVICE_NAME=grots-orders \
    OTEL_JAVAAGENT_EXTENSIONS=/app/health-check-sampler.jar \
    OTEL_TRACES_SAMPLER=grots_parentbased_always_on \
    OTEL_JAVA_EXPERIMENTAL_SPAN_ATTRIBUTES_COPY_FROM_BAGGAGE_INCLUDE=grot.ui.action
EXPOSE 8082

ENTRYPOINT ["java", "-javaagent:/app/opentelemetry-javaagent.jar", "-jar", "/app/app.jar"]
