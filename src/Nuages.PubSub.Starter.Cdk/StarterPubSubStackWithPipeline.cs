using System.Diagnostics.CodeAnalysis;
using Amazon.CDK;
using Amazon.CDK.AWS.Chatbot;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeStarNotifications;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.Pipelines;
using Constructs;
using Microsoft.Extensions.Configuration;

// ReSharper disable ObjectCreationAsStatement

namespace Nuages.PubSub.Starter.Cdk;

[SuppressMessage("Performance", "CA1806:Do not ignore method results")]
public class StarterPubSubStackWithPipeline : Stack
{
    public static void Create(Construct scope, IConfiguration configuration, ApplicationSettings applicationSettings)
    {
        // ReSharper disable once ObjectCreationAsStatement
        new StarterPubSubStackWithPipeline(scope, $"{applicationSettings.StackName}-PipelineStack", configuration, applicationSettings, new StackProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        });
    }

    private StarterPubSubStackWithPipeline(Construct scope, string id, IConfiguration configuration, ApplicationSettings applicationSettings, 
                IStackProps props) : base(scope, id, props)
    {

        var pipeline = new CodePipeline(this, "pipeline", new CodePipelineProps
        {
            PipelineName = $"{applicationSettings.StackName}-Pipeline",
            SynthCodeBuildDefaults = new CodeBuildOptions
            {
                RolePolicy = new PolicyStatement[]
                {
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "route53:*"
                        },
                        Resources = new[] { "*" }
                    }),
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration" },
                        Resources = new[] { "*" }
                    })
                }
            },
            Synth = new ShellStep("Synth",
                new ShellStepProps
                {
                    Input = CodePipelineSource.GitHub(applicationSettings.CDKPipeline.GitHubRepository,
                        "master",
                        new GitHubSourceOptions
                        {
                            Authentication = SecretValue.PlainText(applicationSettings.CDKPipeline.GithubToken),
                            Trigger = GitHubTrigger.WEBHOOK
                        }),
                    Commands = new []
                    {
                        "npm install -g aws-cdk",
                        "cdk synth"
                    }
                }),
            CodeBuildDefaults = new CodeBuildOptions
            {
                BuildEnvironment = null,
                PartialBuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                {
                    {
                        "phases", new Dictionary<string, object>
                        {
                            {
                                "install", new Dictionary<string, object>
                                {
                                    { "commands", new [] { "/usr/local/bin/dotnet-install.sh --channel LTS"} }
                                }
                            }
                        }
                    }
                }),
                RolePolicy = new PolicyStatement[]
                {
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[]
                        {
                            "route53:*"
                        },
                        Resources = new[] { "*" }
                    }),
                    new (new PolicyStatementProps
                    {
                        Effect = Effect.ALLOW,
                        Actions = new[] { "ssm:GetParametersByPath", "appconfig:GetConfiguration" },
                        Resources = new[] { "*" }
                    })
                }
            }
        });
            
        pipeline.AddStage(new PipelineAppStage(this, "Deploy", configuration, applicationSettings, new StageProps
        {
            Env = new Amazon.CDK.Environment
            {
                Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
            }
        }));

        pipeline.BuildPipeline();
        
        var arn = applicationSettings.CDKPipeline.NotificationTargetArn;

        if (!string.IsNullOrEmpty(arn))
        {
            INotificationRuleTarget? target;
            
            if (arn.StartsWith("arn:aws:chatbot"))
                target = SlackChannelConfiguration.FromSlackChannelConfigurationArn(this, "SlackChannel", arn);
            else
            {
                target = Topic.FromTopicArn(this, "SNSTopic", arn);
            }
            
            new NotificationRule(this, "Notification", new NotificationRuleProps
            {
                Events = new []
                {
                    "codepipeline-pipeline-pipeline-execution-failed",
                    "codepipeline-pipeline-pipeline-execution-canceled",
                    "codepipeline-pipeline-pipeline-execution-started",
                    "codepipeline-pipeline-pipeline-execution-resumed",
                    "codepipeline-pipeline-pipeline-execution-succeeded",
                    "codepipeline-pipeline-pipeline-execution-superseded"
                },
                Source = pipeline.Pipeline,
                Targets = new []
                {
                    target
                },
                DetailType = DetailType.BASIC,
                NotificationRuleName = pipeline.Pipeline.PipelineName
            });
        }
    }

    private class PipelineAppStage : Stage
    {
        public PipelineAppStage(Construct scope, string id, IConfiguration configuration, ApplicationSettings applicationSettings, 
            IStageProps props)  : base(scope, id, props)
        {
            StarterPubSubStack.CreateStack(this, configuration, applicationSettings);
        }
    }
}