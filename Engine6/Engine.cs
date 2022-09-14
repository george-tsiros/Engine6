namespace Engine6;

class Engine6 {

    static void Main () {
        using (var f = new CubeTest())  //new("data/teapot.obj", true)
            f.Run();
    }
}
