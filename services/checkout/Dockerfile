FROM python:3.15-slim@sha256:65a65fc7b30b06cb6df4930fd4650697cb23f4c2698ab81fc87e4b031bc36928

ENV PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    PORT=8083 \
    OTEL_SERVICE_NAME=grots-checkout \
    OTEL_SEMCONV_STABILITY_OPT_IN=http \
    OTEL_PYTHON_LOGGING_AUTO_INSTRUMENTATION_ENABLED=true \
    OTEL_PYTHON_FASTAPI_EXCLUDED_URLS=healthz \
    OTEL_EXPERIMENTAL_RESOURCE_DETECTORS=containerid,host,os,otel,process,service_instance \
    OTEL_PYTHON_SYSTEM_METRICS_EXCLUDED_METRICS="system.*,process.runtime.*,process.context_switches,process.cpu.utilization,process.disk.io,process.open_file_descriptor.count,cpython.*"

WORKDIR /app

COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt

COPY app ./app

RUN useradd --system --no-create-home grot
USER grot

EXPOSE 8083

CMD ["sh", "-c", "exec opentelemetry-instrument uvicorn app.main:app --host 0.0.0.0 --port ${PORT}"]
