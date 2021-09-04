using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

Options opts = null;
var parser = new Parser(setting =>
{
    setting.AutoHelp = true;
    setting.AutoVersion = true;
    setting.CaseInsensitiveEnumValues = true;
    setting.CaseSensitive = false;
    setting.HelpWriter = Console.Out;
});
var p = parser.ParseArguments<Options>(args).WithParsed(o=> opts = o).WithNotParsed(_=>opts = new Options());
opts.Init();
if (p is NotParsed<Options>)
{
    return;
}
//workflowTest 1

if (!File.Exists(opts.YODKFile))
{
    Console.WriteLine($"Can't find yodk.exe in {opts.YODKFile}");
    return;
}

if (!Directory.Exists(opts.WorkingDirectory))
{
    Console.WriteLine($"Can't find directory {opts.WorkingDirectory}");
    return;
}

var targetFiles = Directory
    .EnumerateFiles(opts.WorkingDirectory, $"*.{opts.Extension}", SearchOption.AllDirectories);

if (!targetFiles.Any())
{
    Console.WriteLine($"No files found in this directory with pattern *.{opts.Extension}");
    return;
}


var start = new ProcessStartInfo()
{
    Arguments = $"/C {opts.YODKFile} {opts.Verb.ToString().ToLower()} {string.Join(" ", targetFiles)}",
    FileName = "cmd.exe",
    WorkingDirectory = opts.WorkingDirectory,
    WindowStyle = ProcessWindowStyle.Hidden,
};
using var proc = new Process();
proc.StartInfo = start;
proc.OutputDataReceived += (s, e) => Console.WriteLine(e.Data); 
proc.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
proc.Start();
proc.WaitForExit();



class Options
{
    [Option('f', "File", Required = false, HelpText = "(Default: ./yodk.exe) The full path to yodk.exe")]
    public string YODKFile { get; set; } =Path.Combine(Directory.GetCurrentDirectory(), "yodk.exe");

    [Option('d', "Directory", Required = false, HelpText = "(Default: .) The directory where you want to compile your files from")]
    public string WorkingDirectory { get; set; } = Directory.GetCurrentDirectory();

    [Option('v', "Verb", Required = false, HelpText = "(Default: compile) Possible verbs:(.nolol)compile, (.yolol)format, (.yolol)verify, (.yolol)optimize")]
    public NololVerb Verb { get; set; } = NololVerb.Compile;

    public string Extension { get; set; }

    public void  Init()
    {
        YODKFile = Path.GetFullPath(YODKFile);
        WorkingDirectory = Path.GetFullPath(WorkingDirectory);
        switch (Verb)
        {
            case NololVerb.Compile:
                Extension = "nolol";
                break;
            case NololVerb.Format:
            case NololVerb.Verify:
            case NololVerb.Optimize:
                Extension = "yolol";
                break;
            default:
                break;
        }
    }

    public enum NololVerb
    {
        Compile,
        Format,
        Verify,
        Optimize
    }

    
}