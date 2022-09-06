namespace Engine6;


class Engine6 {
    static void Main () {
        using var f = new GdiWindow(); //new("data/teapot.obj", true)
        f.Run();
    }
}
