namespace Gl;

using System;
internal enum ContextAttributes {
    MajorVersion = 0x2091,
    MinorVersion = 0x2092,
    LayerPlane = 0x2093,
    ContextFlags = 0x2094,
    ProfileMask = 0x9126,
}
[Flags]
internal enum ContextFlags {
    Debug = 1,
    ForwardCompatible = 2,
}
internal enum ProfileMask {
    Core = 1,
    Compatibility = 2,
}
public enum OpenglString {
    Vendor = 0x1F00,
    Renderer = 0x1F01,
    Version = 0x1F02,
    Extensions = 0x1F03,
}
[Flags]
internal enum ClassStyle:uint {
    VRedraw = 1 << 0,
    HRedraw = 1 << 1,
    DoubleClicks = 1 << 3,
    OwnDc = 1 << 5,
    ClassDc = 1 << 6,
    ParentDc = 1 << 7,
    NoClose = 1 << 9,
    SaveBits = 1 << 11,
    ByteAlignClient = 1 << 12,
    ByteAlignWindow = 1 << 13,
    GlobalClass = 1 << 14,
    DropShadow = 1 << 17,
}

[Flags]
public enum WindowPosFlags {
    NoSize = 0x1,
    NoMove = 0x2,
    NoZOrder = 0x4,
    NoRedraw = 0x8,
    NoActivate = 0x10,
    DrawFrame = 0x20,
    ShowWindow = 0x40,
    HideWindow = 0x80,
    NoCopyBits = 0x100,
    NoOwnerZOrder = 0x200,
    NoSendChanging = 0x400,
}

public enum SizeMessage {
    Restored = 0,
    Minimized,
    MaxShow,
    Maximized,
    MaxHide
}
[Flags]
public enum PixelFlags {
    DoubleBuffer = 0x00000001,
    Stereo = 0x00000002,
    DrawToWindow = 0x00000004,
    DrawToBitmap = 0x00000008,
    SupportGdi = 0x00000010,
    SupportOpengl = 0x00000020,
    GenericFormat = 0x00000040,
    NeedPalette = 0x00000080,
    NeedSystemPalette = 0x00000100,
    SwapExchange = 0x00000200,
    SwapCopy = 0x00000400,
    SwapLayerBuffers = 0x00000800,
    GenericAccelerated = 0x00001000,
    SupportComposition = 0x00008000,
    DepthDontCare = 0x20000000,
    DoubleBufferDontCare = 0x40000000,
    Stereodontcare = unchecked((int)0x80000000),
}
[Flags]
public enum WindowStyle {
    Overlapped = /*     */ 0x00000000,
    Tabstop = /*        */ 0x00010000,
    MaximizeBox = /*    */ 0x00010000,
    MinimizeBox = /*    */ 0x00020000,
    Group = /*          */ 0x00020000,
    Thickframe = /*     */ 0x00040000,
    Sysmenu = /*        */ 0x00080000,
    Hscroll = /*        */ 0x00100000,
    Vscroll = /*        */ 0x00200000,
    Dlgframe = /*       */ 0x00400000,
    Border = /*         */ 0x00800000,
    Maximize = /*       */ 0x01000000,
    ClipChildren = /*   */ 0x02000000,
    ClipSiblings = /*   */ 0x04000000,
    Disabled = /*       */ 0x08000000,
    Visible = /*        */ 0x10000000,
    Minimize = /*       */ 0x20000000,
    Child = /*          */ 0x40000000,
    Popup = unchecked((int)0x80000000),
    Tiled = Overlapped,
    ChildWindow = Child,
    Iconic = Minimize,
    Sizebox = Thickframe,
    Caption = Border | Dlgframe,
    OverlappedWindow = Caption | Sysmenu | Thickframe | MinimizeBox | MaximizeBox,
    TiledWindow = OverlappedWindow,
    PopupWindow = Popup | Border | Sysmenu,
}

