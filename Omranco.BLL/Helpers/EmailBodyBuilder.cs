namespace Omranco.BLL;

public static class EmailBodyBuilder
{
    public static async Task<string> GenerateEmailBodyAsync(string template, Dictionary<string, string> templateModel)
    {
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"{template}.html");

        if (!File.Exists(templatePath))
            throw new FileNotFoundException($"Email template not found at {templatePath}");


        var body = await File.ReadAllTextAsync(templatePath);

        foreach (var item in templateModel)
            body = body.Replace(item.Key, item.Value);

        return body;
    }
}