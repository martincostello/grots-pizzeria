# Build context: services/orders
FROM eclipse-temurin:25-jdk@sha256:884a2f8692cc1e2c7e21f95914adf0ae0eafee9a7eb663a12169cc44468256ec AS build
WORKDIR /src

# Restore dependencies (including the OpenTelemetry Java agent) in their own layer.
COPY mvnw pom.xml ./
COPY .mvn .mvn
RUN chmod +x mvnw && ./mvnw -B -q dependency:go-offline

COPY src src
RUN ./mvnw -B -DskipTests package

FROM eclipse-temurin:25-jre@sha256:945247f9d092703e0c6be92243ef3c51640229e23ab34d353c8dcdc73e6cd4e6
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
