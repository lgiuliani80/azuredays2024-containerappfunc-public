namespace NetIsolated8FFMpeg;

using FFMpegCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

public class GenerateGif
{
    private readonly ILogger<GenerateGif> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public GenerateGif(ILogger<GenerateGif> logger, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    [Function(nameof(GenerateGif))]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
    {
        var inputFile = req.Query["inputFile"].FirstOrDefault();

        if (inputFile == null)
        {
            return new BadRequestObjectResult(new
            {
                error = "Please pass a name on the query string or in the request body"
            });
        }

        var tmpFileInput = Path.Combine(Path.GetTempPath(), $"in-{Guid.NewGuid()}.mp4");

        _logger.LogInformation("Starting downloading {url} ...", inputFile);

        using (var fileStream = File.Create(tmpFileInput))
        {
            var st = await _httpClientFactory.CreateClient().GetStreamAsync(inputFile);
            await st.CopyToAsync(fileStream);
        }

        _logger.LogInformation("Downloaded {url} to {tmpFileInput}", inputFile, tmpFileInput);

        var tmpFileOutput = Path.Combine(Path.GetTempPath(), $"out-{Guid.NewGuid()}.gif");
        byte[]? outBytes = null;

        try
        {
            _logger.LogInformation("Converting {inputFile} to GIF", inputFile);
            FFMpeg.GifSnapshot(tmpFileInput, tmpFileOutput, new System.Drawing.Size(200, 112), TimeSpan.FromSeconds(2));
            outBytes = File.ReadAllBytes(tmpFileOutput);
        } 
        finally
        {
            File.Delete(tmpFileInput);
            File.Delete(tmpFileOutput);
            _logger.LogInformation("Deleted temporary file {tmpFile}", tmpFileOutput);
        }

        _logger.LogInformation("Conversion completed: {inputFile} -> {outputFile} ({finalSize})", inputFile, tmpFileOutput, outBytes.Length);

        return new FileContentResult(outBytes, "image/gif");
    }
}
