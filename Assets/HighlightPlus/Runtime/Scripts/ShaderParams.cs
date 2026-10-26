using UnityEngine;

namespace HighlightPlus {

    public static class ShaderParams {

        // general uniforms
        public static readonly int Cull = Shader.PropertyToID("_Cull");
        public static readonly int BlurScale = Shader.PropertyToID("_BlurScale");
        public static readonly int BlurScaleFirstHoriz = Shader.PropertyToID("_BlurScaleFirstHoriz");
        public static readonly int Speed = Shader.PropertyToID("_Speed");
        public static readonly int ConstantWidth = Shader.PropertyToID("_ConstantWidth");
        public static readonly int MinimumWidth = Shader.PropertyToID("_MinimumWidth");
        public static readonly int CutOff = Shader.PropertyToID("_CutOff");
        public static readonly int ZTest = Shader.PropertyToID("_ZTest");
        public static readonly int Flip = Shader.PropertyToID("_Flip");
        public static readonly int Debug = Shader.PropertyToID("_Debug");
        public static readonly int Color = Shader.PropertyToID("_Color");
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
        public static readonly int BaseMapST = Shader.PropertyToID("_BaseMap_ST");
        public static readonly int BlendSrc = Shader.PropertyToID("_BlendSrc");
        public static readonly int BlendDst = Shader.PropertyToID("_BlendDst");
        public static readonly int FadeFactor = Shader.PropertyToID("_HP_Fade");
        public static readonly int Padding = Shader.PropertyToID("_Padding");
        public static readonly int ResampleScale = Shader.PropertyToID("_ResampleScale");
        public static readonly int NoiseTex = Shader.PropertyToID("_NoiseTex");
        public static readonly int Pixelation = Shader.PropertyToID("_Pixelation");
        
        // outline uniforms
        public static readonly int OutlineWidth = Shader.PropertyToID("_OutlineWidth");
        public static readonly int OutlineZTest = Shader.PropertyToID("_OutlineZTest");
        public static readonly int OutlineDirection = Shader.PropertyToID("_OutlineDirection");
        public static readonly int OutlineColor = Shader.PropertyToID("_OutlineColor");
        public static readonly int OutlineVertexWidth = Shader.PropertyToID("_OutlineVertexWidth");
        public static readonly int OutlineGradientTex = Shader.PropertyToID("_OutlineGradientTex");
        public static readonly int OutlineVertexData = Shader.PropertyToID("_OutlineVertexData");
        public static readonly int OutlineStencilComp = Shader.PropertyToID("_OutlineStencilComp");
        public static readonly int OutlineEdgeThreshold = Shader.PropertyToID("_EdgeThreshold");
        public static readonly int OutlineSharpness = Shader.PropertyToID("_OutlineSharpness");

        // glow uniforms
        public static readonly int GlowZTest = Shader.PropertyToID("_GlowZTest");
        public static readonly int GlowStencilComp = Shader.PropertyToID("_GlowStencilComp");
        public static readonly int GlowStencilOp = Shader.PropertyToID("_GlowStencilOp");
        public static readonly int GlowDirection = Shader.PropertyToID("_GlowDirection");
        public static readonly int Glow = Shader.PropertyToID("_Glow");
        public static readonly int GlowColor = Shader.PropertyToID("_GlowColor");
        public static readonly int Glow2 = Shader.PropertyToID("_Glow2");

        // see-through uniforms
        public static readonly int SeeThrough = Shader.PropertyToID("_SeeThrough");
        public static readonly int SeeThroughNoise = Shader.PropertyToID("_SeeThroughNoise");
        public static readonly int SeeThroughBorderWidth = Shader.PropertyToID("_SeeThroughBorderWidth");
        public static readonly int SeeThroughBorderConstantWidth = Shader.PropertyToID("_SeeThroughBorderConstantWidth");
        public static readonly int SeeThroughTintColor = Shader.PropertyToID("_SeeThroughTintColor");
        public static readonly int SeeThroughBorderColor = Shader.PropertyToID("_SeeThroughBorderColor");
        public static readonly int SeeThroughStencilRef = Shader.PropertyToID("_SeeThroughStencilRef");
        public static readonly int SeeThroughStencilComp = Shader.PropertyToID("_SeeThroughStencilComp");
        public static readonly int SeeThroughStencilPassOp = Shader.PropertyToID("_SeeThroughStencilPassOp");
        public static readonly int SeeThroughDepthOffset = Shader.PropertyToID("_SeeThroughDepthOffset");
        public static readonly int SeeThroughMaxDepth = Shader.PropertyToID("_SeeThroughMaxDepth");
        public static readonly int SeeThroughFadeRange = Shader.PropertyToID("_SeeThroughFadeRange");
        public static readonly int SeeThroughTexture = Shader.PropertyToID("_SeeThroughTexture");
        public static readonly int SeeThroughTextureScale = Shader.PropertyToID("_SeeThroughTextureScale");

        // inner glow uniforms
        public static readonly int InnerGlowData = Shader.PropertyToID("_InnerGlowData");
        public static readonly int InnerGlowZTest = Shader.PropertyToID("_InnerGlowZTest");
        public static readonly int InnerGlowColor = Shader.PropertyToID("_InnerGlowColor");
        public static readonly int InnerGlowBlendMode = Shader.PropertyToID("_InnerGlowBlendMode");