public enum TextureParameter {
    DepthStencilTextureMode = Const.DEPTH_STENCIL_TEXTURE_MODE,
    BaseLevel = Const.TEXTURE_BASE_LEVEL,
    BorderColor = Const.TEXTURE_BORDER_COLOR,
    CompareMode = Const.TEXTURE_COMPARE_MODE,
    CompareFunc = Const.TEXTURE_COMPARE_FUNC,
    LodBias = Const.TEXTURE_LOD_BIAS,
    MagFilter = Const.TEXTURE_MAG_FILTER,
    MaxLevel = Const.TEXTURE_MAX_LEVEL,
    MaxLod = Const.TEXTURE_MAX_LOD,
    MinFilter = Const.TEXTURE_MIN_FILTER,
    MinLod = Const.TEXTURE_MIN_LOD,
    SwizzleR = Const.TEXTURE_SWIZZLE_R,
    SwizzleG = Const.TEXTURE_SWIZZLE_G,
    SwizzleB = Const.TEXTURE_SWIZZLE_B,
    SwizzleA = Const.TEXTURE_SWIZZLE_A,
    SwizzleRGBA = Const.TEXTURE_SWIZZLE_RGBA,
    WrapS = Const.TEXTURE_WRAP_S,
    WrapT = Const.TEXTURE_WRAP_T,
    WrapR = Const.TEXTURE_WRAP_R,
}

public enum MagFilter {
    Nearest = Const.NEAREST,
    Linear = Const.LINEAR,
}

public enum WrapCoordinate {
    WrapR = Const.TEXTURE_WRAP_R,
    WrapS = Const.TEXTURE_WRAP_S,
    WrapT = Const.TEXTURE_WRAP_T,
}

public enum Wrap {
    ClampToEdge = Const.CLAMP_TO_EDGE,
    ClampToBorder = Const.CLAMP_TO_BORDER,
}

public enum MinFilter {
    Nearest = Const.NEAREST,
    Linear = Const.LINEAR,
    NearestMipMapNearest = Const.NEAREST_MIPMAP_NEAREST,
    LinearMipMapNearest = Const.LINEAR_MIPMAP_NEAREST,
    NearestMipMapLinear = Const.NEAREST_MIPMAP_LINEAR,
    LinearMipMapLinear = Const.LINEAR_MIPMAP_LINEAR,
}
public enum PixelFormat {
    Red = Const.RED,
    Rg = Const.RG,
    Rgb = Const.RGB,
    Rgba = Const.RGBA,
    Bgr = Const.BGR,
    Bgra = Const.BGRA,
}
public enum TextureFormat {
    R8 = Const.R8,
    R16 = Const.R16,
    Rg8 = Const.RG8,
    Rg16 = Const.RG16,
    Rgb8 = Const.RGB8,
    Rgb16 = Const.RGB16,
    Rgba8 = Const.RGBA8,
    Rgba16 = Const.RGBA16,
}

public enum Primitive {
    Points = Const.POINTS,
    LineStrip = Const.LINE_STRIP,
    LineLoop = Const.LINE_LOOP,
    Lines = Const.LINES,
    LineStripAdjacency = Const.LINE_STRIP_ADJACENCY,
    LinesAdjacency = Const.LINES_ADJACENCY,
    TriangleStrip = Const.TRIANGLE_STRIP,
    TriangleFan = Const.TRIANGLE_FAN,
    Triangles = Const.TRIANGLES,
    TrianglesAdjacency = Const.TRIANGLES_ADJACENCY,
    TriangleStripAdjacency = Const.TRIANGLE_STRIP_ADJACENCY,
}

public enum AttribType {
    Byte = Const.BYTE,
    UByte = Const.UNSIGNED_BYTE,
    Short = Const.SHORT,
    UShort = Const.UNSIGNED_SHORT,
    Int = Const.INT,
    UInt = Const.UNSIGNED_INT,
    Float = Const.FLOAT,
    Double = Const.DOUBLE
}

