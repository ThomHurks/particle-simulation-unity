using UnityEngine;

public class Eq11LHS : ImplicitMatrix
{
    private readonly BlockSparseMatrix m_J;
    private double[] m_W;
    // = global n (2*particles)
    private readonly int m_Size;
    private double[] m_TempVector;
    // This is a symetric matrix: m=n (= global m)

    /*
	 * J = m*n
	 * W = n*n
	 * JT = n*m
	 * WJT = n*m
	 * JWJT = m*m
	*/

    public Eq11LHS(BlockSparseMatrix a_J, double[] a_W)
    {
        m_J = a_J;
        m_W = a_W;
        m_Size = a_J.getM();
        m_TempVector = new double[m_W.Length];
    }

    protected override void MatrixTimesVectorImpl(double[] a_Source, double[] a_Destination)
    {
        for (int i = 0; i < m_TempVector.Length; ++i)
        {
            m_TempVector[i] = 0;
        }
        m_J.MatrixTransposeTimesVector(a_Source, m_TempVector); //temp = JTa
        int vectorSize = a_Destination.Length;
        for (int i = 0; i < vectorSize; ++i)
        {
            m_TempVector[i] = m_TempVector[i] * m_W[i]; //temp = WTJa
        }
        m_J.MatrixTimesVector(m_TempVector, a_Destination); // a_Destination = JWJTa
    }

    protected override void MatrixTransposeTimesVectorImpl(double[] a_Source, double[] a_Destination)
    {
        // JWJTranspose is a symmetric matrix.
        MatrixTimesVector(a_Source, a_Destination);
    }

    public override int getM()
    {
        return m_Size;
    }

    public override int getN()
    {
        return m_Size;
    }

    public override double getValue(int i, int j)
    {
        double x = 0;
        for (int k = 0; k < m_J.getN(); k++)
        {
            double y = m_J.getValue(i, k);
            double z = m_W[k] * m_J.getValue(j, k);
            x += y * z;
        }
        return x;
    }

    public override void printX()
    {
        Debug.Log("J= ");
        m_J.printX();
        Debug.Log("JWJT = ");
        base.printX();
    }

}
