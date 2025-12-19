using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace Zynt.Payment.Test;

public class FakeWebHostEnviroment : IWebHostEnvironment
{
    public string ApplicationName { get; set; } = "Zynt.Payment.Test";

    public IFileProvider ContentRootFileProvider { get; set; } = null!;

    public string ContentRootPath { get; set; } = null!;

    public string EnvironmentName { get; set; } = null!;

    public string WebRootPath { get; set; } = null!;

    public IFileProvider WebRootFileProvider { get; set; } = null!;
}