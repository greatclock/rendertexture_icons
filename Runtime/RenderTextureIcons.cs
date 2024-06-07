using UnityEngine;
using System.Collections.Generic;

namespace GreatClock.Common.IconAtlas {

	public class RenderTextureIcons : IOnUpdate {

		private RenderTexture mRenderTexture;
		private int mWidth;
		private int mHeight;
		private int mIconWidth;
		private int mIconHeight;
		private int mPadding;

		private Material mBlitMaterial;
		private Material mDrawSlicedMaterial;
		private int mBlitParamTargetRect;
		private int mBlitParamBorderRect;
		private int mBlitParamUVx;
		private int mBlitParamUVy;
		private int mBlitParamColorMatrix;
		private int mBlitParamMaskTex;
		private int mBlitParamMaskUV;

		private object mUsingsPrevKey;
		private IconUsageData mUsingsPrevData;
		private Dictionary<object, IconUsageData> mUsingDatas = new Dictionary<object, IconUsageData>();
		private Stack<IconUsageData> mClearedDatas = new Stack<IconUsageData>();

		private int mX;
		private int mY;

		private float mUVXDiv;
		private float mUVYDiv;
		private float mUVWidth;
		private float mUVHeight;
		private float mIconWidthDiv;
		private float mIconHeightDiv;

		private bool mAtlasFull = false;

		/// <summary>
		/// RenderTextureAtlas contructor.
		/// </summary>
		/// <param name="atlasWidth">The width of render texture atlas in pixels.</param>
		/// <param name="atlasHeight">The height of render texture atlas in pixels.</param>
		/// <param name="format">The format of render texture atlas.</param>
		/// <param name="iconWidth">Icon width in atlas in pixels.</param>
		/// <param name="iconHeight">Icon height in atlas in pixels.</param>
		/// <param name="padding">Space between two icons in pixels.</param>
		public RenderTextureIcons(int atlasWidth, int atlasHeight, RenderTextureFormat format, int iconWidth, int iconHeight, int padding) {
			mWidth = atlasWidth;
			mHeight = atlasHeight;
			mIconWidth = iconWidth;
			mIconHeight = iconHeight;
			mPadding = padding;

			mUVXDiv = 1f / mWidth;
			mUVYDiv = 1f / mHeight;
			mUVWidth = mIconWidth * mUVXDiv;
			mUVHeight = mIconHeight * mUVYDiv;
			mIconWidthDiv = 1f / iconWidth;
			mIconHeightDiv = 1f / iconHeight;

			mRenderTexture = new RenderTexture(mWidth, mHeight, 0, format, RenderTextureReadWrite.sRGB);
			mRenderTexture.name = "RenderTextureAtlas";
			mRenderTexture.autoGenerateMips = false;
			mRenderTexture.useMipMap = false;
			mRenderTexture.Create();
			mRenderTexture.MarkRestoreExpected();
			mRenderTexture.filterMode = FilterMode.Bilinear;
			mRenderTexture.wrapMode = TextureWrapMode.Clamp;

			mBlitMaterial = new Material(Shader.Find("Hidden/GreatClock/RenderTextureAtlas/IconRender"));
			mDrawSlicedMaterial = new Material(Shader.Find("Hidden/GreatClock/RenderTextureAtlas/DrawSlicedRect"));
			mBlitParamTargetRect = Shader.PropertyToID("_TargetRect");
			mBlitParamBorderRect = Shader.PropertyToID("_BorderRect");
			mBlitParamUVx = Shader.PropertyToID("_UV_x");
			mBlitParamUVy = Shader.PropertyToID("_UV_y");
			mBlitParamColorMatrix = Shader.PropertyToID("_ColorMatrix");
			mBlitParamMaskTex = Shader.PropertyToID("_MaskTex");
			mBlitParamMaskUV = Shader.PropertyToID("_MaskRect");

			Graphics.Blit(clearedTexture, mRenderTexture);
			mX = mPadding;
			mY = mPadding;

			AtlasChecker.Register(this);
		}

		/// <summary>
		/// The atlas texture that contains all drawn icons.
		/// </summary>
		public Texture texture { get { return mRenderTexture; } }

		/// <summary>
		/// Allocate a rectangle area in atlas.
		/// </summary>
		/// <param name="onRequestReset">RenderTexture may be released in some cases. Rebuild icon is required via this callback.</param>
		/// <param name="uv">The UV rect where this icon is allocated.</param>
		/// <returns>The key for this icon. It'll be used by further icon operations.</returns>
		public object AllocIcon(System.Action onRequestReset, out Rect uv) {
			IconUsageData data;
			object key;
			AllocIconInternal(onRequestReset, out data, out key);
			if (data == null) {
				uv = new Rect();
				return key;
			}
			uv = data.uv;
			return key;
		}

