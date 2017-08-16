## Installation 

Add the tool to your `csproj` file:

```
<ItemGroup>
    <DotNetCliToolReference Include="Dotnet.exec" Version="0.1.0-*" />
</ItemGroup>
```

## Running

```
dotnet exec MyAssembly "Namespace.Class" Method '{"key1": "value1"}'
```

### TODO

- Instantiation of generics
- Parse serverless.yml