
  
#region --- License ---

/*
Copyright (c) 2006 - 2008 The Open Toolkit library.
Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software without restriction, including without limitation the rights to
use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies
of the Software, and to permit persons to whom the Software is furnished to do
so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
 */

#endregion --- License ---

using System;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Mathd_Lib
{
	/// <summary>Represents a 4D vector using four double-precision floating-point numbers.</summary>
	[Serializable]
	[StructLayout(LayoutKind.Sequential)]
	public struct Vector4d : IEquatable<Vector4d>
	{
		#region Fields

		/// <summary>
		/// The X component of the Vector4d.
		/// </summary>
		public double x;

		/// <summary>
		/// The Y component of the Vector4d.
		/// </summary>
		public double y;

		/// <summary>
		/// The Z component of the Vector4d.
		/// </summary>
		public double z;

		/// <summary>
		/// The W component of the Vector4d.
		/// </summary>
		public double w;

		/// <summary>
		/// Defines a unit-length Vector4d that points towards the X-axis.
		/// </summary>
		public static Vector4d UnitX = new Vector4d(1, 0, 0, 0);

		/// <summary>
		/// Defines a unit-length Vector4d that points towards the Y-axis.
		/// </summary>
		public static Vector4d UnitY = new Vector4d(0, 1, 0, 0);

		/// <summary>
		/// Defines a unit-length Vector4d that points towards the Z-axis.
		/// </summary>
		public static Vector4d UnitZ = new Vector4d(0, 0, 1, 0);

		/// <summary>
		/// Defines a unit-length Vector4d that points towards the W-axis.
		/// </summary>
		public static Vector4d UnitW = new Vector4d(0, 0, 0, 1);

		/// <summary>
		/// Defines a zero-length Vector4d.
		/// </summary>
		public static Vector4d Zero = new Vector4d(0, 0, 0, 0);

		/// <summary>
		/// Defines an instance with all components set to 1.
		/// </summary>
		public static readonly Vector4d One = new Vector4d(1, 1, 1, 1);

		/// <summary>
		/// Defines the size of the Vector4d struct in bytes.
		/// </summary>
		public static readonly int SizeInBytes = Marshal.SizeOf(new Vector4d());

		#endregion Fields

		#region Constructors

		/// <summary>
		/// Constructs a new Vector4d.
		/// </summary>
		/// <param name="x">The x component of the Vector4d.</param>
		/// <param name="y">The y component of the Vector4d.</param>
		/// <param name="z">The z component of the Vector4d.</param>
		/// <param name="w">The w component of the Vector4d.</param>
		public Vector4d(double x, double y, double z, double w)
		{
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}

		/// <summary>
		/// Constructs a new Vector4d from the given Vector2d.
		/// </summary>
		/// <param name="v">The Vector2d to copy components from.</param>
		public Vector4d(Vector2d v)
		{
			x = v.x;
			y = v.y;
			z = 0.0f;
			w = 0.0f;
		}

		/// <summary>
		/// Constructs a new Vector4d from the given Vector3d.
		/// </summary>
		/// <param name="v">The Vector3d to copy components from.</param>
		public Vector4d(Vector3d v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = 0.0f;
		}

		/// <summary>
		/// Constructs a new Vector4d from the specified Vector3d and w component.
		/// </summary>
		/// <param name="v">The Vector3d to copy components from.</param>
		/// <param name="w">The w component of the new Vector4.</param>
		public Vector4d(Vector3d v, double w)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			this.w = w;
		}

		/// <summary>
		/// Constructs a new Vector4d from the given Vector4d.
		/// </summary>
		/// <param name="v">The Vector4d to copy components from.</param>
		public Vector4d(Vector4d v)
		{
			x = v.x;
			y = v.y;
			z = v.z;
			w = v.w;
		}

		#endregion Constructors

		public static Vector4d Parse(string s)
		{
			var result = Vector4d.Zero;

			var values = s.Split(',').Select(sValue =>
			{
				double.TryParse(sValue, out double number);
				return number;
			}).ToArray();

			for (int i = 0; i < Math.Min(4, values.Length); i++)
			{
				result[i] = values[i];
			}

			return result;
		}


		#region Public Members

		#region Properties

		public double this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return x;

					case 1:
						return y;

					case 2:
						return z;

					case 3:
						return w;

					default:
						return 0;
				}
			}

			set
			{
				switch (index)
				{
					case 0:
						x = value;
						break;

					case 1:
						y = value;
						break;

					case 2:
						z = value;
						break;

					case 3:
						w = value;
						break;

					default:
						throw new Exception();
				}
			}
		}

		#endregion Properties

		#region Instance

		#region public double Length

		/// <summary>
		/// Gets the length (magnitude) of the vector.
		/// </summary>
		/// <see cref="LengthFast"/>
		/// <seealso cref="LengthSquared"/>
		public double Length
		{
			get
			{
				return System.Math.Sqrt(x * x + y * y + z * z + w * w);
			}
		}

		#endregion public double Length

		#region public double LengthSquared

		/// <summary>
		/// Gets the square of the vector length (magnitude).
		/// </summary>
		/// <remarks>
		/// This property avoids the costly square root operation required by the Length property. This makes it more suitable
		/// for comparisons.
		/// </remarks>
		/// <see cref="Length"/>
		public double LengthSquared
		{
			get
			{
				return x * x + y * y + z * z + w * w;
			}
		}

		#endregion public double LengthSquared

		#region public void Normalize()

		/// <summary>
		/// Scales the Vector4d to unit length.
		/// </summary>
		public void Normalize()
		{
			double scale = 1.0 / this.Length;
			x *= scale;
			y *= scale;
			z *= scale;
			w *= scale;
		}

		#endregion public void Normalize()

		public bool IsValid()
		{
			if(double.IsNaN(x) || double.IsInfinity(x)
				|| double.IsNaN(y) || double.IsInfinity(y)
				|| double.IsNaN(z) || double.IsInfinity(z)
				|| double.IsNaN(w) || double.IsInfinity(w))
			{
				return false;
			}

			return true;
		}

		#endregion Instance

		#region Static

		#region Add

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="a">Left operand.</param>
		/// <param name="b">Right operand.</param>
		/// <returns>Result of operation.</returns>
		public static Vector4d Add(Vector4d a, Vector4d b)
		{
			Add(ref a, ref b, out a);
			return a;
		}

		/// <summary>
		/// Adds two vectors.
		/// </summary>
		/// <param name="a">Left operand.</param>
		/// <param name="b">Right operand.</param>
		/// <param name="result">Result of operation.</param>
		public static void Add(ref Vector4d a, ref Vector4d b, out Vector4d result)
		{
			result = new Vector4d(a.x + b.x, a.y + b.y, a.z + b.z, a.w + b.w);
		}

		#endregion Add

		#region Subtract

		/// <summary>
		/// Subtract one Vector from another
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>Result of subtraction</returns>
		public static Vector4d Subtract(Vector4d a, Vector4d b)
		{
			Subtract(ref a, ref b, out a);
			return a;
		}

		/// <summary>
		/// Subtract one Vector from another
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <param name="result">Result of subtraction</param>
		public static void Subtract(ref Vector4d a, ref Vector4d b, out Vector4d result)
		{
			result = new Vector4d(a.x - b.x, a.y - b.y, a.z - b.z, a.w - b.w);
		}

		#endregion Subtract

		#region Multiply

		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <returns>Result of the operation.</returns>
		public static Vector4d Multiply(Vector4d vector, double scale)
		{
			Multiply(ref vector, scale, out vector);
			return vector;
		}

		/// <summary>
		/// Multiplies a vector by a scalar.
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Multiply(ref Vector4d vector, double scale, out Vector4d result)
		{
			result = new Vector4d(vector.x * scale, vector.y * scale, vector.z * scale, vector.w * scale);
		}

		/// <summary>
		/// Multiplies a vector by the components a vector (scale).
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <returns>Result of the operation.</returns>
		public static Vector4d Multiply(Vector4d vector, Vector4d scale)
		{
			Multiply(ref vector, ref scale, out vector);
			return vector;
		}

		/// <summary>
		/// Multiplies a vector by the components of a vector (scale).
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Multiply(ref Vector4d vector, ref Vector4d scale, out Vector4d result)
		{
			result = new Vector4d(vector.x * scale.x, vector.y * scale.y, vector.z * scale.z, vector.w * scale.w);
		}

		#endregion Multiply

		#region Divide

		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <returns>Result of the operation.</returns>
		public static Vector4d Divide(Vector4d vector, double scale)
		{
			Divide(ref vector, scale, out vector);
			return vector;
		}

		/// <summary>
		/// Divides a vector by a scalar.
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Divide(ref Vector4d vector, double scale, out Vector4d result)
		{
			Multiply(ref vector, 1 / scale, out result);
		}

		/// <summary>
		/// Divides a vector by the components of a vector (scale).
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <returns>Result of the operation.</returns>
		public static Vector4d Divide(Vector4d vector, Vector4d scale)
		{
			Divide(ref vector, ref scale, out vector);
			return vector;
		}

		/// <summary>
		/// Divide a vector by the components of a vector (scale).
		/// </summary>
		/// <param name="vector">Left operand.</param>
		/// <param name="scale">Right operand.</param>
		/// <param name="result">Result of the operation.</param>
		public static void Divide(ref Vector4d vector, ref Vector4d scale, out Vector4d result)
		{
			result = new Vector4d(vector.x / scale.x, vector.y / scale.y, vector.z / scale.z, vector.w / scale.w);
		}

		#endregion Divide

		#region Min

		/// <summary>
		/// Calculate the component-wise minimum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>The component-wise minimum</returns>
		public static Vector4d Min(Vector4d a, Vector4d b)
		{
			a.x = a.x < b.x ? a.x : b.x;
			a.y = a.y < b.y ? a.y : b.y;
			a.z = a.z < b.z ? a.z : b.z;
			a.w = a.w < b.w ? a.w : b.w;
			return a;
		}

		/// <summary>
		/// Calculate the component-wise minimum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <param name="result">The component-wise minimum</param>
		public static void Min(ref Vector4d a, ref Vector4d b, out Vector4d result)
		{
			result.x = a.x < b.x ? a.x : b.x;
			result.y = a.y < b.y ? a.y : b.y;
			result.z = a.z < b.z ? a.z : b.z;
			result.w = a.w < b.w ? a.w : b.w;
		}

		#endregion Min

		#region Max

		/// <summary>
		/// Calculate the component-wise maximum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <returns>The component-wise maximum</returns>
		public static Vector4d Max(Vector4d a, Vector4d b)
		{
			a.x = a.x > b.x ? a.x : b.x;
			a.y = a.y > b.y ? a.y : b.y;
			a.z = a.z > b.z ? a.z : b.z;
			a.w = a.w > b.w ? a.w : b.w;
			return a;
		}

		/// <summary>
		/// Calculate the component-wise maximum of two vectors
		/// </summary>
		/// <param name="a">First operand</param>
		/// <param name="b">Second operand</param>
		/// <param name="result">The component-wise maximum</param>
		public static void Max(ref Vector4d a, ref Vector4d b, out Vector4d result)
		{
			result.x = a.x > b.x ? a.x : b.x;
			result.y = a.y > b.y ? a.y : b.y;
			result.z = a.z > b.z ? a.z : b.z;
			result.w = a.w > b.w ? a.w : b.w;
		}

		#endregion Max

		#region Clamp

		/// <summary>
		/// Clamp a vector to the given minimum and maximum vectors
		/// </summary>
		/// <param name="vec">Input vector</param>
		/// <param name="min">Minimum vector</param>
		/// <param name="max">Maximum vector</param>
		/// <returns>The clamped vector</returns>
		public static Vector4d Clamp(Vector4d vec, Vector4d min, Vector4d max)
		{
			vec.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
			vec.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
			vec.z = vec.x < min.z ? min.z : vec.z > max.z ? max.z : vec.z;
			vec.w = vec.y < min.w ? min.w : vec.w > max.w ? max.w : vec.w;
			return vec;
		}

		/// <summary>
		/// Clamp a vector to the given minimum and maximum vectors
		/// </summary>
		/// <param name="vec">Input vector</param>
		/// <param name="min">Minimum vector</param>
		/// <param name="max">Maximum vector</param>
		/// <param name="result">The clamped vector</param>
		public static void Clamp(ref Vector4d vec, ref Vector4d min, ref Vector4d max, out Vector4d result)
		{
			result.x = vec.x < min.x ? min.x : vec.x > max.x ? max.x : vec.x;
			result.y = vec.y < min.y ? min.y : vec.y > max.y ? max.y : vec.y;
			result.z = vec.z < min.z ? min.z : vec.z > max.z ? max.z : vec.z;
			result.w = vec.y < min.w ? min.w : vec.w > max.w ? max.w : vec.w;
		}

		#endregion Clamp

		#region Normalize

		/// <summary>
		/// Scale a vector to unit length
		/// </summary>
		/// <param name="vec">The input vector</param>
		/// <returns>The normalized vector</returns>
		public static Vector4d Normalize(Vector4d vec)
		{
			double scale = 1.0 / vec.Length;
			vec.x *= scale;
			vec.y *= scale;
			vec.z *= scale;
			vec.w *= scale;
			return vec;
		}

		/// <summary>
		/// Scale a vector to unit length
		/// </summary>
		/// <param name="vec">The input vector</param>
		/// <param name="result">The normalized vector</param>
		public static void Normalize(ref Vector4d vec, out Vector4d result)
		{
			double scale = 1.0 / vec.Length;
			result.x = vec.x * scale;
			result.y = vec.x * scale;
			result.z = vec.z * scale;
			result.w = vec.w * scale;
		}

		#endregion Normalize

		#region Dot

		/// <summary>
		/// Calculate the dot product of two vectors
		/// </summary>
		/// <param name="left">First operand</param>
		/// <param name="right">Second operand</param>
		/// <returns>The dot product of the two inputs</returns>
		public static double Dot(Vector4d left, Vector4d right)
		{
			return left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w;
		}

		/// <summary>
		/// Calculate the dot product of two vectors
		/// </summary>
		/// <param name="left">First operand</param>
		/// <param name="right">Second operand</param>
		/// <param name="result">The dot product of the two inputs</param>
		public static void Dot(ref Vector4d left, ref Vector4d right, out double result)
		{
			result = left.x * right.x + left.y * right.y + left.z * right.z + left.w * right.w;
		}

		#endregion Dot

		#region Lerp

		/// <summary>
		/// Returns a new Vector that is the linear blend of the 2 given Vectors
		/// </summary>
		/// <param name="a">First input vector</param>
		/// <param name="b">Second input vector</param>
		/// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
		/// <returns>a when blend=0, b when blend=1, and a linear combination otherwise</returns>
		public static Vector4d Lerp(Vector4d a, Vector4d b, double blend)
		{
			a.x = blend * (b.x - a.x) + a.x;
			a.y = blend * (b.y - a.y) + a.y;
			a.z = blend * (b.z - a.z) + a.z;
			a.w = blend * (b.w - a.w) + a.w;
			return a;
		}

		/// <summary>
		/// Returns a new Vector that is the linear blend of the 2 given Vectors
		/// </summary>
		/// <param name="a">First input vector</param>
		/// <param name="b">Second input vector</param>
		/// <param name="blend">The blend factor. a when blend=0, b when blend=1.</param>
		/// <param name="result">a when blend=0, b when blend=1, and a linear combination otherwise</param>
		public static void Lerp(ref Vector4d a, ref Vector4d b, double blend, out Vector4d result)
		{
			result.x = blend * (b.x - a.x) + a.x;
			result.y = blend * (b.y - a.y) + a.y;
			result.z = blend * (b.z - a.z) + a.z;
			result.w = blend * (b.w - a.w) + a.w;
		}

		#endregion Lerp

		#region Barycentric

		/// <summary>
		/// Interpolate 3 Vectors using Barycentric coordinates
		/// </summary>
		/// <param name="a">First input Vector</param>
		/// <param name="b">Second input Vector</param>
		/// <param name="c">Third input Vector</param>
		/// <param name="u">First Barycentric Coordinate</param>
		/// <param name="v">Second Barycentric Coordinate</param>
		/// <returns>a when u=v=0, b when u=1,v=0, c when u=0,v=1, and a linear combination of a,b,c otherwise</returns>
		public static Vector4d BaryCentric(Vector4d a, Vector4d b, Vector4d c, double u, double v)
		{
			return a + u * (b - a) + v * (c - a);
		}

		/// <summary>Interpolate 3 Vectors using Barycentric coordinates</summary>
		/// <param name="a">First input Vector.</param>
		/// <param name="b">Second input Vector.</param>
		/// <param name="c">Third input Vector.</param>
		/// <param name="u">First Barycentric Coordinate.</param>
		/// <param name="v">Second Barycentric Coordinate.</param>
		/// <param name="result">Output Vector. a when u=v=0, b when u=1,v=0, c when u=0,v=1, and a linear combination of a,b,c otherwise</param>
		public static void BaryCentric(ref Vector4d a, ref Vector4d b, ref Vector4d c, double u, double v, out Vector4d result)
		{
			result = a; // copy

			Vector4d temp = b; // copy
			Subtract(ref temp, ref a, out temp);
			Multiply(ref temp, u, out temp);
			Add(ref result, ref temp, out result);

			temp = c; // copy
			Subtract(ref temp, ref a, out temp);
			Multiply(ref temp, v, out temp);
			Add(ref result, ref temp, out result);
		}

		#endregion Barycentric

		#region Transform

		/// <summary>Transform a Vector by the given Matrix</summary>
		/// <param name="vec">The vector to transform</param>
		/// <param name="mat">The desired transformation</param>
		/// <returns>The transformed vector</returns>
		public static Vector4d Transform(Vector4d vec, Matrix4x4 mat)
		{
			Vector4d result;
			Transform(vec, ref mat, out result);
			return result;
		}

		/// <summary>Transform a Vector by the given Matrix</summary>
		/// <param name="vec">The vector to transform</param>
		/// <param name="mat">The desired transformation</param>
		/// <param name="result">The transformed vector</param>
		public static void Transform(Vector4d vec, ref Matrix4x4 mat, out Vector4d result)
		{
			result = new Vector4d(
				vec.x * mat.GetRow(0).x + vec.y * mat.GetRow(1).x + vec.z * mat.GetRow(2).x + vec.w * mat.GetRow(3).x,
				vec.x * mat.GetRow(0).y + vec.y * mat.GetRow(1).y + vec.z * mat.GetRow(2).y + vec.w * mat.GetRow(3).y,
				vec.x * mat.GetRow(0).z + vec.y * mat.GetRow(1).z + vec.z * mat.GetRow(2).z + vec.w * mat.GetRow(3).z,
				vec.x * mat.GetRow(0).w + vec.y * mat.GetRow(1).w + vec.z * mat.GetRow(2).w + vec.w * mat.GetRow(3).w);
		}

		/// <summary>
		/// Transforms a vector by a quaternion rotation.
		/// </summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <returns>The result of the operation.</returns>
		public static Vector4d Transform(Vector4d vec, Quaterniond quat)
		{
			Vector4d result;
			Transform(ref vec, ref quat, out result);
			return result;
		}

		/// <summary>
		/// Transforms a vector by a quaternion rotation.
		/// </summary>
		/// <param name="vec">The vector to transform.</param>
		/// <param name="quat">The quaternion to rotate the vector by.</param>
		/// <param name="result">The result of the operation.</param>
		public static void Transform(ref Vector4d vec, ref Quaterniond quat, out Vector4d result)
		{
			Quaterniond v = new Quaterniond(vec.x, vec.y, vec.z, vec.w), i, t;
			Quaterniond.Invert(ref quat, out i);
			Quaterniond.Multiply(ref quat, ref v, out t);
			Quaterniond.Multiply(ref t, ref i, out v);

			result = new Vector4d(v.X, v.Y, v.Z, v.W);
		}

		#endregion Transform

		#endregion Static

		#region Swizzle

		/// <summary>
		/// Gets or sets an OpenTK.Vector2d with the X and Y components of this instance.
		/// </summary>
		public Vector2d Xy { get { return new Vector2d(x, y); } set { x = value.x; y = value.y; } }

		/// <summary>
		/// Gets or sets an OpenTK.Vector3d with the X, Y and Z components of this instance.
		/// </summary>
		public Vector3d Xyz { get { return new Vector3d(x, y, z); } set { x = value.x; y = value.y; z = value.z; } }

		#endregion Swizzle

		#region Operators

		/// <summary>
		/// Adds two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Vector4d operator +(Vector4d left, Vector4d right)
		{
			left.x += right.x;
			left.y += right.y;
			left.z += right.z;
			left.w += right.w;
			return left;
		}

		/// <summary>
		/// Subtracts two instances.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Vector4d operator -(Vector4d left, Vector4d right)
		{
			left.x -= right.x;
			left.y -= right.y;
			left.z -= right.z;
			left.w -= right.w;
			return left;
		}

		/// <summary>
		/// Negates an instance.
		/// </summary>
		/// <param name="vec">The instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Vector4d operator -(Vector4d vec)
		{
			vec.x = -vec.x;
			vec.y = -vec.y;
			vec.z = -vec.z;
			vec.w = -vec.w;
			return vec;
		}

		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="vec">The instance.</param>
		/// <param name="scale">The scalar.</param>
		/// <returns>The result of the calculation.</returns>
		public static Vector4d operator *(Vector4d vec, double scale)
		{
			vec.x *= scale;
			vec.y *= scale;
			vec.z *= scale;
			vec.w *= scale;
			return vec;
		}

		/// <summary>
		/// Multiplies an instance by a scalar.
		/// </summary>
		/// <param name="scale">The scalar.</param>
		/// <param name="vec">The instance.</param>
		/// <returns>The result of the calculation.</returns>
		public static Vector4d operator *(double scale, Vector4d vec)
		{
			vec.x *= scale;
			vec.y *= scale;
			vec.z *= scale;
			vec.w *= scale;
			return vec;
		}

		/// <summary>
		/// Divides an instance by a scalar.
		/// </summary>
		/// <param name="vec">The instance.</param>
		/// <param name="scale">The scalar.</param>
		/// <returns>The result of the calculation.</returns>
		public static Vector4d operator /(Vector4d vec, double scale)
		{
			double mult = 1 / scale;
			vec.x *= mult;
			vec.y *= mult;
			vec.z *= mult;
			vec.w *= mult;
			return vec;
		}

		/// <summary>
		/// Compares two instances for equality.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>True, if left equals right; false otherwise.</returns>
		public static bool operator ==(Vector4d left, Vector4d right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two instances for inequality.
		/// </summary>
		/// <param name="left">The first instance.</param>
		/// <param name="right">The second instance.</param>
		/// <returns>True, if left does not equa lright; false otherwise.</returns>
		public static bool operator !=(Vector4d left, Vector4d right)
		{
			return !left.Equals(right);
		}

		#endregion Operators

		#region Overrides

		#region public override string ToString()

		/// <summary>
		/// Returns a System.String that represents the current Vector4d.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", x, y, z, w);
		}

		/// <summary>
		/// Returns a System.String that represents the current Vector4d, formatting each element with format.
		/// </summary>
		/// <param name="format"></param>
		/// <returns></returns>
		public string ToString(string format = "")
		{
			return x.ToString(format) + ", " + y.ToString(format) + ", " + z.ToString(format) + ", " + w.ToString(format);
		}

		#endregion public override string ToString()

		#region public override int GetHashCode()

		/// <summary>
		/// Returns the hashcode for this instance.
		/// </summary>
		/// <returns>A System.Int32 containing the unique hashcode for this instance.</returns>
		public override int GetHashCode()
		{
			return new { x, y, z, w }.GetHashCode();
		}

		public static ulong GetLongHashCode(double data, ulong hash = 14695981039346656037)
		{
			return ComputeHash(BitConverter.GetBytes(data), hash);
		}

		// FNV-1a (64-bit) non-cryptographic hash function.
		// Adapted from: http://github.com/jakedouglas/fnv-java
		public static ulong ComputeHash(byte[] bytes, ulong hash = 14695981039346656037)
		{
			const ulong fnv64Prime = 0x100000001b3;

			for (var i = 0; i < bytes.Length; i++)
			{
				hash = hash ^ bytes[i];
				hash *= fnv64Prime;
			}

			return hash;
		}

		/// <summary>
		/// return a 64 bit hash code proposed by Jon Skeet
		// http://stackoverflow.com/questions/8094867/good-gethashcode-override-for-list-of-foo-objects-respecting-the-order
		/// </summary>
		/// <returns></returns>
		public ulong GetLongHashCode(ulong hash = 14695981039346656037)
		{
			hash = GetLongHashCode(x, hash);
			hash = GetLongHashCode(y, hash);
			hash = GetLongHashCode(z, hash);
			hash = GetLongHashCode(w, hash);

			return hash;
		}

		#endregion public override int GetHashCode()

		#region public override bool Equals(object obj)

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <param name="obj">The object to compare to.</param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public override bool Equals(object obj)
		{
			if (!(obj is Vector4d))
				return false;

			return this.Equals((Vector4d)obj);
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal within an error range.
		/// </summary>
		/// <param name="OtherVector"></param>
		/// <param name="ErrorValue"></param>
		/// <returns>True if the instances are equal; false otherwise.</returns>
		public bool Equals(Vector4d OtherVector, double ErrorValue)
		{
			if ((x < OtherVector.x + ErrorValue && x > OtherVector.x - ErrorValue) &&
				(y < OtherVector.y + ErrorValue && y > OtherVector.y - ErrorValue) &&
				(z < OtherVector.z + ErrorValue && z > OtherVector.z - ErrorValue) &&
				(w < OtherVector.w + ErrorValue && w > OtherVector.w - ErrorValue))
			{
				return true;
			}

			return false;
		}

		#endregion public override bool Equals(object obj)

		#endregion Overrides

		#endregion Public Members

		#region IEquatable<Vector4d> Members

		/// <summary>Indicates whether the current vector is equal to another vector.</summary>
		/// <param name="other">A vector to compare with this vector.</param>
		/// <returns>true if the current vector is equal to the vector parameter; otherwise, false.</returns>
		public bool Equals(Vector4d other)
		{
			return
				x == other.x &&
				y == other.y &&
				z == other.z &&
				w == other.w;
		}

		#endregion IEquatable<Vector4d> Members
	}
}