public enum UniformType {
    Double = Const.DOUBLE,
    Float = Const.FLOAT,
    Int = Const.INT,
    Mat2d = Const.DOUBLE_MAT2,
    Mat2x3 = Const.FLOAT_MAT2x3,
    Mat2x3d = Const.DOUBLE_MAT2x3,
    Mat2x4 = Const.FLOAT_MAT2x4,
    Mat2x4d = Const.DOUBLE_MAT2x4,
    Mat3d = Const.DOUBLE_MAT3,
    Mat3x2 = Const.FLOAT_MAT3x2,
    Mat3x2d = Const.DOUBLE_MAT3x2,
    Mat3x4 = Const.FLOAT_MAT3x4,
    Mat3x4d = Const.DOUBLE_MAT3x4,
    Mat4d = Const.DOUBLE_MAT4,
    Mat4x2 = Const.FLOAT_MAT4x2,
    Mat4x2d = Const.DOUBLE_MAT4x2,
    Mat4x3 = Const.FLOAT_MAT4x3,
    Mat4x3d = Const.DOUBLE_MAT4x3,
    Matrix2x2 = Const.FLOAT_MAT2,
    Matrix3x3 = Const.FLOAT_MAT3,
    Matrix4x4 = Const.FLOAT_MAT4,
    Sampler2D = Const.SAMPLER_2D,
    UInt = Const.UNSIGNED_INT,
    Vec2d = Const.DOUBLE_VEC2,
    Vec2ui = Const.UNSIGNED_INT_VEC2,
    Vec3d = Const.DOUBLE_VEC3,
    Vec3ui = Const.UNSIGNED_INT_VEC3,
    Vec4d = Const.DOUBLE_VEC4,
    Vec4ui = Const.UNSIGNED_INT_VEC4,
    Vector2 = Const.FLOAT_VEC2,
    Vector2i = Const.INT_VEC2,
    Vector3 = Const.FLOAT_VEC3,
    Vector3i = Const.INT_VEC3,
    Vector4 = Const.FLOAT_VEC4,
    Vector4i = Const.INT_VEC4,
}

public enum AttributeType {
    Bool = Const.BOOL,
    Double = Const.DOUBLE,
    Float = Const.FLOAT,
    Int = Const.INT,
    Mat2d = Const.DOUBLE_MAT2,
    Mat2x3 = Const.FLOAT_MAT2x3,
    Mat2x3d = Const.DOUBLE_MAT2x3,
    Mat2x4 = Const.FLOAT_MAT2x4,
    Mat2x4d = Const.DOUBLE_MAT2x4,
    Mat3d = Const.DOUBLE_MAT3,
    Mat3x2 = Const.FLOAT_MAT3x2,
    Mat3x2d = Const.DOUBLE_MAT3x2,
    Mat3x4 = Const.FLOAT_MAT3x4,
    Mat3x4d = Const.DOUBLE_MAT3x4,
    Mat4d = Const.DOUBLE_MAT4,
    Mat4x2 = Const.FLOAT_MAT4x2,
    Mat4x2d = Const.DOUBLE_MAT4x2,
    Mat4x3 = Const.FLOAT_MAT4x3,
    Mat4x3d = Const.DOUBLE_MAT4x3,
    Matrix2x2 = Const.FLOAT_MAT2,
    Matrix3x3 = Const.FLOAT_MAT3,
    Matrix4x4 = Const.FLOAT_MAT4,
    Sampler1D = Const.SAMPLER_1D,
    Sampler2D = Const.SAMPLER_2D,
    Sampler3D = Const.SAMPLER_3D,
    UInt = Const.UNSIGNED_INT,
    Vec2b = Const.BOOL_VEC2,
    Vec2d = Const.DOUBLE_VEC2,
    Vec2ui = Const.UNSIGNED_INT_VEC2,
    Vec3b = Const.BOOL_VEC3,
    Vec3d = Const.DOUBLE_VEC3,
    Vec3ui = Const.UNSIGNED_INT_VEC3,
    Vec4b = Const.BOOL_VEC4,
    Vec4d = Const.DOUBLE_VEC4,
    Vec4ui = Const.UNSIGNED_INT_VEC4,
    Vector2 = Const.FLOAT_VEC2,
    Vector2i = Const.INT_VEC2,
    Vector3 = Const.FLOAT_VEC3,
    Vector3i = Const.INT_VEC3,
    Vector4 = Const.FLOAT_VEC4,
    Vector4i = Const.INT_VEC4,
}

