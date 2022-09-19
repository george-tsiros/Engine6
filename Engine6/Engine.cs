namespace Engine6;
using Common;
using System;
using System.Linq;
using System.Diagnostics;
using System.Text;
class Engine6 {

    static readonly byte[] Unshifted = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 49, 50, 51, 52, 53, 54, 55, 56, 57, 0, 0, 0, 0, 0, 0, 0, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111, 112, 113, 114, 115, 116, 117, 118, 119, 120, 121, 122, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 59, 61, 44, 45, 46, 47, 96, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 91, 92, 93, 39, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    static readonly byte[] Shifted = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 32, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 41, 33, 64, 35, 36, 37, 94, 38, 42, 40, 0, 0, 0, 0, 0, 0, 0, 65, 66, 67, 68, 69, 70, 71, 72, 73, 74, 75, 76, 77, 78, 79, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 58, 43, 60, 95, 62, 63, 126, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 123, 124, 125, 34, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    static void DoControl (LineEdit ed, ConsoleKey key) {
        switch (key) {
            case ConsoleKey.Z:
                ed.Undo();
                break;
        }
    }

    static void DoShifted (LineEdit ed, ConsoleKey key) {
        var b = Shifted[(int)key];
        if (0 != b)
            ed.Insert(b);
    }

    static void DoNormal (LineEdit ed, ConsoleKey key) {

        var b = Unshifted[(int)key];
        if (0 != b)
            ed.Insert(b);
        else
            switch (key) {
                case ConsoleKey.LeftArrow:
                    if (0 < ed.At)
                        --ed.At;
                    return;
                case ConsoleKey.RightArrow:
                    if (ed.At < ed.Length)
                        ++ed.At;
                    return;
                case ConsoleKey.Home:
                    if (0 < ed.At)
                        ed.At = 0;
                    break;
                case ConsoleKey.End:
                    if (ed.At < ed.Length)
                        ed.At = ed.Length;
                    break;
                case ConsoleKey.Delete:
                    if (ed.At < ed.Length)
                        ed.Delete();
                    break;
                case ConsoleKey.Backspace:
                    if (0 < ed.At)
                        ed.Backspace();
                    break;
            }
    }

    static int Main () {

        Console.OutputEncoding = Encoding.ASCII;
        Console.TreatControlCAsInput = true;
        LineEdit ed = new("");
        Console.Clear();
        Console.SetCursorPosition(0, 0);
        string blank = new(' ', Console.WindowWidth - 1);
        for (; ; ) {
            Console.CursorLeft = 0;
            Console.Write(blank);
            Console.CursorLeft = 0;
            Console.Write(Encoding.ASCII.GetString(ed.GetCopy()));
            Console.CursorLeft = ed.At;

            var k = Console.ReadKey(true);
            if (ConsoleModifiers.Control == k.Modifiers) {
                DoControl(ed, k.Key);
            } else if (ConsoleModifiers.Shift == k.Modifiers) {
                DoShifted(ed, k.Key);
            } else if (0 == k.Modifiers) {
                if (ConsoleKey.Escape == k.Key)
                    break;
                DoNormal(ed, k.Key);
            }
        }
        return 0;
    }
}
//using CubeTest window = new ();
//window.Run(Win32.CmdShow.ShowMaximized);
