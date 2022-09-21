namespace Engine6;
using Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Win32;

public class EditorWindow:GdiWindow {

    public EditorWindow () : base() {
        Editors.Add(new());
    }

    private bool showCaret;
    //private LineEdit ed = new();
    private DateTime LastCaretBlink;
    private Vector2i windowOffset;
    private Vector2i caretPosition;

    protected override void OnIdle () {
        var now = DateTime.Now;
        if (CaretBlinkingPeriod < now - LastCaretBlink) {
            showCaret = !showCaret;
            LastCaretBlink = now;
            User32.InvalidateWindow(this);
        }
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

    private readonly List<LineEdit> Editors = new();

    private void DoPrintable (Key k) {
        Debug.Assert(IsPrintable(k));
        var i = (int)k;
        var bank = IsKeyDown(Key.ShiftKey) ? 1 : 0;
        if ('A' <= i && i <= 'Z' && User32.IsCapsLockOn())
            bank = 1 - bank;
        Editors[caretPosition.Y].Insert(Banks[bank][i]);
    }

    protected override void OnPaint (in PaintArgs _) {
        Resize();

        if (0 == ClientSize.X || 0 == ClientSize.Y)
            return;

        Dib.ClearU32(Color.Black);

        for (var y = windowOffset.Y; y < Editors.Count; ++y)
            if (0 < Editors[y].Length)
                Dib.DrawString(Editors[y].GetCopy(), PixelFont, 0, y * PixelFont.Height, Color.White);

        if (showCaret)
            Dib.FillRectU32(new(new(Editors[caretPosition.Y].At * PixelFont.Width, 0), new(PixelFont.Width, PixelFont.Height)), Color.White);

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
                    if (0 < Editors[caretPosition.Y].At)
                        --Editors[caretPosition.Y].At;
                    break;
                case Key.Right:
                    if (Editors[caretPosition.Y].At < Editors[caretPosition.Y].Length)
                        ++Editors[caretPosition.Y].At;
                    break;
                case Key.Home:
                    Editors[caretPosition.Y].At = 0;
                    break;
                case Key.End:
                    Editors[caretPosition.Y].At = Editors[caretPosition.Y].Length;
                    break;
                case Key.Delete:
                    if (Editors[caretPosition.Y].At < Editors[caretPosition.Y].Length)
                        Editors[caretPosition.Y].Delete();
                    break;
                case Key.Back:
                    if (0 < Editors[caretPosition.Y].At)
                        Editors[caretPosition.Y].Backspace();
                    break;
                default:
                    return;
            }
        LastCaretBlink = DateTime.Now;
        showCaret = true;
        User32.InvalidateWindow(this);
    }

    private static readonly byte[] Unshifted = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)' ', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)'0', (byte)'1', (byte)'2', (byte)'3', (byte)'4', (byte)'5', (byte)'6', (byte)'7', (byte)'8', (byte)'9', 0, 0, 0, 0, 0, 0, 0, (byte)'a', (byte)'b', (byte)'c', (byte)'d', (byte)'e', (byte)'f', (byte)'g', (byte)'h', (byte)'i', (byte)'j', (byte)'k', (byte)'l', (byte)'m', (byte)'n', (byte)'o', (byte)'p', (byte)'q', (byte)'r', (byte)'s', (byte)'t', (byte)'u', (byte)'v', (byte)'w', (byte)'x', (byte)'y', (byte)'z', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)';', (byte)'=', (byte)',', (byte)'-', (byte)'.', (byte)'/', (byte)'`', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)'[', (byte)'\\', (byte)']', (byte)'\'', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
    private static readonly byte[] Shifted = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)' ', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)')', (byte)'!', (byte)'@', (byte)'#', (byte)'$', (byte)'%', (byte)'^', (byte)'&', (byte)'*', (byte)'(', 0, 0, 0, 0, 0, 0, 0, (byte)'A', (byte)'B', (byte)'C', (byte)'D', (byte)'E', (byte)'F', (byte)'G', (byte)'H', (byte)'I', (byte)'J', (byte)'K', (byte)'L', (byte)'M', (byte)'N', (byte)'O', (byte)'P', (byte)'Q', (byte)'R', (byte)'S', (byte)'T', (byte)'U', (byte)'V', (byte)'W', (byte)'X', (byte)'Y', (byte)'Z', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)':', (byte)'+', (byte)'<', (byte)'_', (byte)'>', (byte)'?', (byte)'~', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, (byte)'{', (byte)'|', (byte)'}', (byte)'"', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, };
    private static readonly byte[][] Banks = { Unshifted, Shifted };
    private static bool IsPrintable (Key k) =>
        Unshifted[(byte)k] != 0;

    private static readonly TimeSpan CaretBlinkingPeriod = new(500 * 10000);

}