        // mask uniforms (not declared in Properties so globals take effect over material defaults)
        public static readonly int MaskStencilRef = Shader.PropertyToID("_MaskStencilRef");
        public static readonly int MaskWriteMask = Shader.PropertyToID("_MaskWriteMask");

        // focus uniforms
        public static readonly int FocusColor = Shader.PropertyToID("_FocusColor");
        public static readonly int FocusBlurTex = Shader.PropertyToID("_FocusBlurTex");
        public static readonly int FocusBlurRT = Shader.PropertyToID("_HPFocusBlurRT");
        public static readonly int FocusBlurTempRT = Shader.PropertyToID("_HPFocusBlurTempRT");
        public static readonly int FocusDesaturation = Shader.PropertyToID("_FocusDesaturation");

        // overlay uniforms
        public static readonly int OverlayData = Shader.PropertyToID("_OverlayData");
        public static readonly int OverlayBackColor = Shader.PropertyToID("_OverlayBackColor");
        public static readonly int OverlayColor = Shader.PropertyToID("_OverlayColor");
        public static readonly int OverlayHitPosData = Shader.PropertyToID("_OverlayHitPosData");
        public static readonly int OverlayHitStartTime = Shader.PropertyToID("_OverlayHitStartTime");
        public static readonly int OverlayTexture = Shader.PropertyToID("_OverlayTexture");
        public static readonly int OverlayTextureScrolling = Shader.PropertyToID("_OverlayTextureScrolling");
        public static readonly int OverlayZTest = Shader.PropertyToID("_OverlayZTest");
        public static readonly int OverlayPatternScrolling = Shader.PropertyToID("_OverlayPatternScrolling");
        public static readonly int OverlayPatternData = Shader.PropertyToID("_OverlayPatternData");
        public static readonly int OverlayBlendDst = Shader.PropertyToID("_OverlayBlendDst");

        // target uniforms
        public static readonly int TargetFXRenderData = Shader.PropertyToID("_TargetFXRenderData");
        public static readonly int TargetFXFrameData = Shader.PropertyToID("_TargetFXFrameData");
        public static readonly int GlowRT = Shader.PropertyToID("_HPComposeGlowFinal");
        public static readonly int OutlineRT = Shader.PropertyToID("_HPComposeOutlineFinal");

        // icon uniforms
        public static readonly int IconFXDarkColor = Shader.PropertyToID("_DarkColor");

        // custom vertex transform
        public static readonly int VertexTransformMode = Shader.PropertyToID("_HP_VertexTransformMode");

        // pattern uniforms
        public static readonly int PatternTex = Shader.PropertyToID("_PatternTex");
        public static readonly int DistortionTex = Shader.PropertyToID("_DistortionTex");
        public static readonly int PatternData = Shader.PropertyToID("_PatternData");
        
        // dashed outline
        public static readonly int DashData = Shader.PropertyToID("_DashData");
        public static readonly int DashCenter = Shader.PropertyToID("_HPDashCenter");

        // outline gradient
        public static readonly int OutlineGradientData = Shader.PropertyToID("_OutlineGradientData");
        
        // keywords
        public const string SKW_ALPHACLIP = "HP_ALPHACLIP";
        public const string SKW_OUTLINE_GRADIENT_WS = "HP_OUTLINE_GRADIENT_WS";
        public const string SKW_OUTLINE_GRADIENT_LS = "HP_OUTLINE_GRADIENT_LS";
        public const string SKW_ALL_EDGES = "HP_ALL_EDGES";
        public const string SKW_DEPTHCLIP = "HP_DEPTHCLIP";
        public const string SKW_DEPTHCLIP_INV = "HP_DEPTHCLIP_INV";
        public const string SKW_DEPTH_OFFSET = "HP_DEPTH_OFFSET";
        public const string SKW_TEXTURE_TRIPLANAR = "HP_TEXTURE_TRIPLANAR";
        public const string SKW_TEXTURE_TRIPLANAR_LOCAL = "HP_TEXTURE_TRIPLANAR_LOCAL";
        public const string SKW_TEXTURE_SCREENSPACE = "HP_TEXTURE_SCREENSPACE";
        public const string SKW_TEXTURE_OBJECTSPACE = "HP_TEXTURE_OBJECTSPACE";
        public const string SKW_SEETHROUGH_ONLY_BORDER = "HP_SEETHROUGH_ONLY_BORDER";
        public const string SKW_MASK_CUTOUT = "HP_MASK_CUTOUT";
        public const string SKW_DITHER_BLUENOISE = "HP_DITHER_BLUENOISE";
        public const string SKW_OUTLINE_STYLIZED = "HP_STYLIZED";
        public const string SKW_OUTLINE_DASHED = "HP_DASHED";
        public const string SKW_TARGET_FRAME = "HP_TARGET_FRAME";
        public const string SKW_TARGET_INWARD_CORNERS = "HP_TARGET_INWARD_CORNERS";
        public const string SKW_TARGET_CROSS = "HP_TARGET_CROSS";
        public const string SKW_PATTERN_POLKADOTS = "HP_PATTERN_POLKADOTS";
        public const string SKW_PATTERN_GRID = "HP_PATTERN_GRID";
        public const string SKW_PATTERN_STAGGERED_LINES = "HP_PATTERN_STAGGERED_LINES";
        public const string SKW_PATTERN_ZIGZAG = "HP_PATTERN_ZIGZAG";
        public const string SKW_SOURCE_SOLID_COLOR = "SOURCE_SOLID_COLOR";
    }
}

