using Aspire.Hosting.Eventing;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting;

//// HACK Workaround for https://github.com/microsoft/aspire/issues/20709

internal static class SingleThreadedBuildExtensions
{
    public static IResourceBuilder<T> WithSingleThreadedBuild<T>(this IResourceBuilder<T> project)
        where T : IResource
    {
        var builder = project.ApplicationBuilder;

        if (builder.ExecutionContext.IsPublishMode)
        {
            return project;
        }

        builder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
        {
            builder.Eventing.Subscribe<BeforeResourceStartedEvent>((@event, _) =>
            {
                if (@event.Resource is ExecutableResource { Command: "dotnet" } executable && IsBuildOf(project.Resource, executable))
                {
                    lock (executable.Annotations)
                    {
                        if (!executable.Annotations.OfType<SingleThreadedBuildAnnotation>().Any())
                        {
                            executable.Annotations.Add(new SingleThreadedBuildAnnotation());
                        }
                    }
                }

                return Task.CompletedTask;
            });

            return Task.CompletedTask;
        });

        return project;
    }

    private static bool IsBuildOf(IResource project, ExecutableResource executable) =>
        (executable is IResourceWithParent parent && ReferenceEquals(parent.Parent, project)) ||
        project.Annotations.OfType<WaitAnnotation>().Any((p) => p.WaitType == WaitType.WaitForCompletion && ReferenceEquals(p.Resource, executable));

    private sealed class SingleThreadedBuildAnnotation() : CommandLineArgsCallbackAnnotation(context =>
    {
        if (context.Args is ["build", ..] && context.Args.Remove("-mt"))
        {
            context.Logger.LogInformation(
                "Disabled multithreaded MSBuild for {ResourceName} as the Blazor WebAssembly SDK does not support it.",
                context.Resource.Name);
        }

        return Task.CompletedTask;
    });
}
