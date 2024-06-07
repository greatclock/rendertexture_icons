using UnityEngine;

namespace GreatClock.Common.IconAtlas {

	/// <summary>
	/// A matrix that change a color into another with specific rules.<br />
	/// Final Color <i>out</i> calculated from <i>in</i> :<br />
	/// <i>out.r = in.r * rr + in.g * gr + in.b * br + rc</i> <br />
	/// <i>out.g = in.r * rg + in.g * gg + in.b * bg + gc</i> <br />
	/// <i>out.b = in.r * rb + in.g * gb + in.b * bb + bc</i> <br />
	/// <i>out.a = in.a * alpha</i>
	/// </summary>
	public struct ColorMatrix {

		/// <summary>
		/// The factors to multiply color.rgb when calculating grayscale.
		/// </summary>
		public static Vector3 luminance_factors = Vector3.zero;

		/// <summary>
		/// Value to multiply by the R-channel from target color.
		/// </summary>
		public float rr, rg, rb;

		/// <summary>
		/// Value to multiply by the G-channel from target color.
		/// </summary>
		public float gr, gg, gb;

		/// <summary>
		/// Value to multiply by the B-channel from target color.
		/// </summary>
		public float br, bg, bb;

		/// <summary>
		/// Compensation value added to the specific channel to final color.
		/// </summary>
		public float rc, gc, bc;

		/// <summary>
		/// Aphla value to multiply by alpha-channel from target color.
		/// </summary>
		public float alpha;

		public Matrix4x4 ToMatrix4x4() {
			return new Matrix4x4(
				new Vector4(rr, rg, rb, 0f),
				new Vector4(gr, gg, gb, 0f),
				new Vector4(br, bg, bb, 0f),
				new Vector4(rc, gc, bc, alpha)
			);
		}

		/// <summary>
		/// Default ColorMatrix without any color change.
		/// </summary>
		public static ColorMatrix Default {
			get {
				ColorMatrix matrix = new ColorMatrix();
				matrix.rr = matrix.gg = matrix.bb = 1f;
				matrix.rg = matrix.rb = matrix.gr = matrix.gb = matrix.br = matrix.bg = 0f;
				matrix.rc = matrix.gc = matrix.bc = 0f;
				matrix.alpha = 1f;
				return matrix;
			}
		}

		/// <summary>
		/// Create a ColorMatrix data object with color tint.
		/// </summary>
		/// <param name="tint">The color to multiply by target color.</param>
		/// <returns>The ColorMatrix data object with color tint.</returns>
		public static ColorMatrix Tint(Color tint) {
			ColorMatrix matrix = new ColorMatrix();
			matrix.rr = tint.r;
			matrix.gg = tint.g;
			matrix.bb = tint.b;
			matrix.alpha = tint.a;
			matrix.rg = matrix.rb = matrix.gr = matrix.gb = matrix.br = matrix.bg = 0f;
			matrix.rc = matrix.gc = matrix.bc = 0f;
			return matrix;
		}

		/// <summary>
		/// Create a ColorMatrix data object with saturation specified.
		/// </summary>
		/// <param name="saturation">Grayscale value. 0 : grayscale, 1 : do not change.</param>
		/// <returns>The ColorMatrix data object with saturation specified.</returns>
		public static ColorMatrix Saturation(float saturation) {
			return SaturationTint(saturation, Color.white);
		}

		/// <summary>
		/// Create a ColorMatrix data object with saturation and color tint.
		/// </summary>
		/// <param name="saturation">Grayscale value. 0 : grayscale, 1 : do not change.</param>
		/// <param name="tint">The color to multiply by target color.</param>
		/// <returns>The ColorMatrix data object with saturation and color tint.</returns>
		public static ColorMatrix SaturationTint(float saturation, Color tint) {
			ColorMatrix matrix = new ColorMatrix();
			float fr = luminance_factors.x;
			float fg = luminance_factors.y;
			float fb = luminance_factors.z;
			if (luminance_factors == Vector3.zero) {
				if (QualitySettings.activeColorSpace == ColorSpace.Linear) {
					fr = 0.2126f;
					fg = 0.7152f;
					fb = 0.0722f;
				} else {
					fr = 0.299f;
					fg = 0.587f;
					fb = 0.114f;
				}
			}
			matrix.rr = Mathf.LerpUnclamped(fr, 1f, saturation) * tint.r;
			matrix.rg = matrix.rb = Mathf.LerpUnclamped(fr, 0f, saturation) * tint.r;
			matrix.gg = Mathf.LerpUnclamped(fg, 1f, saturation) * tint.g;
			matrix.gr = matrix.gb = Mathf.LerpUnclamped(fg, 0f, saturation) * tint.g;
			matrix.bb = Mathf.LerpUnclamped(fb, 1f, saturation) * tint.b;
			matrix.br = matrix.bg = Mathf.LerpUnclamped(fb, 0f, saturation) * tint.b;
			matrix.rc = matrix.gc = matrix.bc = 0f;
			matrix.alpha = tint.a;
			return matrix;
		}

	}

}