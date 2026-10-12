namespace Omranco.BLL.Services.Abstractions.FileServices;

public interface IFileStorageService
{
    Task<string> SaveAsync(
        Stream stream,
        string originalFileName,
        string folder);

    Task DeleteAsync(
        string filePath);
}