public enum BufferTarget {
    Array = Const.ARRAY_BUFFER,
    AtomicCounter = Const.ATOMIC_COUNTER_BUFFER,
    CopyRead = Const.COPY_READ_BUFFER,
    CopyWrite = Const.COPY_WRITE_BUFFER,
    DispatchIndirect = Const.DISPATCH_INDIRECT_BUFFER,
    DrawIndirect = Const.DRAW_INDIRECT_BUFFER,
    ElementArray = Const.ELEMENT_ARRAY_BUFFER,
    PixelPack = Const.PIXEL_PACK_BUFFER,
    PixelUnpack = Const.PIXEL_UNPACK_BUFFER,
    Query = Const.QUERY_BUFFER,
    ShaderStorage = Const.SHADER_STORAGE_BUFFER,
    Texture = Const.TEXTURE_BUFFER,
    TransformFeedback_Buffer = Const.TRANSFORM_FEEDBACK_BUFFER,
    Uniform = Const.UNIFORM_BUFFER,
}

public enum BlendSourceFactor {
    Zero = 0,
    One = 1,
    SrcColor = 0x0300,
    OneMinusSrcColor = 0x0301,
    SrcAlpha = 0x0302,
    OneMinusSrcAlpha = 0x0303,
    DstAlpha = 0x0304,
    OneMinusDstAlpha = 0x0305,
}

public enum BlendDestinationFactor {
    Zero = 0,
    One = 1,
    SrcColor = 0x0300,
    OneMinusSrcColor = 0x0301,
    SrcAlpha = 0x0302,
    OneMinusSrcAlpha = 0x0303,
    DstAlpha = 0x0304,
    OneMinusDstAlpha = 0x0305,
    DstColor = 0x0306,
    OneMinusDstColor = 0x0307,
    SrcAlphaSaturate = 0x0308,
}

public enum Capability {
    Blend = Const.BLEND,
    CullFace = Const.CULL_FACE,
    DebugOutput = Const.DEBUG_OUTPUT,
    DebugOutputSynchronous = Const.DEBUG_OUTPUT_SYNCHRONOUS,
    DepthTest = Const.DEPTH_TEST,
    Dither = Const.DITHER,
    LineSmooth = Const.LINE_SMOOTH,
    StencilTest = Const.STENCIL_TEST,
}

public enum DepthFunction {
    Never = Const.NEVER,
    Less = Const.LESS,
    Equal = Const.EQUAL,
    LessEqual = Const.LEQUAL,
    Greater = Const.GREATER,
    NotEqual = Const.NOTEQUAL,
    GreaterEqual = Const.GEQUAL,
    Always = Const.ALWAYS,
}

[Flags]
public enum BufferBit {
    Color = Const.COLOR_BUFFER_BIT,
    Depth = Const.DEPTH_BUFFER_BIT,
    Stencil = Const.STENCIL_BUFFER_BIT,
}

public enum DebugSource {
    Api = Const.DEBUG_SOURCE_API,
    WindowSystem = Const.DEBUG_SOURCE_WINDOW_SYSTEM,
    ShaderCompiler = Const.DEBUG_SOURCE_SHADER_COMPILER,
    ThirdParty = Const.DEBUG_SOURCE_THIRD_PARTY,
    SourceApplication = Const.DEBUG_SOURCE_APPLICATION,
    Other = Const.DEBUG_SOURCE_OTHER
}

