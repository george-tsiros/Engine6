namespace Engine6;

using System;
using System.Windows.Forms;

class Engine6 {
    [STAThread]
    static void Main () {
        Application.VisualStyleState = System.Windows.Forms.VisualStyles.VisualStyleState.NoneEnabled;
        //new BlitTest(new(1280,720)).Run();
        //new HighlightTriangle(new("data/teapot.obj", true), new(800,600)).Run();
    }
}

