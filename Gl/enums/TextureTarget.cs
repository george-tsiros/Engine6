﻿namespace Gl;

public enum TextureTarget {
    TEXTURE_1D = Const.GL_TEXTURE_1D,
    TEXTURE_2D = Const.GL_TEXTURE_2D,
    PROXY_TEXTURE_1D = Const.GL_PROXY_TEXTURE_1D,
    //PROXY_TEXTURE_1D_EXT = Const.GL_PROXY_TEXTURE_1D_EXT,
    PROXY_TEXTURE_2D = Const.GL_PROXY_TEXTURE_2D,
    //PROXY_TEXTURE_2D_EXT = Const.GL_PROXY_TEXTURE_2D_EXT,
    TEXTURE_3D = Const.GL_TEXTURE_3D,
    //TEXTURE_3D_EXT = Const.GL_TEXTURE_3D_EXT,
    //TEXTURE_3D_OES = Const.GL_TEXTURE_3D_OES,
    PROXY_TEXTURE_3D = Const.GL_PROXY_TEXTURE_3D,
    //PROXY_TEXTURE_3D_EXT = Const.GL_PROXY_TEXTURE_3D_EXT,
    DETAIL_TEXTURE_2D_SGIS = Const.GL_DETAIL_TEXTURE_2D_SGIS,
    TEXTURE_4D_SGIS = Const.GL_TEXTURE_4D_SGIS,
    PROXY_TEXTURE_4D_SGIS = Const.GL_PROXY_TEXTURE_4D_SGIS,
    TEXTURE_RECTANGLE = Const.GL_TEXTURE_RECTANGLE,
    //TEXTURE_RECTANGLE_ARB = Const.GL_TEXTURE_RECTANGLE_ARB,
    //TEXTURE_RECTANGLE_NV = Const.GL_TEXTURE_RECTANGLE_NV,
    PROXY_TEXTURE_RECTANGLE = Const.GL_PROXY_TEXTURE_RECTANGLE,
    //PROXY_TEXTURE_RECTANGLE_ARB = Const.GL_PROXY_TEXTURE_RECTANGLE_ARB,
    //PROXY_TEXTURE_RECTANGLE_NV = Const.GL_PROXY_TEXTURE_RECTANGLE_NV,
    TEXTURE_CUBE_MAP = Const.GL_TEXTURE_CUBE_MAP,
    //TEXTURE_CUBE_MAP_ARB = Const.GL_TEXTURE_CUBE_MAP_ARB,
    //TEXTURE_CUBE_MAP_EXT = Const.GL_TEXTURE_CUBE_MAP_EXT,
    //TEXTURE_CUBE_MAP_OES = Const.GL_TEXTURE_CUBE_MAP_OES,
    TEXTURE_CUBE_MAP_POSITIVE_X = Const.GL_TEXTURE_CUBE_MAP_POSITIVE_X,
    TEXTURE_CUBE_MAP_NEGATIVE_X = Const.GL_TEXTURE_CUBE_MAP_NEGATIVE_X,
    TEXTURE_CUBE_MAP_POSITIVE_Y = Const.GL_TEXTURE_CUBE_MAP_POSITIVE_Y,
    TEXTURE_CUBE_MAP_NEGATIVE_Y = Const.GL_TEXTURE_CUBE_MAP_NEGATIVE_Y,
    TEXTURE_CUBE_MAP_POSITIVE_Z = Const.GL_TEXTURE_CUBE_MAP_POSITIVE_Z,
    TEXTURE_CUBE_MAP_NEGATIVE_Z = Const.GL_TEXTURE_CUBE_MAP_NEGATIVE_Z,
    PROXY_TEXTURE_CUBE_MAP = Const.GL_PROXY_TEXTURE_CUBE_MAP,
    //PROXY_TEXTURE_CUBE_MAP_ARB = Const.GL_PROXY_TEXTURE_CUBE_MAP_ARB,
    //PROXY_TEXTURE_CUBE_MAP_EXT = Const.GL_PROXY_TEXTURE_CUBE_MAP_EXT,
    TEXTURE_1D_ARRAY = Const.GL_TEXTURE_1D_ARRAY,
    PROXY_TEXTURE_1D_ARRAY = Const.GL_PROXY_TEXTURE_1D_ARRAY,
    //PROXY_TEXTURE_1D_ARRAY_EXT = Const.GL_PROXY_TEXTURE_1D_ARRAY_EXT,
    TEXTURE_2D_ARRAY = Const.GL_TEXTURE_2D_ARRAY,
    PROXY_TEXTURE_2D_ARRAY = Const.GL_PROXY_TEXTURE_2D_ARRAY,
    //PROXY_TEXTURE_2D_ARRAY_EXT = Const.GL_PROXY_TEXTURE_2D_ARRAY_EXT,
    TEXTURE_BUFFER = Const.GL_TEXTURE_BUFFER,
    RENDERBUFFER = Const.GL_RENDERBUFFER,
    TEXTURE_CUBE_MAP_ARRAY = Const.GL_TEXTURE_CUBE_MAP_ARRAY,
    //TEXTURE_CUBE_MAP_ARRAY_ARB = Const.GL_TEXTURE_CUBE_MAP_ARRAY_ARB,
    //TEXTURE_CUBE_MAP_ARRAY_EXT = Const.GL_TEXTURE_CUBE_MAP_ARRAY_EXT,
    //TEXTURE_CUBE_MAP_ARRAY_OES = Const.GL_TEXTURE_CUBE_MAP_ARRAY_OES,
    PROXY_TEXTURE_CUBE_MAP_ARRAY = Const.GL_PROXY_TEXTURE_CUBE_MAP_ARRAY,
    //PROXY_TEXTURE_CUBE_MAP_ARRAY_ARB = Const.GL_PROXY_TEXTURE_CUBE_MAP_ARRAY_ARB,
    TEXTURE_2D_MULTISAMPLE = Const.GL_TEXTURE_2D_MULTISAMPLE,
    PROXY_TEXTURE_2D_MULTISAMPLE = Const.GL_PROXY_TEXTURE_2D_MULTISAMPLE,
    TEXTURE_2D_MULTISAMPLE_ARRAY = Const.GL_TEXTURE_2D_MULTISAMPLE_ARRAY,
    PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY = Const.GL_PROXY_TEXTURE_2D_MULTISAMPLE_ARRAY,
}