public enum DebugType {
    Error = Const.DEBUG_TYPE_ERROR,
    DeprecatedBehavior = Const.DEBUG_TYPE_DEPRECATED_BEHAVIOR,
    UndefinedBehavior = Const.DEBUG_TYPE_UNDEFINED_BEHAVIOR,
    Portability = Const.DEBUG_TYPE_PORTABILITY,
    Performance = Const.DEBUG_TYPE_PERFORMANCE,
    Marker = Const.DEBUG_TYPE_MARKER,
    PushGroup = Const.DEBUG_TYPE_PUSH_GROUP,
    PopGroup = Const.DEBUG_TYPE_POP_GROUP,
    Other = Const.DEBUG_TYPE_OTHER
}

public enum DebugSeverity {
    Low = Const.DEBUG_SEVERITY_LOW,
    Medium = Const.DEBUG_SEVERITY_MEDIUM,
    High = Const.DEBUG_SEVERITY_HIGH,
}

[Flags]
public enum Direction {
    None = 0,
    PosX = 1 << 0,
    NegX = 1 << 1,
    PosY = 1 << 2,
    NegY = 1 << 3,
    PosZ = 1 << 4,
    NegZ = 1 << 5,
}

public enum CheckFramebuffer {
    DrawFramebuffer = Const.DRAW_FRAMEBUFFER,
    Framebuffer = Const.FRAMEBUFFER,
    ReadFramebuffer = Const.READ_FRAMEBUFFER
}

public enum ShaderParameter {
    ShaderType = Const.SHADER_TYPE,
    DeleteStatus = Const.DELETE_STATUS,
    CompileStatus = Const.COMPILE_STATUS,
    InfoLogLength = Const.INFO_LOG_LENGTH,
    ShaderSourceLength = Const.SHADER_SOURCE_LENGTH,
}

public enum FramebufferTarget {
    Read = Const.READ_FRAMEBUFFER,
    Draw = Const.DRAW_FRAMEBUFFER,
    Framebuffer = Const.FRAMEBUFFER,
}

public enum FramebufferStatus:int {
    Undefined = Const.FRAMEBUFFER_UNDEFINED,
    Complete = Const.FRAMEBUFFER_COMPLETE,
    IncompleteAttachment = Const.FRAMEBUFFER_INCOMPLETE_ATTACHMENT,
    IncompleteMissingAttachment = Const.FRAMEBUFFER_INCOMPLETE_MISSING_ATTACHMENT,
    IncompleteDimensionsExt = Const.FRAMEBUFFER_INCOMPLETE_DIMENSIONS_EXT,
    IncompleteFormatsExt = Const.FRAMEBUFFER_INCOMPLETE_FORMATS_EXT,
    IncompleteDrawBuffer = Const.FRAMEBUFFER_INCOMPLETE_DRAW_BUFFER,
    IncompleteReadBuffer = Const.FRAMEBUFFER_INCOMPLETE_READ_BUFFER,
    Unsupported = Const.FRAMEBUFFER_UNSUPPORTED,
    IncompleteMultisample = Const.FRAMEBUFFER_INCOMPLETE_MULTISAMPLE,
    IncompleteLayerTargets = Const.FRAMEBUFFER_INCOMPLETE_LAYER_TARGETS,
    IncompleteLayerCount = Const.FRAMEBUFFER_INCOMPLETE_LAYER_COUNT,
}

