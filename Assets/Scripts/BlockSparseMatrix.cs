using System;
using System.Collections.Generic;

public class BlockSparseMatrix : ImplicitMatrix
{
    public class MatrixBlock
    {
        public int i;
        //constraint index (top left)
        public int j;
        //particle index (top left)
        public int iLength;
        // 2
        public int jLength;
        //constraint length
        public float[] data;

        public MatrixBlock(int a_i, int a_j, int a_iLength, int a_jLength)
        {
            i = a_i;
            j = a_j;
            iLength = a_iLength;
            jLength = a_jLength;
            data = new float[a_iLength * a_jLength];
        }
    }

    private List<MatrixBlock> m_MatrixBlocks;
    private int m_n;
    //represents an m*n matrix: m rows, n columns
    private int m_m;

    /* Indices: (i,j)
	 * ________________________
	 * |(0,0)  (0,1) ... (0,n)|
	 * |(1,0)  (1,1) ... (1,n)|
	 * |  .      .            |
	 * |  .           .       |
	 * |(m,0)  (m,1) ... (m,n)|
	 * ________________________
	 * 
	 * 0<= i < m
	 * 0<= j < n
	 * 
	 * */

    public BlockSparseMatrix()// give the ammount of particles
    {
        m_MatrixBlocks = new List<MatrixBlock>();
        m_m = 0;
        m_n = 0;//Not maintainable -> not all particles need to be constrained
    }

    public void SetN(int a_n)
    {
        m_n = a_n;
    }

    public override int getN()
    {
        return m_n;
    }

    public override int getM()
    {
        return m_m;
    }

    public MatrixBlock CreateMatrixBlock(int a_i, int a_j, int a_iLength, int a_jLength)
    {
        MatrixBlock block = new MatrixBlock(a_i, a_j, a_iLength, a_jLength);
        m_MatrixBlocks.Add(block);
        m_m = Math.Max(m_m, a_i + a_iLength);
        return block;
    }

    //a_Destination = M*a_Source
    //|a_Destination| = m && |a_Source| = n shold hold
    protected override void MatrixTimesVectorImpl(float[] a_Source, float[] a_Destination)
    {
        GenericMatrixTimesVector(a_Source, a_Destination, false);//Do a normal multiplication
    }

    protected override void MatrixTransposeTimesVectorImpl(float[] a_Source, float[] a_Destination)
    {
		GenericMatrixTimesVector(a_Source, a_Destination, true);//Do a transpose multiplication
    }

	private void GenericMatrixTimesVector(float[] a_Source, float[] a_Destination, bool transpose)
	{
		int blockCount = m_MatrixBlocks.Count;
        MatrixBlock curBlock;
        for (int index = 0; index < blockCount; ++index)
        {
            curBlock = m_MatrixBlocks[index];
            for (int j = curBlock.j; j < curBlock.jLength; ++j)
            {
                for (int i = curBlock.i; i < curBlock.iLength; ++i)
                {
					int k1 = transpose?j:i;
					int k2 = transpose?i:j;
                    a_Destination[k1] += curBlock.data[((i - curBlock.i) * curBlock.jLength) + (j - curBlock.j)] * a_Source[k2];
                    if (float.IsNaN(a_Destination[i]) || float.IsNaN(a_Destination[i]))
                    {
                        throw new System.Exception("NaN or Inf in BSM.");
                    }
                }
            }

        }
	}

}
