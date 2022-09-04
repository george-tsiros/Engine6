namespace FixEol;

using System;
using System.IO;
using System.Text;

class Program {
    static readonly string[] extensions = ".cs,.txt,.cap,.json,.vert,.frag,.csproj".Split(',');

    static bool HasExtension (string f) => Array.Exists(extensions, e => string.Equals(Path.GetExtension(f), e, StringComparison.OrdinalIgnoreCase));

    static void Main () {
        Array.ForEach(Array.FindAll(Directory.GetFiles(".", "*.*", SearchOption.AllDirectories), HasExtension), Fix);
    }

    static bool IsAscii (string line) {
        foreach (var c in line)
            if ('~' < c)
                return false;
        return true;
    }

    static void Fix (string filepath) {
        var lines = File.ReadAllLines(filepath);
        if (Array.TrueForAll(lines, IsAscii))
            return;
        using var f = new StreamWriter(filepath, false, Encoding.ASCII) { NewLine = "\n" };
        foreach (var line in lines)
            f.WriteLine(line);
    }
}