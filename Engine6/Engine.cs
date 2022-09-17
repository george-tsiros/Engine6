using System.Windows.Forms;
using Engine6;
using Gl;

using Configuration c = new();
if (DialogResult.OK == c.ShowDialog() && c.Config is ContextConfiguration config)
    using (CubeTest window = new(config))
        window.Run();
