namespace FixEol;
using System;
using System.IO;
class Program {
    static readonly string[] extensions = ".cs,.txt,.cap,.json,.vert,.frag".Split(',');
    static bool HasExtension (string f) => Array.Exists(extensions, e => string.Equals(Path.GetExtension(f), e, StringComparison.OrdinalIgnoreCase));
    static void Main () {
        var all = Directory.GetFiles(".", "*.*", SearchOption.AllDirectories);
        foreach (var f in Array.FindAll(all, HasExtension))
            Fix(f);
    }
    static void Fix (string filepath) {
        var lines = File.ReadAllLines(filepath);
        using (var f = new StreamWriter(filepath, false) { NewLine = "\n" })
            foreach (var line in lines)
                f.WriteLine(line);
    }
}