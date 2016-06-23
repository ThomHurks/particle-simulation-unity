public class Eq11LHS : ImplicitMatrix
{
    private BlockSparseMatrix m_J;
    private float[] m_W;
    // = global n (2*particles)
    private readonly int m_Size;
    // This is a symetric matrix: m=n (= global m)

    /*
	 * J = m*n
	 * W = n*n
	 * JT = n*m
	 * WJT = n*m
	 * JWJT = m*m
	*/

    public Eq11LHS(BlockSparseMatrix a_J, float[] a_W)
    {
        m_J = a_J;
        m_W = a_W;
        m_Size = a_J.getM();
    }

    protected override void MatrixTimesVectorImpl(float[] a_Source, float[] a_Destination)
    {
        float[] temp = new float[m_W.Length];
        m_J.MatrixTransposeTimesVector(a_Source, temp); //temp = JTa
        int vectorSize = a_Destination.Length;
        for (int i = 0; i < vectorSize; ++i)
        {
            temp[i] = temp[i] * m_W[i]; //temp = WTJa
        }
        m_J.MatrixTimesVector(temp, a_Destination); // a_Destination = JWJTa
    }

    protected override void MatrixTransposeTimesVectorImpl(float[] a_Source, float[] a_Destination)
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

}
