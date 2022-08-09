**Bench**: standalone, only used for benchmarking any piece of code.

**BitmapToRaster**: a *.net fw* project that converts `.png` files to a trivial, deflate-compressed file because I refuse to add a dependency to `System.Drawing.Common` just for reading a png. Recently added the ability to convert .ttf fonts (System.Drawing.Font) into a human-readable text format.

**Engine6**: sixth iteration of the toy engine. `GlWindow` is the main class, so to speak. `FastNoiseLite` is [FastNoiseLite](https://github.com/Auburn/FastNoiseLite). Recently added GlWindowArb which extends GlWindow to use the "newer"/"extended" opengl context creation method. 

**FixEol**: this converts all CR-LF line endings to LF line endings with extreme prejudice before every build.

**Gl**: anything directly or indirectly related to opengl. 

**Linear**: some extra matrix and vector structs (integer, double, etc)

**Perf**: simplistic viewer for performance logs created with Engine6/Perf.

**ShaderGen**: creates classes in `Shaders` for every vertex/fragment shader pair in `Shaders/shadersources` by compiling and inspecting them. Allows for type-safe use of shaders in c#. Could be better.

**Shaders**: `ParsedShader.Prepare` prepares each class by compiling its respective shader and writing the appropriate values. 

**Win32**: structures and enumerations for the win32 api
