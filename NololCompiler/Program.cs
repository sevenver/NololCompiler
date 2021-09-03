using CommandLine;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

Options opts = null;
Parser.Default.ParseArguments<Options>(args).WithParsed(o=> opts = o).WithNotParsed(_=>opts = new Options());
if (!File.Exists(opts.YODKFile))
{
    Console.WriteLine($"Can't find yodk.exe in {opts.YODKFile}");
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
    Arguments = $"/C {opts.YODKFile} {opts.Verb} {string.Join(" ", targetFiles)}",
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

    [Option('v', "Verb", Required = false, HelpText = "(Default: compile) Possible verbs:(.nolol)compile, (.yolol)format, (.yolol)verify, (.optimize)verify")]
    public string Verb { get; set; } = "compile";

    [Option('e', "Extension", Required = false, HelpText = "(Default: nolol) The extension to run the verb for")]
    public string Extension { get; set; } = "nolol";

}