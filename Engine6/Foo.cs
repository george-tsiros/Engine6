namespace Engine6;

using Common;
using Win32;
using Gl;
using static Gl.GlContext;
using System.Numerics;
using System;
using Shaders;

public class Foo:GlWindow {
    private BufferObject<Vector2> uiBuffer;
    private UiTexture uiProgram;
    private Raster uiRaster;
    private Sampler2D uiSampler;
    private VertexArray uiVA;
    private Vector2i cursor = new();

    public Foo () : this(new(1280, 720)) { }
    public Foo (Vector2i size) {
        Reusables.Add(uiVA = new());
        Reusables.Add(uiProgram = new());
        Reusables.Add(uiBuffer = new(PresentationQuad));
        uiVA.Assign(uiBuffer, uiProgram.VertexPosition);
        Enable(Capability.CULL_FACE);
    }

    protected override void OnInput (int dx, int dy) {
        cursor = new(int.Clamp(cursor.X + dx, 0, ClientSize.X), int.Clamp(cursor.Y + dy, 0, ClientSize.Y));
    }

    protected override void OnLoad () {
        base.OnLoad();
        Disposables.Add(uiSampler = new(ClientSize, SizedInternalFormat.RGBA8) { Mag = MagFilter.Nearest, Min = MinFilter.Nearest });
        Disposables.Add(uiRaster = new(ClientSize));
        uiRaster.Clear(Color.Black);
    }
    /*
    move(
        Pixels + RenderWidth * spleen_8x16.height, 
        Pixels, 
        sizeof(u32) * RenderWidth * (RenderHeight - spleen_8x16.height));
    QueryPerformanceCounter((LARGE_INTEGER*)(Ticks + 1));
    for (i32 i = 0; Buffer[i]; ++i) {
        u8 const* glyph = rfGetGlyph(&spleen_8x16, Buffer[i]);
        if (!glyph)
            continue;
        for (i32 y = 0; y < spleen_8x16.height; ++y) {
            vi32 const row = vi32_set1(glyph[y]);
            vi32 const pixels = vi32_or(vi32_and(vi32_eq(vi32_and(row, Mask), Mask), On), vi32_and(vi32_eq(vi32_andnot(row, Mask), Mask), Off));
            avx_store((vi32*)(Pixels + RenderWidth * (spleen_8x16.height - y) + i * spleen_8x16.width), pixels);
        }
    }
    */

    protected override void Render () {
        BindVertexArray(uiVA);
        UseProgram(uiProgram);
        Array.Copy(uiRaster.Pixels, 0, uiRaster.Pixels, ClientSize.X * PixelFont.Height, ClientSize.X * (ClientSize.Y - PixelFont.Height));
        uiRaster.Pixels[(ClientSize.Y - cursor.Y - 1) * ClientSize.X + cursor.X] = ~0u;
        uiSampler.Upload(uiRaster);
        uiProgram.Tex0(0);
        uiSampler.BindTo(0);
        Viewport(Vector2i.Zero, ClientSize);
        ClearColor(0, 0, 0, 1);
        Clear(BufferBit.ColorDepth);
        DrawArrays(PrimitiveType.TRIANGLES, 0, 6);
    }
}