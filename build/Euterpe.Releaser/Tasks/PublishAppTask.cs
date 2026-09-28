using Cake.Common.Tools.DotNet;
using Cake.Common.Tools.DotNet.MSBuild;
using Cake.Common.Tools.DotNet.Publish;

namespace Euterpe.Releaser.Tasks;

[TaskName("PublishApp")]
[IsDependentOn(typeof(CleanTask))]
public sealed class PublishAppTask : FrostingTask<ReleaseContext>
{
    public override bool ShouldRun(ReleaseContext context) => !context.IsVersionPublished;

    public override void Run(ReleaseContext context)
    {
        context.DotNetPublish(context.ApplicationProject, new DotNetPublishSettings
        {
            Configuration = "Release",
            Runtime = context.Rid,
            OutputDirectory = context.ApplicationDirectory,
            MSBuildSettings = new DotNetMSBuildSettings()
                .WithProperty("MinVerVersionOverride", context.Version.ToString())
        });
    }
}