public enum Attachment {
    Depth = Const.DEPTH_ATTACHMENT,
    Stencil = Const.STENCIL_ATTACHMENT,
    DepthStencil = Const.DEPTH_STENCIL_ATTACHMENT,
    Color0 = Const.COLOR_ATTACHMENT0,
    Color1 = Const.COLOR_ATTACHMENT1,
    Color2 = Const.COLOR_ATTACHMENT2,
    Color3 = Const.COLOR_ATTACHMENT3,
    Color4 = Const.COLOR_ATTACHMENT4,
    Color5 = Const.COLOR_ATTACHMENT5,
    Color6 = Const.COLOR_ATTACHMENT6,
    Color7 = Const.COLOR_ATTACHMENT7,
    Color8 = Const.COLOR_ATTACHMENT8,
    Color10 = Const.COLOR_ATTACHMENT10,
    Color11 = Const.COLOR_ATTACHMENT11,
    Color12 = Const.COLOR_ATTACHMENT12,
    Color13 = Const.COLOR_ATTACHMENT13,
}

public enum ProgramParameter {
    DeleteStatus = Const.DELETE_STATUS,
    LinkStatus = Const.LINK_STATUS,
    ValidateStatus = Const.VALIDATE_STATUS,
    InfoLogLength = Const.INFO_LOG_LENGTH,
    AttachedShaders = Const.ATTACHED_SHADERS,
    ActiveAtomicCounterBuffers = Const.ACTIVE_ATOMIC_COUNTER_BUFFERS,
    ActiveAttributes = Const.ACTIVE_ATTRIBUTES,
    ActiveAttributeMaxLength = Const.ACTIVE_ATTRIBUTE_MAX_LENGTH,
    ActiveUniforms = Const.ACTIVE_UNIFORMS,
    ActiveUniformMaxLength = Const.ACTIVE_UNIFORM_MAX_LENGTH,
    TransformFeedbackBufferMode = Const.TRANSFORM_FEEDBACK_BUFFER_MODE,
    TransformFeedbackVaryings = Const.TRANSFORM_FEEDBACK_VARYINGS,
    TransformFeedbackVaryingMaxLength = Const.TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH,
    GeometryVerticesOut = Const.GEOMETRY_VERTICES_OUT,
    GeometryInputType = Const.GEOMETRY_INPUT_TYPE,
    GeometryOutputType = Const.GEOMETRY_OUTPUT_TYPE,
}

public enum ProgramInterface {
    Uniform = Const.UNIFORM,
    ProgramInput = Const.PROGRAM_INPUT,
    ProgramOutput = Const.PROGRAM_OUTPUT,
    VertexSubroutine = Const.VERTEX_SUBROUTINE,
    FragmentSubroutine = Const.FRAGMENT_SUBROUTINE,
    VertexSubroutineUniform = Const.VERTEX_SUBROUTINE_UNIFORM,
    FragmentSubroutineUniform = Const.FRAGMENT_SUBROUTINE_UNIFORM,
}

public enum InterfaceParameter {
    ActiveResources = Const.ACTIVE_RESOURCES,
    MaxNameLength = Const.MAX_NAME_LENGTH,
}

public enum RenderbufferFormat {
    DepthComponent = Const.DEPTH_COMPONENT,
    R8 = Const.R8,
    Rg8 = Const.RG8,
    Rgb8 = Const.RGB8,
    Rgba8 = Const.RGBA8,
    R16 = Const.R16,
    Rg16 = Const.RG16,
    Rgb16 = Const.RGB16,
    Rgba16 = Const.RGBA16,
}

public enum ShaderType {
    Fragment = Const.FRAGMENT_SHADER,
    Vertex = Const.VERTEX_SHADER,

}

public enum IntParameter {
    FramebufferBinding = Const.FRAMEBUFFER_BINDING,
    DepthFunc = Const.DEPTH_FUNC,
    CurrentProgram = Const.CURRENT_PROGRAM,
    ArrayBufferBinding = Const.ARRAY_BUFFER_BINDING,
    VertexArrayBinding = Const.VERTEX_ARRAY_BINDING,
    ActiveTexture = Const.ACTIVE_TEXTURE,
}
