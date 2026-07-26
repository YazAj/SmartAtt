using AttendAI.Application.FaceRecognition;
using AttendAI.Infrastructure.Configuration;
using AttendAI.Infrastructure.FaceRecognition;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

var parsed = Arguments.Parse(args);
if (parsed.ShowHelp)
{
    PrintUsage();
    return 2;
}

var contentRoot = parsed.ContentRoot ?? Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "AttendAI.Web"));
var options = new FaceRecognitionOptions
{
    Provider = "Real",
    RealEngine = new RealFaceRecognitionOptions
    {
        CosineSimilarityThreshold = parsed.Threshold ?? 0.363
    }
};

var environment = new PocHostEnvironment(Path.GetFullPath(contentRoot));
var wrappedOptions = Options.Create(options);
var store = new RealFaceRecognitionModelStore(wrappedOptions, environment);
var readiness = store.CheckReadiness();

PrintReadiness(readiness, options.RealEngine);

if (!readiness.Succeeded)
{
    Console.WriteLine("Real model-dependent image evaluation: Not Verified.");
    return 1;
}

if (parsed.ModelsOnly || parsed.Commands.Count == 0)
{
    Console.WriteLine("Authorized sample evaluation: Not Verified. Provide explicit local image paths to run it.");
    return 0;
}

using var engine = new OpenCvSFaceRecognitionEngine(
    store,
    wrappedOptions,
    NullLogger<OpenCvSFaceRecognitionEngine>.Instance);

var exitCode = 0;
foreach (var command in parsed.Commands)
{
    try
    {
        switch (command.Kind)
        {
            case EvaluationKind.SamePerson:
                exitCode = Math.Max(exitCode, await RunPairAsync("same-person", command.Paths, engine, options.RealEngine));
                break;
            case EvaluationKind.DifferentPerson:
                exitCode = Math.Max(exitCode, await RunPairAsync("different-person", command.Paths, engine, options.RealEngine));
                break;
            case EvaluationKind.NoFace:
                exitCode = Math.Max(exitCode, await RunSingleAsync("no-face", command.Paths[0], engine));
                break;
            case EvaluationKind.MultipleFaces:
                exitCode = Math.Max(exitCode, await RunSingleAsync("multiple-faces", command.Paths[0], engine));
                break;
        }
    }
    catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
    {
        Console.WriteLine($"{command.Kind}: file error: {exception.Message}");
        exitCode = 1;
    }
}

Console.WriteLine("Sample-size limitation: this POC reports local outcomes only and does not claim statistical accuracy.");
return exitCode;

static async Task<int> RunPairAsync(
    string label,
    IReadOnlyList<string> paths,
    OpenCvSFaceRecognitionEngine engine,
    RealFaceRecognitionOptions options)
{
    var reference = await ExtractAsync(paths[0], engine);
    if (!reference.Succeeded || reference.Template is null)
    {
        PrintEncoding(label, paths[0], reference);
        return 1;
    }

    var probeBytes = await File.ReadAllBytesAsync(paths[1]);
    var verification = await engine.VerifyAsync(probeBytes, reference.Template);
    Console.WriteLine("");
    Console.WriteLine($"Evaluation: {label}");
    Console.WriteLine($"Reference: {SafePath(paths[0])}");
    Console.WriteLine($"Probe: {SafePath(paths[1])}");
    Console.WriteLine($"Succeeded: {verification.Succeeded}");
    Console.WriteLine($"Decision: {(verification.IsMatch ? "Match" : "No Match")}");
    Console.WriteLine($"Cosine score: {verification.SimilarityScore:0.000000}");
    Console.WriteLine($"Development threshold: {options.CosineSimilarityThreshold:0.000000}");
    Console.WriteLine($"Code: {verification.ErrorCode}");
    return verification.Succeeded ? 0 : 1;
}

static async Task<int> RunSingleAsync(
    string label,
    string path,
    OpenCvSFaceRecognitionEngine engine)
{
    var result = await ExtractAsync(path, engine);
    PrintEncoding(label, path, result);
    return result.Succeeded ? 0 : 1;
}

static async Task<FaceEncodingResult> ExtractAsync(
    string path,
    OpenCvSFaceRecognitionEngine engine)
{
    var bytes = await File.ReadAllBytesAsync(path);
    return await engine.ExtractEncodingAsync(bytes);
}

static void PrintEncoding(string label, string path, FaceEncodingResult result)
{
    Console.WriteLine("");
    Console.WriteLine($"Evaluation: {label}");
    Console.WriteLine($"Image: {SafePath(path)}");
    Console.WriteLine($"Succeeded: {result.Succeeded}");
    Console.WriteLine($"Code: {result.ErrorCode}");
    Console.WriteLine($"Detected faces: {result.FaceCount}");
    Console.WriteLine($"Quality: {result.QualityScore:0.000}");
    Console.WriteLine($"Brightness: {result.BrightnessScore:0.000}");
    Console.WriteLine($"Sharpness: {result.SharpnessScore:0.000}");
    Console.WriteLine($"Face area ratio: {result.FaceAreaRatio:0.000}");
}

