
public class Settings
{
    public AzureOpenAI AzureOpenAI { get; set; }
    public Travily Travily { get; set; }
}
public class AzureOpenAI
{
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string Deployment { get; set; } = string.Empty;
}
public class Travily
{
    public string ApiKey { get; set; } = string.Empty;
}