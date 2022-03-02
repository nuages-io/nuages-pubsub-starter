using Nuages.Web;

namespace Nuages.PubSub.Starter.Cdk;

public class ApplicationSettings
{
    public string StackName { get; set; } = string.Empty;
    
    public ApplicationConfig ApplicationConfig { get; set; } = new();
    
    public string? WebSocketDomainName { get; set; }
    public string? WebSocketCertificateArn { get; set; }
    public string? ApiDomainName { get; set; }
    public string? ApiCertificateArn { get; set; }
    public string? ApiApiKey { get; set; }
    public string? VpcId { get; set; }
    public string? SecurityGroupId { get; set; }
    public string? DatabaseProxyArn { get; set; }
    public string? DatabaseProxyEndpoint { get; set; }
    public string? DatabaseProxyName { get; set; }
    public string? DatabaseProxyUser { get; set; }
}

