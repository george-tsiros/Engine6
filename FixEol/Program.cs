namespace FixEol;

using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class Program {

    static readonly string[] extensions = ".cs,.cpp,.c,.h,.hpp,.md,.bat,.txt,.cap,.json,.vert,.frag,.csproj,.sql,.runsettings".Split(',');

    static bool HasKnownExtension (string f) =>
        Array.Exists(extensions, e => string.Equals(Path.GetExtension(f), e, StringComparison.OrdinalIgnoreCase));

    static bool HasKnownEncoding (string f) {
        var buffer = new byte[4];
        using (var fr = File.OpenRead(f))
            if (4 != fr.Read(buffer, 0, 4)) {
                Console.Write("{0} is too short\n", f);
                return false;
            }
        var encoding = Bom.FindBom(buffer).Encoding;
        if (encoding is null)
            Console.Write("{0} has no known encoding\n", f);
        else if (encoding != Encoding.ASCII)
            Console.Write("{0} has {1}\n", f, encoding.EncodingName);

        return encoding is not null;
    }

    static bool IsNotInOutputDir (string f) =>
        f.IndexOf(@"\obj\", StringComparison.OrdinalIgnoreCase) < 0 &&
        f.IndexOf(@"\properties\", StringComparison.OrdinalIgnoreCase) < 0 &&
        f.IndexOf(@"\bin\", StringComparison.OrdinalIgnoreCase) < 0;

    static bool HasBadEol (string f) {
        const int BufferLength = 4096;
        var buffer = new byte[BufferLength];
        using var r = File.OpenRead(f);
        while (r.Position < r.Length) {
            var read = r.Read(buffer, 0, BufferLength);
            if (0 == read)
                return false;
            if (0 <= Array.IndexOf(buffer, (byte)'\r', 0, read))
                return true;
        }
        return false;
    }

    static bool NeedsFix (string f) => HasKnownExtension(f) && IsNotInOutputDir(f) && HasKnownEncoding(f) && HasBadEol(f);

    static void Main () =>
        Parallel.ForEach(Array.FindAll(Directory.GetFiles(".", "*.*", SearchOption.AllDirectories), NeedsFix), Fix);

    static void Fix (string filepath) {
        Console.WriteLine($"WARNING: {filepath}");
        var buffer = new byte[4];
        using (var fs = File.OpenRead(filepath))
            if (4 != fs.Read(buffer, 0, 4))
                throw new InvalidOperationException("expected at least 4 bytes, this shouldn't happen");
        var bom = Bom.FindBom(buffer);
        var encoding = bom.Encoding ?? throw new InvalidOperationException("no encoding for this Bom, this shouldn't happen");
        var lines = File.ReadAllLines(filepath);
        using var f = new StreamWriter(filepath, false, encoding) { NewLine = "\n" };
        foreach (var line in lines)
            f.WriteLine(line);
    }
}
