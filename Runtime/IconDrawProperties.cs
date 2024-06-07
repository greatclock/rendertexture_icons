using UnityEngine;

namespace GreatClock.Common.IconAtlas {

	/// <summary>
	/// The data struct defining the region that mask texture is applied.
	/// </summary>
	public enum eMaskRegionType {
		/// <summary>
		/// The mask region will be the icon rect.
		/// </summary>
		IconRegion,
		/// <summary>
		/// The mask region will be the current drawing rect.
		/// </summary>
		DrawRegion
	}

	/// <summary>
	/// All additional drawing properties.
	/// </summary>
	public struct IconDrawProperties {
		public bool drawRegionDefault;
		public Rect drawRegion;
		public ColorMatrix colorMatrix;
		public int slicedRight;
		public int slicedTop;
		public int slicedLeft;
		public int slicedBottom;
		public float spriteBorderRight;
		public float spriteBorderTop;
		public float spriteBorderLeft;
		public float spriteBorderBottom;
		public Texture maskTexture;
		public Rect maskRectUV;
		public eMaskRegionType maskRegionType;

		/// <summary>
		/// Get a default IconDrawProperties object.
		/// </summary>
		public static IconDrawProperties Default {
			get {
				IconDrawProperties ret = new IconDrawProperties();
				ret.drawRegionDefault = true;
				ret.drawRegion = new Rect();
				ret.colorMatrix = ColorMatrix.Default;
				ret.slicedRight = 0;
				ret.slicedTop = 0;
				ret.slicedLeft = 0;
				ret.slicedBottom = 0;
				ret.spriteBorderRight = 0f;
				ret.spriteBorderTop = 0f;
				ret.spriteBorderLeft = 0f;
				ret.spriteBorderBottom = 0f;
				return ret;
			}
		}

		/// <summary>
		/// Set the region for draw command to specify the rect area to fill within icon region.
		/// </summary>
		/// <param name="drawRegion">The region in normalized rect of icon region to fill.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetDrawRegion(Rect drawRegion) {
			drawRegionDefault = false;
			this.drawRegion = drawRegion;
			return this;
		}

		/// <summary>
		/// Set the color to tint the texture to draw.
		/// </summary>
		/// <param name="color">The tint color value.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetColor(Color color) {
			colorMatrix = ColorMatrix.Tint(color);
			return this;
		}

		/// <summary>
		/// Set the saturation value.
		/// </summary>
		/// <param name="saturation">0 : full grayscale, 1 : origin color.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetSaturation(float saturation) {
			colorMatrix = ColorMatrix.Saturation(saturation);
			return this;
		}

		/// <summary>
		/// Set custom color transform matrix.
		/// </summary>
		/// <param name="colorMatrix">Custom color transform matrix.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetColorMatrix(ColorMatrix colorMatrix) {
			this.colorMatrix = colorMatrix;
			return this;
		}

		/// <summary>
		/// Set the 4 lines for 9-sliced to each side of the drawing area.
		/// </summary>
		/// <param name="slicedRight">The right line inwards from the right side in pixel.</param>
		/// <param name="slicedTop">The top line inwards from the top side in pixel.</param>
		/// <param name="slicedLeft">The left line inwards from the left side in pixel.</param>
		/// <param name="slicedBottom">The bottom line inwards from the bottom side in pixel.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetSliced(int slicedRight, int slicedTop, int slicedLeft, int slicedBottom) {
			this.slicedRight = slicedRight;
			this.slicedTop = slicedTop;
			this.slicedLeft = slicedLeft;
			this.slicedBottom = slicedBottom;
			return this;
		}

		/// <summary>
		/// Set the 9-sliced uv borders of the texture.
		/// </summary>
		/// <param name="borderRight">The uv width of right border.</param>
		/// <param name="borderTop">The uv height of top border.</param>
		/// <param name="borderLeft">The uv width of left border.</param>
		/// <param name="borderBottom">The uv height of bottom border.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetSpriteBorders(float borderRight, float borderTop, float borderLeft, float borderBottom) {
			spriteBorderRight = borderRight;
			spriteBorderTop = borderTop;
			spriteBorderLeft = borderLeft;
			spriteBorderBottom = borderBottom;
			return this;
		}

		/// <summary>
		/// Set the drawing texture mask for the draw command.
		/// </summary>
		/// <param name="maskTex">Texture to per pixel multiply by the texture to draw.</param>
		/// <param name="regionType">The area of the icon that the mask applies.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetMask(Texture maskTex, eMaskRegionType regionType) {
			maskTexture = maskTex;
			maskRectUV = new Rect(0f, 0f, 1f, 1f);
			maskRegionType = regionType;
			return this;
		}

		/// <summary>
		/// Set the drawing texture mask for the draw command.
		/// </summary>
		/// <param name="maskTex">Texture to per pixel multiply by the texture to draw.</param>
		/// <param name="uv">The rect area to apply for mask.</param>
		/// <param name="regionType">The area of the icon that the mask applies.</param>
		/// <returns>The IconDrawProperties object itself.</returns>
		public IconDrawProperties SetMask(Texture maskTex, Rect uv, eMaskRegionType regionType) {
			maskTexture = maskTex;
			maskRectUV = uv;
			maskRegionType = regionType;
			return this;
		}

	}

}