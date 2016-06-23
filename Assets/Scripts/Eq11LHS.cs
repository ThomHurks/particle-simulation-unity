using UnityEngine;
using System.Collections;

public class Eq11LHS : ImplicitMatrix
{
	private BlockSparseMatrix m_J;
	private float[] m_W;

	public Eq11LHS(BlockSparseMatrix a_J, float[] a_W)
	{
		m_J = a_J;
		m_W = a_W;
	}

	public void MatrixTimesVector(float[] a_Source, float[] a_Destination)
	{
		int vectorSize = a_Destination.Length;
		float[] temp = new float[vectorSize];
		m_J.MatrixTransposeTimesVector(a_Source, temp);
		for (int i = 0; i < vectorSize; ++i)
		{
			temp[i] = temp[i] * m_W[i];
		}
		m_J.MatrixTimesVector(temp, a_Destination);
	}

	public void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination)
	{
		// JWJTranspose is a symmetric matrix.
		MatrixTimesVector(a_Source, a_Destination);
	}
}
