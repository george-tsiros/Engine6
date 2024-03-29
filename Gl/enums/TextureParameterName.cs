namespace Gl;

public enum TextureParameterName {
    TEXTURE_WIDTH = Const.GL_TEXTURE_WIDTH,
    TEXTURE_HEIGHT = Const.GL_TEXTURE_HEIGHT,
    TEXTURE_INTERNAL_FORMAT = Const.GL_TEXTURE_INTERNAL_FORMAT,
    TEXTURE_COMPONENTS = Const.GL_TEXTURE_COMPONENTS,
    TEXTURE_BORDER_COLOR = Const.GL_TEXTURE_BORDER_COLOR,
    //TEXTURE_BORDER_COLOR_NV = Const.GL_TEXTURE_BORDER_COLOR_NV,
    TEXTURE_BORDER = Const.GL_TEXTURE_BORDER,
    TEXTURE_MAG_FILTER = Const.GL_TEXTURE_MAG_FILTER,
    TEXTURE_MIN_FILTER = Const.GL_TEXTURE_MIN_FILTER,
    TEXTURE_WRAP_S = Const.GL_TEXTURE_WRAP_S,
    TEXTURE_WRAP_T = Const.GL_TEXTURE_WRAP_T,
    TEXTURE_RED_SIZE = Const.GL_TEXTURE_RED_SIZE,
    TEXTURE_GREEN_SIZE = Const.GL_TEXTURE_GREEN_SIZE,
    TEXTURE_BLUE_SIZE = Const.GL_TEXTURE_BLUE_SIZE,
    TEXTURE_ALPHA_SIZE = Const.GL_TEXTURE_ALPHA_SIZE,
    TEXTURE_LUMINANCE_SIZE = Const.GL_TEXTURE_LUMINANCE_SIZE,
    TEXTURE_INTENSITY_SIZE = Const.GL_TEXTURE_INTENSITY_SIZE,
    TEXTURE_PRIORITY = Const.GL_TEXTURE_PRIORITY,
    //TEXTURE_PRIORITY_EXT = Const.GL_TEXTURE_PRIORITY_EXT,
    TEXTURE_RESIDENT = Const.GL_TEXTURE_RESIDENT,
    TEXTURE_DEPTH_EXT = Const.GL_TEXTURE_DEPTH_EXT,
    TEXTURE_WRAP_R = Const.GL_TEXTURE_WRAP_R,
    //TEXTURE_WRAP_R_EXT = Const.GL_TEXTURE_WRAP_R_EXT,
    //TEXTURE_WRAP_R_OES = Const.GL_TEXTURE_WRAP_R_OES,
    DETAIL_TEXTURE_LEVEL_SGIS = Const.GL_DETAIL_TEXTURE_LEVEL_SGIS,
    DETAIL_TEXTURE_MODE_SGIS = Const.GL_DETAIL_TEXTURE_MODE_SGIS,
    DETAIL_TEXTURE_FUNC_POINTS_SGIS = Const.GL_DETAIL_TEXTURE_FUNC_POINTS_SGIS,
    SHARPEN_TEXTURE_FUNC_POINTS_SGIS = Const.GL_SHARPEN_TEXTURE_FUNC_POINTS_SGIS,
    SHADOW_AMBIENT_SGIX = Const.GL_SHADOW_AMBIENT_SGIX,
    DUAL_TEXTURE_SELECT_SGIS = Const.GL_DUAL_TEXTURE_SELECT_SGIS,
    QUAD_TEXTURE_SELECT_SGIS = Const.GL_QUAD_TEXTURE_SELECT_SGIS,
    TEXTURE_4DSIZE_SGIS = Const.GL_TEXTURE_4DSIZE_SGIS,
    TEXTURE_WRAP_Q_SGIS = Const.GL_TEXTURE_WRAP_Q_SGIS,
    TEXTURE_MIN_LOD = Const.GL_TEXTURE_MIN_LOD,
    //TEXTURE_MIN_LOD_SGIS = Const.GL_TEXTURE_MIN_LOD_SGIS,
    TEXTURE_MAX_LOD = Const.GL_TEXTURE_MAX_LOD,
    //TEXTURE_MAX_LOD_SGIS = Const.GL_TEXTURE_MAX_LOD_SGIS,
    TEXTURE_BASE_LEVEL = Const.GL_TEXTURE_BASE_LEVEL,
    //TEXTURE_BASE_LEVEL_SGIS = Const.GL_TEXTURE_BASE_LEVEL_SGIS,
    TEXTURE_MAX_LEVEL = Const.GL_TEXTURE_MAX_LEVEL,
    //TEXTURE_MAX_LEVEL_SGIS = Const.GL_TEXTURE_MAX_LEVEL_SGIS,
    TEXTURE_FILTER4_SIZE_SGIS = Const.GL_TEXTURE_FILTER4_SIZE_SGIS,
    TEXTURE_CLIPMAP_CENTER_SGIX = Const.GL_TEXTURE_CLIPMAP_CENTER_SGIX,
    TEXTURE_CLIPMAP_FRAME_SGIX = Const.GL_TEXTURE_CLIPMAP_FRAME_SGIX,
    TEXTURE_CLIPMAP_OFFSET_SGIX = Const.GL_TEXTURE_CLIPMAP_OFFSET_SGIX,
    TEXTURE_CLIPMAP_VIRTUAL_DEPTH_SGIX = Const.GL_TEXTURE_CLIPMAP_VIRTUAL_DEPTH_SGIX,
    TEXTURE_CLIPMAP_LOD_OFFSET_SGIX = Const.GL_TEXTURE_CLIPMAP_LOD_OFFSET_SGIX,
    TEXTURE_CLIPMAP_DEPTH_SGIX = Const.GL_TEXTURE_CLIPMAP_DEPTH_SGIX,
    POST_TEXTURE_FILTER_BIAS_SGIX = Const.GL_POST_TEXTURE_FILTER_BIAS_SGIX,
    POST_TEXTURE_FILTER_SCALE_SGIX = Const.GL_POST_TEXTURE_FILTER_SCALE_SGIX,
    TEXTURE_LOD_BIAS_S_SGIX = Const.GL_TEXTURE_LOD_BIAS_S_SGIX,
    TEXTURE_LOD_BIAS_T_SGIX = Const.GL_TEXTURE_LOD_BIAS_T_SGIX,
    TEXTURE_LOD_BIAS_R_SGIX = Const.GL_TEXTURE_LOD_BIAS_R_SGIX,
    GENERATE_MIPMAP = Const.GL_GENERATE_MIPMAP,
    //GENERATE_MIPMAP_SGIS = Const.GL_GENERATE_MIPMAP_SGIS,
    TEXTURE_COMPARE_SGIX = Const.GL_TEXTURE_COMPARE_SGIX,
    TEXTURE_COMPARE_OPERATOR_SGIX = Const.GL_TEXTURE_COMPARE_OPERATOR_SGIX,
    TEXTURE_LEQUAL_R_SGIX = Const.GL_TEXTURE_LEQUAL_R_SGIX,
    TEXTURE_GEQUAL_R_SGIX = Const.GL_TEXTURE_GEQUAL_R_SGIX,
    TEXTURE_MAX_CLAMP_S_SGIX = Const.GL_TEXTURE_MAX_CLAMP_S_SGIX,
    TEXTURE_MAX_CLAMP_T_SGIX = Const.GL_TEXTURE_MAX_CLAMP_T_SGIX,
    TEXTURE_MAX_CLAMP_R_SGIX = Const.GL_TEXTURE_MAX_CLAMP_R_SGIX,
    TEXTURE_MAX_ANISOTROPY = Const.GL_TEXTURE_MAX_ANISOTROPY,
    TEXTURE_LOD_BIAS = Const.GL_TEXTURE_LOD_BIAS,
    TEXTURE_COMPARE_MODE = Const.GL_TEXTURE_COMPARE_MODE,
    TEXTURE_COMPARE_FUNC = Const.GL_TEXTURE_COMPARE_FUNC,
    TEXTURE_SWIZZLE_R = Const.GL_TEXTURE_SWIZZLE_R,
    TEXTURE_SWIZZLE_G = Const.GL_TEXTURE_SWIZZLE_G,
    TEXTURE_SWIZZLE_B = Const.GL_TEXTURE_SWIZZLE_B,
    TEXTURE_SWIZZLE_A = Const.GL_TEXTURE_SWIZZLE_A,
    TEXTURE_SWIZZLE_RGBA = Const.GL_TEXTURE_SWIZZLE_RGBA,
    TEXTURE_UNNORMALIZED_COORDINATES_ARM = Const.GL_TEXTURE_UNNORMALIZED_COORDINATES_ARM,
    DEPTH_STENCIL_TEXTURE_MODE = Const.GL_DEPTH_STENCIL_TEXTURE_MODE,
    TEXTURE_TILING_EXT = Const.GL_TEXTURE_TILING_EXT,
    TEXTURE_FOVEATED_CUTOFF_DENSITY_QCOM = Const.GL_TEXTURE_FOVEATED_CUTOFF_DENSITY_QCOM,
}
