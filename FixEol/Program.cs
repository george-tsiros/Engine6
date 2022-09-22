namespace FixEol;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class Program {

    static readonly string[] extensions = ".sln,.cs,.cpp,.c,.h,.hpp,.md,.bat,.txt,.cap,.json,.vert,.frag,.csproj,.sql,.runsettings".Split(',');
    static readonly string[] skipDirectories = "obj,.git,.vs,properties,bin,TestResults".Split(',');

    static bool HasKnownExtension (string f) =>
        Array.Exists(extensions, e => string.Equals(Path.GetExtension(f), e, StringComparison.OrdinalIgnoreCase));


    static bool IsNotInOutputDir (string f) =>
        Array.TrueForAll(skipDirectories, x => f.IndexOf($"\\{x}\\", StringComparison.OrdinalIgnoreCase) < 0);

    static bool MayNeedFix (string f) =>
        HasKnownExtension(f) && IsNotInOutputDir(f);

    static void Main () =>
        Parallel.ForEach(Array.FindAll(Directory.GetFiles(".", "*.*", SearchOption.AllDirectories), MayNeedFix), Fix);

    static void Fix (string filepath) {
        if (0 == new FileInfo(filepath).Length)
            return;

        var asciiChars = NonAsciiBytesAtBeginning(filepath);
        if (0 != asciiChars)
            Console.Write($"{filepath} starts with {asciiChars} non-ascii bytes\n");
        var buffer = new byte[4096];
        var hasLineFeed = false;
        byte lastByte = 0;
        using (var fs = File.OpenRead(filepath)) {
            fs.Seek(asciiChars, SeekOrigin.Begin);
            var lineCount = 0;
            var lineIndex = 0;
            while (fs.Position < fs.Length) {
                var start = fs.Position;
                var read = fs.Read(buffer, 0, 4096);
                for (var i = 0; i < read; ++i) {
                    var b = buffer[i];
                    if ('\r' == b) {
                        hasLineFeed = true;
                        ++lineCount;
                        lineIndex = 0;
                        Console.Write($"{filepath} line #{lineCount} ends in \\r\n");
                    } else if ('\n' == b) {
                        if (lastByte != 13)
                            ++lineCount;
                        lineIndex = 0;
                    } else if (b < ' ' || '~' < b) {
                        Console.Write($"{filepath} line #{lineCount} index #{lineIndex} has byte 0x{b:x}\n");
                    }
                    ++lineIndex;
                    lastByte = b;
                }
            }
        }
        if (!hasLineFeed && 0 == asciiChars)
            return;
        Console.Write($"warning: rewriting {filepath}\n");
        var lines = File.ReadAllLines(filepath);
        using StreamWriter f = new(filepath, false, Encoding.ASCII) { NewLine = "\n" };
        foreach (var line in lines)
            f.WriteLine(line);
    }

    static int NonAsciiBytesAtBeginning (string filepath) {
        using var fs = File.OpenRead(filepath);
        for (int count = 0; ; ++count) {
            var b = fs.ReadByte();
            if (b < 0)
                throw new InvalidOperationException("read failed");
            if (b < 0x80)
                return count;
        }
    }
}
