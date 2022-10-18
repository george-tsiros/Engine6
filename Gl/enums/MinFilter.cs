namespace Gl;

public enum MinFilter {
    Nearest = Const.GL_NEAREST,
    Linear = Const.GL_LINEAR,
    NearestMipMapNearest = Const.GL_NEAREST_MIPMAP_NEAREST,
    LinearMipMapNearest = Const.GL_LINEAR_MIPMAP_NEAREST,
    NearestMipMapLinear = Const.GL_NEAREST_MIPMAP_LINEAR,
    LinearMipMapLinear = Const.GL_LINEAR_MIPMAP_LINEAR,
}
