class CliOptions
{
    public CliOptions(string assembly, string className, string method, string data)
    {
        AssemblyName = assembly;
        FullyQualifiedClassName = className;
        MethodName = method;
        Data = data;
    }
    public string AssemblyName { get; set; }
    public string FullyQualifiedClassName { get; set; }
    public string MethodName { get; set; }
    public string Data { get; set; }
}