		/// <summary>
		/// Clear the icon. Replace the icon with cleared pixels.
		/// </summary>
		/// <param name="key">Key of the operated icon.</param>
		/// <returns>True if the operation is succeed.</returns>
		public bool ClearIcon(object key) {
			IconUsageData data;
			if (!TryGetUsingData(key, out data)) {
				return false;
			}
			mBlitMaterial.SetVector(mBlitParamTargetRect, new Vector4(data.uv.x, data.uv.y, data.uv.width, data.uv.height));
			Graphics.Blit(clearedTexture, mRenderTexture, mBlitMaterial);
			return true;
		}

		/// <summary>
		/// Draw a texture into the icon area.
		/// </summary>
		/// <param name="key">Key of the operated icon.</param>
		/// <param name="tex">The texture to be drawn into the area.</param>
		/// <returns>True if the operation is succeed.</returns>
		public bool Draw(object key, Texture tex) {
			return DrawInternal(key, tex, new Rect(0f, 0f, 1f, 1f), IconDrawProperties.Default);
		}

		/// <summary>
		/// Draw a rectangle part of texture into the icon area.
		/// </summary>
		/// <param name="key">Key of the operated icon.</param>
		/// <param name="tex">The texture to be drawn into the area.</param>
		/// <param name="uv">The UV rect range to be used in source texture.</param>
		/// <returns>True if the operation is succeed.</returns>
		public bool Draw(object key, Texture tex, Rect uv) {
			return DrawInternal(key, tex, uv, IconDrawProperties.Default);
		}

		/// <summary>
		/// Draw a rectangle part of texture into the icon area.
		/// </summary>
		/// <param name="key">Key of the operated icon.</param>
		/// <param name="tex">The texture to be drawn into the area.</param>
		/// <param name="properties">The additional properties for drawing texture.</param>
		/// <returns>True if the operation is succeed.</returns>
		public bool Draw(object key, Texture tex, IconDrawProperties properties) {
			return DrawInternal(key, tex, new Rect(0f, 0f, 1f, 1f), properties);
		}

		/// <summary>
		/// Draw a rectangle part of texture into the icon area.
		/// </summary>
		/// <param name="key">Key of the operated icon.</param>
		/// <param name="tex">The texture to be drawn into the area.</param>
		/// <param name="uv">The UV rect range to be used in source texture.</param>
		/// <param name="properties">The additional properties for drawing texture.</param>
		/// <returns>True if the operation is succeed.</returns>
		public bool Draw(object key, Texture tex, Rect uv, IconDrawProperties properties) {
			return DrawInternal(key, tex, uv, properties);
		}

		/// <summary>
		/// Release the icon that <see cref="AllocIcon(System.Action, out Rect)">AllocIcon</see> allocated.
		/// </summary>
		/// <param name="key">Key of the operated icon.</param>
		/// <returns>True if the operation is succeed.</returns>
		public bool ReleaseIcon(object key) {
			if (key == null) { return false; }
			IconUsageData data;
			if (!TryGetUsingData(key, out data)) {
				return false;
			}
			mUsingDatas.Remove(key);
			if (key == mUsingsPrevKey) {
				mUsingsPrevKey = null;
				mUsingsPrevData = null;
			}
			mClearedDatas.Push(data);
			return true;
		}

		/// <summary>
		/// Release all unmanaged resources. This method should be called when the atlas will not be used any more.
		/// </summary>
		public void Dispose() {
			if (mRenderTexture != null) {
				Object.Destroy(mRenderTexture);
				mRenderTexture = null;
			}
			if (mBlitMaterial != null) {
				Object.Destroy(mBlitMaterial);
				mBlitMaterial = null;
			}
			if (mDrawSlicedMaterial != null) {
				Object.Destroy(mDrawSlicedMaterial);
				mDrawSlicedMaterial = null;
			}
			mUsingDatas = null;
			mClearedDatas = null;
			AtlasChecker.Unregister(this);
		}

		void IOnUpdate.OnUpdate() {
			CheckRenderTexture();
		}

		private void AllocIconInternal(System.Action onRequestReset, out IconUsageData data, out object key) {
			data = null;
			key = null;
			if (mClearedDatas.Count > 0) {
				data = mClearedDatas.Pop();
				data.Reset();
			} else if (!mAtlasFull) {
				data = new IconUsageData(new Rect(mX * mUVXDiv, mY * mUVYDiv, mUVWidth, mUVHeight));
				mX += mPadding + mIconWidth;
				if (mX + mPadding + mIconWidth > mWidth) {
					mX = mPadding;
					mY += mPadding + mIconHeight;
					if (mY + mPadding + mIconHeight > mHeight) {
						mAtlasFull = true;
					}
				}
			}
			if (data == null) { return; }
			key = new object();
			data.onRequestReset = onRequestReset;
			mUsingDatas.Add(key, data);
			mUsingsPrevKey = key;
			mUsingsPrevData = data;
			mBlitMaterial.SetVector(mBlitParamTargetRect, new Vector4(data.uv.x, data.uv.y, data.uv.width, data.uv.height));
			Graphics.Blit(clearedTexture, mRenderTexture, mBlitMaterial);
		}

