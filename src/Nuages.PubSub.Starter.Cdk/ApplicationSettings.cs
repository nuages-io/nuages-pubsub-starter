namespace Nuages.PubSub.Starter.Cdk;

public class ApplicationSettings
{
    public string StackName { get; set; } = string.Empty;
    
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

    public string? DataStorage { get; set; }
    public string? DataConnectionString { get; set; }
    
    // ReSharper disable once InconsistentNaming
    public CDKPipeline CDKPipeline { get; set; } = new();

    public Auth Auth { get; set; } = new();
}

public class ExternalAuth
{
    public bool Enabled { get; set; }
    public string ValidIssuers { get; set; } = "";
    public string? ValidAudiences { get; set; }
    public string JsonWebKeySetUrlPath { get; set; } = ".well-known/jwks";
    public bool? DisableSslCheck { get; set; }
}

public class InternalAuth
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public string? Secret { get; set; }
}

public class Auth
{
    public InternalAuth InternalAuth { get; set; } = new();
    public ExternalAuth ExternalAuth { get; set; } = new();
}

// ReSharper disable once InconsistentNaming
public class CDKPipeline
{
    public string GitHubRepository { get; set; } = string.Empty;
    public string GithubToken { get; set; } = string.Empty;
    public string? NotificationTargetArn { get; set; }
}


