using System.Collections.Concurrent;

namespace MyNUnitWebSolution.Pages;

using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyNUnitSolution;

public class Index : PageModel
{
    private readonly IWebHostEnvironment env;

    public Index(IWebHostEnvironment env)
    {
        this.env = env;
        this.ThePathToTheUploadedFilesDirectory = Path.Combine(this.env.ContentRootPath, "uploadedFiles");

        Directory.CreateDirectory(this.ThePathToTheUploadedFilesDirectory);

        this.filesNames = Directory
            .GetFiles(this.ThePathToTheUploadedFilesDirectory)
            .Select(Path.GetFileName)
            .ToList();
    }

    public string ThePathToTheUploadedFilesDirectory { get; private set; }

    [BindProperty]
    public IFormFile UploadFile { get; set; }

    public List<string?> filesNames = [];

    public string Message { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPost()
    {
        var (success, message, _) = await HandleUploadAsync();
        this.Message = message;
        return this.Page();
    }

    public async Task<IActionResult> OnPostUploadFile()
    {
        var (success, message, storedFileName) = await HandleUploadAsync();

        return new JsonResult(new
        {
            success,
            message,
            fileName = storedFileName,
        });
    }

    public async Task<IActionResult> OnPostRunTheTests()
    {
        var results = new ConcurrentDictionary<string, ConcurrentBag<string>>();

        try
        {
            await Parallel.ForEachAsync(
                Directory.GetFiles(this.ThePathToTheUploadedFilesDirectory),
                async (thePathToTheAssembly, _) =>
                {
                    var myNUnit = new MyNUnit();
                    var assemblyName = Path.GetFileName(thePathToTheAssembly);

                    var assemblyResults =
                        await myNUnit.RunAllTheTestsAlongThisPath(thePathToTheAssembly);

                    results[assemblyName] = assemblyResults;
                });

            var dto = results.Select(kvp => new
            {
                assemblyName = kvp.Key,
                output = kvp.Value.ToArray(),
            });

            return new JsonResult(new
            {
                success = true,
                data = dto,
            });
        }
        catch (Exception ex)
        {
            return this.BadRequest(new
            {
                success = false,
                error = ex.Message,
            });
        }
    }


    public IActionResult OnPostDeleteFile(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return this.BadRequest();
        }

        fileName = Path.GetFileName(fileName);

        var filePath = Path.Combine(this.ThePathToTheUploadedFilesDirectory, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return this.NotFound();
        }

        System.IO.File.Delete(filePath);

        this.filesNames.Remove(fileName);

        return new JsonResult(new { success = true });
    }

    private static string GetStreamHash(Stream stream)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    private async Task<(bool Success, string Message, string? StoredFileName)> HandleUploadAsync()
    {
        if (this.UploadFile is null || this.UploadFile.Length == 0)
        {
            return (false, "Please select a file to upload", null);
        }

        Directory.CreateDirectory(this.ThePathToTheUploadedFilesDirectory);

        var originalFileName = Path.GetFileNameWithoutExtension(this.UploadFile.FileName);
        var extension = Path.GetExtension(this.UploadFile.FileName);

        var targetFileName = this.UploadFile.FileName;
        var filePath = Path.Combine(this.ThePathToTheUploadedFilesDirectory, targetFileName);

        if (System.IO.File.Exists(filePath))
        {
            await using var stream1 = System.IO.File.OpenRead(filePath);
            await using var stream2 = this.UploadFile.OpenReadStream();

            if (GetStreamHash(stream1) == GetStreamHash(stream2))
            {
                return (false, "The file has already been uploaded", null);
            }

            var counter = 1;
            do
            {
                targetFileName = $"{originalFileName} ({counter}){extension}";
                filePath = Path.Combine(this.ThePathToTheUploadedFilesDirectory, targetFileName);
                counter++;
            }
            while (System.IO.File.Exists(filePath));
        }

        await this.SaveUploadedFile(filePath);

        return (true, $"File \"{targetFileName}\" successfully uploaded", targetFileName);
    }

    private async Task SaveUploadedFile(string path)
    {
        await using var stream = System.IO.File.Create(path);
        await this.UploadFile.CopyToAsync(stream);

        this.filesNames.Add(Path.GetFileName(path));
    }
}