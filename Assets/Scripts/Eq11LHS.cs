using UnityEngine;

public class Eq11LHS : ImplicitMatrix
{
    private readonly BlockSparseMatrix m_J;
    private double[] m_W;
    // = global n (2*particles)
    private readonly int m_Size;
    private double[] m_TempVector;
    // Purely as a cache if we computed it at some point:
    private ExplicitMatrix m_ExplicitMatrix;


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
        m_Size = a_J.GetM();//=m
        m_TempVector = new double[m_W.Length];//=n
    }

    protected override void MatrixTimesVectorImpl(double[] a_Source, double[] a_Destination)
    {
        for (int i = 0; i < m_TempVector.Length; ++i)
        {
            m_TempVector[i] = 0;
        }
        m_J.MatrixTransposeTimesVector(a_Source, m_TempVector); //temp = JTa
        for (int i = 0; i < m_W.Length; ++i)
        {
            m_TempVector[i] = m_TempVector[i] * m_W[i]; //temp = WJTa
        }
        m_J.MatrixTimesVector(m_TempVector, a_Destination); // a_Destination = JWJTa
    }

    protected override void MatrixTransposeTimesVectorImpl(double[] a_Source, double[] a_Destination)
    {
        // JWJTranspose is a symmetric matrix.
        MatrixTimesVector(a_Source, a_Destination);
    }

    public override int GetM()
    {
        return m_Size;
    }

    public override int GetN()
    {
        return m_Size;
    }

    public override ExplicitMatrix AsExplicitMatrix()
    {
        if (m_ExplicitMatrix == null || m_ExplicitMatrix.GetM() != m_Size || m_ExplicitMatrix.GetN() != m_Size)
        {
            m_ExplicitMatrix = new ExplicitMatrix(m_Size, m_Size);
        }
        else
        {
            m_ExplicitMatrix.Clear();
        }
        ExplicitMatrix J_Explicit = m_J.AsExplicitMatrix();

        int jn = J_Explicit.GetN();
        for (int i = 0; i < m_Size; ++i)
        {
            for (int j = i; j < m_Size; ++j)
            {
                double x = 0;
                for (int k = 0; k < jn; ++k)
                {
                    double y = J_Explicit.GetValue(i, k);
                    double z = m_W[k] * J_Explicit.GetValue(j, k);
                    x += y * z;
                }
                m_ExplicitMatrix.SetValue(i, j, x);
                m_ExplicitMatrix.SetValue(j, i, x);
            }
        }
        return m_ExplicitMatrix;
    }

    public override double GetValue(int i, int j)
    {
        throw new System.NotImplementedException();
    }

    public override void printX()
    {
        Debug.Log("J= ");
        m_J.printX();
        Debug.Log("JWJT = ");
        base.printX();
    }

}
