**Bench**: standalone, only used for benchmarking any piece of code.

**BitmapToRaster**: a *.net fw* project that converts `.png` files to a trivial, deflate-compressed file because I refuse to add a dependency to `System.Drawing.Common` just for reading a png.

**CppExample** and **CsFwExample**: experimental/exploratory code. 

**Engine5**: fifth iteration of the toy engine. `GlWindowBase` is the main class, so to speak. `FastNoiseLite` is [FastNoiseLite](https://github.com/Auburn/FastNoiseLite). `glfw.dll` is 3.3.4, 64bit.

**Gl**: anything directly or indirectly related to opengl. 

**GLFW.NET**: [GLFW.NET](https://github.com/ForeverZer0/glfw-net/tree/master/GLFW.NET) slightly altered.

**GlfwExample**: trivial glfw example in c++.

**Perf**: simplistic viewer for performance logs created with Engine5/Perf.

**ShaderGen**: creates classes in `Shaders` for every vertex/fragment shader pair in `Shaders/shadersources` by compiling and inspecting them. Allows for type-safe use of shaders in c#.

**Shaders**: `ParsedShader.Prepare` prepares each class by compiling its respective shader and writing the appropriate values. 