		private bool TryGetUsingData(object key, out IconUsageData data) {
			if (key == null) { data = null; return false; }
			if (key == mUsingsPrevKey) {
				data = mUsingsPrevData;
				return true;
			}
			if (!mUsingDatas.TryGetValue(key, out data)) {
				return false;
			}
			mUsingsPrevKey = key;
			mUsingsPrevData = data;
			return true;
		}

		private bool DrawInternal(object key, Texture tex, Rect uv, IconDrawProperties properties) {
			if (!mRenderTexture.IsCreated()) { return false; }
			IconUsageData data;
			if (!TryGetUsingData(key, out data)) {
				return false;
			}
			Rect drawRegion = new Rect(0f, 0f, 1f, 1f);
			if (!properties.drawRegionDefault) {
				drawRegion = properties.drawRegion;
				drawRegion = new Rect(drawRegion.x * mIconWidthDiv, drawRegion.y * mIconHeightDiv,
					drawRegion.width * mIconWidthDiv, drawRegion.height * mIconHeightDiv);
			}
			Vector2 regionMin = new Vector2(Mathf.Lerp(data.uv.xMin, data.uv.xMax, drawRegion.xMin), Mathf.Lerp(data.uv.yMin, data.uv.yMax, drawRegion.yMin));
			Vector2 regionMax = new Vector2(Mathf.Lerp(data.uv.xMin, data.uv.xMax, drawRegion.xMax), Mathf.Lerp(data.uv.yMin, data.uv.yMax, drawRegion.yMax));
			Rect region = new Rect(regionMin, regionMax - regionMin);
			mDrawSlicedMaterial.SetVector(mBlitParamTargetRect, new Vector4(region.x, region.y, region.width, region.height));
			mDrawSlicedMaterial.SetVector(mBlitParamBorderRect, new Vector4(
				properties.slicedLeft * mIconWidthDiv, properties.slicedBottom * mIconHeightDiv,
				1f - (properties.slicedLeft + properties.slicedRight) * mIconWidthDiv,
				1f - (properties.slicedBottom + properties.slicedTop) * mIconHeightDiv));
			mDrawSlicedMaterial.SetVector(mBlitParamUVx, new Vector4(uv.xMin, Mathf.Lerp(uv.xMin, uv.xMax, properties.spriteBorderLeft), Mathf.Lerp(uv.xMax, uv.xMin, properties.spriteBorderRight), uv.xMax));
			mDrawSlicedMaterial.SetVector(mBlitParamUVy, new Vector4(uv.yMin, Mathf.Lerp(uv.yMin, uv.yMax, properties.spriteBorderBottom), Mathf.Lerp(uv.yMax, uv.yMin, properties.spriteBorderTop), uv.yMax));
			mDrawSlicedMaterial.SetMatrix(mBlitParamColorMatrix, properties.colorMatrix.ToMatrix4x4());
			mDrawSlicedMaterial.SetTexture(mBlitParamMaskTex, properties.maskTexture);
			Rect maskRect = properties.maskRectUV;
			if (properties.maskRegionType == eMaskRegionType.IconRegion) {
				maskRect = new Rect(maskRect.min + maskRect.size * drawRegion.min, maskRect.size * drawRegion.size);
			}
			mDrawSlicedMaterial.SetVector(mBlitParamMaskUV, new Vector4(maskRect.x, maskRect.y, maskRect.width, maskRect.height));
			Graphics.Blit(tex, mRenderTexture, mDrawSlicedMaterial);
			return true;
		}

		private void CheckRenderTexture() {
			if (mRenderTexture.IsCreated()) { return; }
			mRenderTexture.Create();
			mRenderTexture.DiscardContents(true, true);
			Graphics.Blit(clearedTexture, mRenderTexture);
			foreach (KeyValuePair<object, IconUsageData> kv in mUsingDatas) {
				IconUsageData data = kv.Value;
				try { data.onRequestReset(); } catch (System.Exception e) { Debug.LogException(e); }
			}
		}

		static Texture2D cleared_texture;
		private static Texture2D clearedTexture {
			get {
				if (cleared_texture == null) {
					cleared_texture = new Texture2D(2, 2, TextureFormat.ARGB32, false);
					clearedTexture.SetPixel(0, 0, Color.clear);
					clearedTexture.SetPixel(0, 1, Color.clear);
					clearedTexture.SetPixel(1, 0, Color.clear);
					clearedTexture.SetPixel(1, 1, Color.clear);
					clearedTexture.Apply(false, true);
				}
				return cleared_texture;
			}
		}

		private class IconUsageData {
			public System.Action onRequestReset;
			private Rect mUV;
			public IconUsageData(Rect uv) {
				mUV = uv;
			}
			public Rect uv { get { return mUV; } }
			public void Reset() {
				onRequestReset = null;
			}
		}

	}

}