namespace Engine6;
using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;

public class EditorWindow:GdiWindow {

    public EditorWindow () : base() {
        using Ascii str = new("does not work yet");
        User32.SetWindowText(this, str);
    }

    protected override void OnIdle () {
        var now = DateTime.Now;
        if (CaretBlinkingPeriod < now - LastCaretBlink) {
            showCaret = !showCaret;
            LastCaretBlink = now;
            User32.InvalidateWindow(this);
        }
    }

    protected override void OnPaint (in PaintArgs _) {
        Resize();

        if (0 == ClientSize.X || 0 == ClientSize.Y)
            return;

        Dib.ClearU32(Color.Black);
        var (visibleTextRowCount, r) = Maths.IntDivRem(ClientSize.Y, PixelFont.Height);
        if (0 != r)
            ++visibleTextRowCount;
        //for (var y = windowOffset.Y; y < Editors.Count && y < visibleTextRowCount; ++y)
        //    if (0 < Editors[y].Length)
        //        Dib.DrawString(Editors[y].GetCopy(), PixelFont, 0, y * PixelFont.Height, Color.White);

        //if (showCaret)
        //    Dib.FillRectU32(new(new(Editors[caretPosition.Y].At * PixelFont.Width, 0), new(PixelFont.Width, PixelFont.Height)), Color.White);

        Blit(Dc, new(new(), ClientSize), Dib);
    }

    protected override void OnKeyDown (in KeyArgs args) {
        var key = args.Key;

        if (IsKeyDown(Key.ControlKey))
            DoControl(key);
        else if (IsPrintable(key))
            DoPrintable(key);
        else
            switch (key) {
                case Key.Escape:
                    User32.PostQuitMessage(0);
                    return;
                case Key.Left:
                    //if (0 < Editors[caretPosition.Y].At)
                    //    --Editors[caretPosition.Y].At;
                    break;
                case Key.Right:
                    //if (Editors[caretPosition.Y].At < Editors[caretPosition.Y].Length)
                    //    ++Editors[caretPosition.Y].At;
                    break;
                case Key.Home:
                    //Editors[caretPosition.Y].At = 0;
                    break;
                case Key.End:
                    //Editors[caretPosition.Y].At = Editors[caretPosition.Y].Length;
                    break;
                case Key.Delete:
                    //if (Editors[caretPosition.Y].At < Editors[caretPosition.Y].Length)
                    //    Editors[caretPosition.Y].Delete();
                    break;
                case Key.Back:
                    //if (0 < Editors[caretPosition.Y].At)
                    //    Editors[caretPosition.Y].Backspace();
                    break;
                default:
                    return;
            }
        LastCaretBlink = DateTime.Now;
        showCaret = true;
        User32.InvalidateWindow(this);
    }

    private void DoControl (Key k) {
        switch (k) {
            //case Key.Z:
            //    if (0 < ed.GetUndoCount())
            //        ed.Undo();
            //    break;
            default:
                return;
        }
        User32.InvalidateWindow(this);
    }


    private void DoPrintable (Key k) {
        Debug.Assert(IsPrintable(k));
        var i = (int)k;
        var bank = IsKeyDown(Key.ShiftKey) ? 1 : 0;
        if ('A' <= i && i <= 'Z' && User32.IsCapsLockOn())
            bank = 1 - bank;
        //Editors.Insert(Banks[bank][i]);
    }

    private Range selection;
    private bool showCaret;
    private DateTime LastCaretBlink;
    private Vector2i windowOffset;
    private Vector2i caretPosition;

    private static readonly char[] Unshifted = {'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  ' ','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '0', '1', '2', '3', '4', '5', '6', '7', '8', '9','\0', '\0', '\0', '\0', '\0', '\0', '\0',  'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  ';', '=', ',', '-', '.', '/', '`','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '[', '\\', ']', '\'','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  };
    private static readonly char[] Shifted = {'\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  ' ','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  ')', '!', '@', '#', '$', '%', '^', '&', '*', '(','\0', '\0', '\0', '\0', '\0', '\0', '\0',  'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  ':', '+', '<', '_', '>', '?', '~','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  '{', '|', '}', '"','\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0', '\0',  };
    private static readonly char[][] Banks = { Unshifted, Shifted };
    private static bool IsPrintable (Key k) =>
        Unshifted[(byte)k] != 0;

    private static readonly TimeSpan CaretBlinkingPeriod = new(500 * 10000);

}
