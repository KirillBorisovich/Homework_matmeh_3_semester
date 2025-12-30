// <copyright file="Index.cshtml.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace MyNUnitWebSolution.Pages;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MyNUnitSolution;
using MyNUnitWebSolution.Data;

/// <summary>
/// Page model for the main MyNUnit web UI page.
/// </summary>
public class Index : PageModel
{
    private readonly IWebHostEnvironment env;
    private readonly TestHistoryContext db;

    /// <summary>
    /// Initializes a new instance of the <see cref="Index"/> class.
    /// </summary>
    /// <param name="env">The web hosting environment that provides access to the content root path
    /// and other environment-specific information.</param>
    /// <param name="db">The EF Core context used to
    /// store and read the test execution history.</param>
    public Index(IWebHostEnvironment env, TestHistoryContext db)
    {
        this.env = env;
        this.db = db;
        this.ThePathToTheUploadedFilesDirectory = Path.Combine(this.env.ContentRootPath, "uploadedFiles");

        Directory.CreateDirectory(this.ThePathToTheUploadedFilesDirectory);

        this.FilesNames = Directory
            .GetFiles(this.ThePathToTheUploadedFilesDirectory)
            .Select(Path.GetFileName)
            .ToList();
    }

    /// <summary>
    /// Gets the path to the folder with uploaded builds.
    /// </summary>
    public string ThePathToTheUploadedFilesDirectory { get; private set; }

    /// <summary>
    /// Gets or sets uploaded file.
    /// </summary>
    [BindProperty]
    public IFormFile? UploadFile { get; set; }

    /// <summary>
    /// Gets names of uploaded files.
    /// </summary>
    public List<string?> FilesNames { get; private set; }

    /// <summary>
    /// Gets message.
    /// </summary>
    public string? Message { get; private set; }

    /// <summary>
    /// A method that responds to a request with a type of post.
    /// </summary>
    /// <returns>An existing page.</returns>
    public async Task<IActionResult> OnPost()
    {
        var (_, message, _) = await this.HandleUploadAsync();
        this.Message = message;
        return this.Page();
    }

    /// <summary>
    /// The method for uploading files.
    /// </summary>
    /// <returns>Json.</returns>
    public async Task<IActionResult> OnPostUploadFile()
    {
        var (success, message, storedFileName) = await this.HandleUploadAsync();

        return new JsonResult(new
        {
            success,
            message,
            fileName = storedFileName,
        });
    }

    /// <summary>
    /// A method for running tests from uploaded assemblies.
    /// </summary>
    /// <returns>Json.</returns>
    public async Task<IActionResult> OnPostRunTheTests()
    {
        try
        {
            var dllPaths = Directory.GetFiles(this.ThePathToTheUploadedFilesDirectory, "*.dll");

            if (dllPaths.Length == 0)
            {
                return new JsonResult(new
                {
                    success = false,
                    error = "No assemblies to test",
                });
            }

            var allResults = new ConcurrentDictionary<string, ConcurrentBag<string>>();

            await Parallel.ForEachAsync(
                dllPaths,
                async (assemblyPath, _) =>
                {
                    var myNUnit = new MyNUnit();
                    var assemblyName = Path.GetFileName(assemblyPath);

                    var output = await myNUnit.RunAllTheTestsAlongThisPath(assemblyPath);
                    allResults[assemblyName] = output;
                });

            var run = new TestRun
            {
                StartedAt = DateTime.UtcNow,
                FinishedAt = DateTime.UtcNow,
            };

            foreach (var (assemblyName, linesBag) in allResults.OrderBy(k => k.Key))
            {
                var asmRun = new AssemblyRun
                {
                    AssemblyName = assemblyName,
                };

                var order = 0;
                foreach (var line in linesBag)
                {
                    asmRun.Lines.Add(new TestOutputLine
                    {
                        Order = order++,
                        Text = line,
                    });
                }

                run.Assemblies.Add(asmRun);
            }

            this.db.TestRuns.Add(run);
            await this.db.SaveChangesAsync();

            var uiData = allResults
                .OrderBy(k => k.Key)
                .Select(kvp => new
                {
                    assemblyName = kvp.Key,
                    output = kvp.Value.ToArray(),
                })
                .ToArray();

            return new JsonResult(new
            {
                success = true,
                data = uiData,
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

    /// <summary>
    /// A method for deleting uploaded files.
    /// </summary>
    /// <param name="fileName">The name of the file being deleted.</param>
    /// <returns>Json.</returns>
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

        this.FilesNames.Remove(fileName);

        return new JsonResult(new { success = true });
    }

    private static string GetStreamHash(Stream stream)
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(stream));
    }

    private async Task<(bool Success, string Message, string? StoredFileName)> HandleUploadAsync()
    {
        if (this.UploadFile == null || this.UploadFile.Length == 0)
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
        await this.UploadFile!.CopyToAsync(stream);

        this.FilesNames.Add(Path.GetFileName(path));
    }
}