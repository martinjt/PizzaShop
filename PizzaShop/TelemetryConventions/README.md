# Generating Semantic conventions

csharp

```shell
docker run --rm \
        -v $(pwd)/Shared:/output \
        -v $(pwd)/TelemetryConventions:/conventions \
        -v $(pwd)/templates:/templates \
        otel/weaver:latest \
        registry generate csharp \
        --registry=/conventions \
        --templates=/templates \
        /output/
```

markdown

```shell
docker run --rm \
        -v $(pwd)/TelemetryDocs:/output \
        -v $(pwd)/TelemetryConventions:/conventions \
        otel/weaver:latest \
        registry generate markdown \
        --registry=/conventions \
        --templates=https://github.com/open-telemetry/semantic-conventions/archive/refs/tags/v1.33.0.zip\[templates\] \
        /output/
```