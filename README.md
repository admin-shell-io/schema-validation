# schema-validation
![Check](https://github.com/admin-shell-io/schema-validation/workflows/Check/badge.svg)

This repository provides sample programs used to validate the examples in 
[aas-specs](https://github.com/admin-shell-io/aas-specs) repository.

## Binaries

We provide Windows binaries in the [releases](
https://github.com/admin-shell-io/aas-specs/releases).

The validators are based on .NET Core 3.1.

## Build from Source

To build the binaries from source code, change to the `src/` directory
and invoke:

```bash
dotnet publish -c Release -o out/schema-validation
```

We provide a script `src/Release.ps1` that builds and packs the binaries
into a zip archive. This is used to obtain the final binary release.

