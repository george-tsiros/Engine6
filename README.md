**Engine6**: sixth iteration of the toy engine. `GlWindow` is the main class, so to speak. (Also, `FastNoiseLite` [FastNoiseLite](https://github.com/Auburn/FastNoiseLite) was used earlier.)

**FixEol**: this converts all CR-LF line endings to LF line endings and removes any kind of BOM, with extreme prejudice.

**Gl**: anything directly or indirectly related to opengl.

**Common**: some extra matrix and vector structs (integer, double, etc) along with functions, types and extensions common to all projects.

**ShaderGen**: creates classes in `Shaders` for every vertex/fragment shader pair in `Shaders/shadersources` by compiling and inspecting them. Allows for type-safe use of shaders in c#. Could be better.

**Shaders**: autogenerated classes exposing the shaders in shadersources/ as c#/.net classes

**Win32**: structures, enumerations and functions for the win32 api
