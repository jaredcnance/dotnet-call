## Installation 

Add the tool to your `csproj` file:

```
<ItemGroup>
    <DotNetCliToolReference Include="dotnet-exec" Version="0.1.0-*" />
</ItemGroup>
```

## Running

```
dotnet exec -a MyAssembly -c "Namespace.Class" -m Method -d '{"key1": "value1"}'
```

### TODO

- Instantiation of generics
- Parse serverless.yml