using Microsoft.AspNetCore.Hosting;
using Omranco.BLL.Services.Abstractions.FileServices;

namespace Omranco.PL.Services;

public sealed class LocalFileStorageService
    : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;


    public LocalFileStorageService(
        IWebHostEnvironment environment)
    {
        _environment = environment;
    }


    public async Task<string> SaveAsync(
        Stream stream,
        string originalFileName,
        string folder)
    {
        var extension =
            Path.GetExtension(
                originalFileName
            );


        var fileName =
            $"{Guid.NewGuid():N}{extension}";


        var webRoot =
            _environment.WebRootPath;


        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot =
                Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );
        }


        var directory =
            Path.Combine(
                webRoot,
                folder
                    .Replace(
                        "/",
                        Path.DirectorySeparatorChar
                            .ToString()
                    )
            );


        Directory.CreateDirectory(
            directory
        );


        var fullPath =
            Path.Combine(
                directory,
                fileName
            );


        await using var output =
            new FileStream(
                fullPath,
                FileMode.Create
            );


        await stream.CopyToAsync(
            output
        );


        return
            $"/{folder.Trim('/')}/{fileName}";
    }


    public Task DeleteAsync(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            return Task.CompletedTask;
        }


        var webRoot =
            _environment.WebRootPath;


        if (string.IsNullOrWhiteSpace(webRoot))
        {
            webRoot =
                Path.Combine(
                    _environment.ContentRootPath,
                    "wwwroot"
                );
        }


        var relative =
            filePath.TrimStart('/')
                .Replace(
                    "/",
                    Path.DirectorySeparatorChar
                        .ToString()
                );


        var fullPath =
            Path.Combine(
                webRoot,
                relative
            );


        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }


        return Task.CompletedTask;
    }
}