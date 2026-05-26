# Data Manager

## Build & Publish

### Local build
```
dotnet restore
dotnet clean
dotnet build
```

### Publish (Release, single-file, self-contained)
```
dotnet publish .\"Data Manager.csproj" -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:EnableCompressionInSingleFile=true
```

### Output location
- Publish output: `bin\Release\net10.0-windows\win-x64\publish\`
