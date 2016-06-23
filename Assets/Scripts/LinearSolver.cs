using UnityEngine;

public class LinearSolver
{
	private const int MAX_STEPS = 1000000000;

	// Solve Ax = b for a symmetric, positive definite matrix A
	// A is represented implicitly by the function "matVecMult"
	// which performs a matrix vector multiple Av and places result in r
	// "n" is the length of the vectors x and b
	// "epsilon" is the error tolerance
	// "steps", as passed, is the maximum number of steps, or 0 (implying MAX_STEPS)
	// Upon completion, "steps" contains the number of iterations taken
	public float ConjGrad(int n, ImplicitMatrix A, float[] x, float[] b,
		float epsilon,    // how low should we go?
		int steps, out int stepsPerformed)
	{
		int i, iMax;
		float alpha, beta, rSqrLen, rSqrLenOld, u;

		float[] r = new float[n];
		float[] d = new float[n];
		float[] t = new float[n];
		float[] temp = new float[n];

		vecAssign(n, x, b);

		vecAssign(n, r, b);
		A.MatrixTimesVector(x, temp);
		vecDiffEqual(n, r, temp);

		rSqrLen = vecSqrLen(n, r);
		vecAssign(n, d, r);

		i = 0;
		if (steps > 0 && !float.IsInfinity(steps))
		{ iMax = steps; }
		else
		{ iMax = MAX_STEPS; }

		if (rSqrLen > epsilon)
		{
			while (i < iMax)
			{
				i++;
				A.MatrixTimesVector(d, t);
				u = vecDot(n, d, t);

				if (u == 0)
				{
					Debug.Log("(SolveConjGrad) d'Ad = 0\n");
					break;
				}

				// How far should we go?
				alpha = rSqrLen / u;

				// Take a step along direction d
				vecAssign(n, temp, d);
				vecTimesScalar(n, temp, alpha);
				vecAddEqual(n, x, temp);

				if ((i & 0x3F) == 0x3F)
				{
					vecAssign(n, temp, t);
					vecTimesScalar(n, temp, alpha);
					vecDiffEqual(n, r, temp);
				}
				else
				{
					// For stability, correct r every 64th iteration
					vecAssign(n, r, b);
					A.MatrixTimesVector(x, temp);
					vecDiffEqual(n, r, temp);
				}

				rSqrLenOld = rSqrLen;
				rSqrLen = vecSqrLen(n, r);

				// Converged! Let's get out of here
				if (rSqrLen <= epsilon)
				{ break; }

				// Change direction: d = r + beta * d
				beta = rSqrLen / rSqrLenOld;
				vecTimesScalar(n, d, beta);
				vecAddEqual(n, d, r);
			}
		}
		stepsPerformed = i;
		return rSqrLen;
	}

	private void vecAddEqual(int n, float[] r, float[] v)
	{
		for (int i = 0; i < n; ++i)
		{
			r[i] = r[i] + v[i];
		}
	}

	private void vecDiffEqual(int n, float[] r, float[] v)
	{
		for (int i = 0; i < n; ++i)
		{
			r[i] = r[i] - v[i];
		}
	}

	private void vecAssign(int n, float[] v1, float[] v2)
	{
		for (int i = 0; i < n; ++i)
		{
			v1[i] = v2[i];
		}
	}

	private void vecTimesScalar(int n, float[] v, float s)
	{
		for (int i = 0; i < n; ++i)
		{
			v[i] *= s;
		}
	}

	private float vecDot(int n, float[] v1, float[] v2)
	{
		float dot = 0;
		for (int i = 0; i < n; i++)
		{
			dot += v1[i] * v2[i];
		}
		return dot;
	}

	private float vecSqrLen(int n, float[] v)
	{
		return vecDot(n, v, v);
	}
}
