namespace Gl;

using System.Diagnostics;
using System;
using static GlContext;

public class FragOut {
    internal int location;
    internal FragOut (int i) => location = i;
    public static implicit operator int (FragOut self) => self.location;
    public static implicit operator FragOut (int i) => new(i);
}

public class Attrib<T> where T : unmanaged {
    internal int location;
    internal Attrib (int i) => location = i;
    public static implicit operator int (Attrib<T> self) => self.location;
    public static implicit operator Attrib<T> (int i) => new(i);
}

public class Thingie<T>:IDisposable where T : Program, new() {
    public T Program { get; }
    public VertexArray VertexArray { get; }

    public Thingie () : this(new()) { }

    public Thingie (T program) {
        Program = program;
        VertexArray = new();
    }

    public void Assign<U> (System.Linq.Expressions.Expression<Func<T, Attrib<U>>> f, BufferObject<U> buffer, int divisor = 0) where U : unmanaged {
        Debug.Assert(f is not null);
        var F = f.Compile();
        VertexArray.Assign(buffer, F(Program), divisor);
    }

    public void Enable (Capability cap) => Active[Array.IndexOf(Capabilities, cap)] = 1;
    public void Disable (Capability cap) => Active[Array.IndexOf(Capabilities, cap)] = -1;
    public void Ignore (Capability cap) => Active[Array.IndexOf(Capabilities, cap)] = 0;

    public void Use () {
        UseProgram(Program);
        BindVertexArray(VertexArray);
        for (var i = 0; i < Capabilities.Length; ++i) {
            var active = Active[i];
            if (0 == active)
                continue;
            var c = Capabilities[i];
            var isEnabled = IsEnabled(c);
            var mustBe = 0 < active;
            if (isEnabled == mustBe)
                continue;
            if (mustBe)
                Enable(c);
            else
                Disable(c);
        }
    }

    private readonly int[] Active = new int[Capabilities.Length];

    private static readonly Capability[] Capabilities = { };

    bool disposed = false;
    public void Dispose () {
        if (!disposed) {
            disposed = true;
            Program.Dispose();
            VertexArray.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