static void PrintReadiness(RealFaceModelReadiness readiness, RealFaceRecognitionOptions options)
{
    Console.WriteLine("AttendAI Real Face Engine POC");
    Console.WriteLine($"Status: {(readiness.Succeeded ? "Ready" : "Not Ready")}");
    Console.WriteLine($"Gate: {readiness.GateResult}");
    Console.WriteLine($"Error code: {readiness.ErrorCode}");
    Console.WriteLine($"Engine: {options.EngineName} {options.EngineVersion}");
    Console.WriteLine($"Model: {options.ModelName} {options.ModelVersion}");
    Console.WriteLine($"Template format: {options.TemplateFormatVersion}");
    Console.WriteLine($"Expected embedding dimension: {options.EmbeddingDimension}");
    Console.WriteLine($"OpenCV version: {(string.IsNullOrWhiteSpace(readiness.OpenCvVersion) ? "Not loaded" : readiness.OpenCvVersion)}");
    if (readiness.RecognizerMetadata is not null)
    {
        Console.WriteLine($"SFace input: {readiness.RecognizerMetadata.InputElementType} {readiness.RecognizerMetadata.InputShape}");
        Console.WriteLine($"SFace output: {readiness.RecognizerMetadata.OutputElementType} {readiness.RecognizerMetadata.OutputShape}");
        Console.WriteLine($"Confirmed embedding dimension: {readiness.RecognizerMetadata.EmbeddingDimension}");
    }

    if (readiness.Paths is not null)
    {
        Console.WriteLine($"Detector model: {readiness.Paths.DetectorSafeIdentifier}");
        Console.WriteLine($"Recognizer model: {readiness.Paths.RecognizerSafeIdentifier}");
    }
}

static string SafePath(string path)
    => Path.GetFileName(path);

static void PrintUsage()
{
    Console.WriteLine("AttendAI Real Face Engine POC");
    Console.WriteLine("Usage:");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.RealFaceEngine.Poc -- --models-only");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.RealFaceEngine.Poc -- --same-person <image1> <image2>");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.RealFaceEngine.Poc -- --different-person <image1> <image2>");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.RealFaceEngine.Poc -- --no-face <image>");
    Console.WriteLine("  dotnet run --project experiments/AttendAI.RealFaceEngine.Poc -- --multiple-faces <image>");
    Console.WriteLine("Options:");
    Console.WriteLine("  --content-root <path>  ASP.NET Core content root. Defaults to src/AttendAI.Web.");
    Console.WriteLine("  --threshold <score>   Development cosine threshold. Default: 0.363.");
    Console.WriteLine("No images are copied, saved, or printed.");
}

internal enum EvaluationKind
{
    SamePerson,
    DifferentPerson,
    NoFace,
    MultipleFaces
}

internal sealed record EvaluationCommand(EvaluationKind Kind, IReadOnlyList<string> Paths);

internal sealed record Arguments(
    bool ShowHelp,
    bool ModelsOnly,
    string? ContentRoot,
    double? Threshold,
    IReadOnlyList<EvaluationCommand> Commands)
{
    public static Arguments Parse(string[] args)
    {
        var commands = new List<EvaluationCommand>();
        var modelsOnly = false;
        string? contentRoot = null;
        double? threshold = null;

        for (var index = 0; index < args.Length; index++)
        {
            switch (args[index])
            {
                case "-h":
                case "--help":
                    return new Arguments(true, false, null, null, []);
                case "--models-only":
                    modelsOnly = true;
                    break;
                case "--content-root":
                    contentRoot = ReadValue(args, ref index);
                    break;
                case "--threshold":
                    if (double.TryParse(ReadValue(args, ref index), out var parsedThreshold))
                    {
                        threshold = parsedThreshold;
                    }
                    break;
                case "--same-person":
                    commands.Add(new EvaluationCommand(EvaluationKind.SamePerson, ReadPaths(args, ref index, 2)));
                    break;
                case "--different-person":
                    commands.Add(new EvaluationCommand(EvaluationKind.DifferentPerson, ReadPaths(args, ref index, 2)));
                    break;
                case "--no-face":
                    commands.Add(new EvaluationCommand(EvaluationKind.NoFace, ReadPaths(args, ref index, 1)));
                    break;
                case "--multiple-faces":
                    commands.Add(new EvaluationCommand(EvaluationKind.MultipleFaces, ReadPaths(args, ref index, 1)));
                    break;
                default:
                    return new Arguments(true, false, null, null, []);
            }
        }

        return new Arguments(false, modelsOnly, contentRoot, threshold, commands);
    }

    private static string ReadValue(string[] args, ref int index)
    {
        if (index + 1 >= args.Length)
        {
            throw new ArgumentException($"Missing value for {args[index]}.");
        }

        index++;
        return args[index];
    }

    private static IReadOnlyList<string> ReadPaths(string[] args, ref int index, int count)
    {
        var paths = new List<string>(count);
        for (var offset = 0; offset < count; offset++)
        {
            paths.Add(ReadValue(args, ref index));
        }

        return paths;
    }
}

internal sealed class PocHostEnvironment : IHostEnvironment
{
    public PocHostEnvironment(string contentRootPath)
    {
        ContentRootPath = contentRootPath;
    }

    public string EnvironmentName { get; set; } = Environments.Development;

    public string ApplicationName { get; set; } = "AttendAI.RealFaceEngine.Poc";

    public string ContentRootPath { get; set; }

    public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
}
