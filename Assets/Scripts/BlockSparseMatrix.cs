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
        //constraint length, Usually 1 (i.e. scalar constraints)
        public double[] data;

        public MatrixBlock(int a_i, int a_j, int a_iLength, int a_jLength)
        {
            i = a_i;
            j = a_j;
            iLength = a_iLength;
            jLength = a_jLength;
            data = new double[a_iLength * a_jLength];
        }
    }

    private readonly List<MatrixBlock> m_MatrixBlocks;
    private int m_n;
    //represents an m*n matrix: m rows, n columns
    private int m_m;
    // Purely as a cache if we computed it at some point:
    private ExplicitMatrix m_ExplicitMatrix;

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

    public override int GetN()
    {
        return m_n;
    }

    public override int GetM()
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
    protected override void MatrixTimesVectorImpl(double[] a_Source, double[] a_Destination)
    {
        GenericMatrixTimesVector(a_Source, a_Destination, false);//Do a normal multiplication
    }

    protected override void MatrixTransposeTimesVectorImpl(double[] a_Source, double[] a_Destination)
    {
        GenericMatrixTimesVector(a_Source, a_Destination, true);//Do a transpose multiplication
    }

    public override double GetValue(int i, int j)
    {
        for (int k = 0; k < m_MatrixBlocks.Count; k++)
        {
            MatrixBlock b = m_MatrixBlocks[k];
            if (b.i <= i && i < b.i + b.iLength && b.j <= j && j < b.j + b.jLength)
            {
                return b.data[(i - b.i) * b.jLength + j - b.j];
            }
        }
        return 0;
    }

    public override ExplicitMatrix AsExplicitMatrix()
    {
        if (m_ExplicitMatrix == null || m_ExplicitMatrix.GetM() != m_m || m_ExplicitMatrix.GetN() != m_n)
        {
            m_ExplicitMatrix = new ExplicitMatrix(m_m, m_n);
        }
        else
        {
            m_ExplicitMatrix.Clear();
        }
        MatrixBlock curBlock;
        for (int k = 0; k < m_MatrixBlocks.Count; ++k)
        {
            curBlock = m_MatrixBlocks[k];
            int starti = curBlock.i;
            int startj = curBlock.j;
            int iLength = starti + curBlock.iLength;
            int jLength = startj + curBlock.jLength;
            for (int i = starti; i < iLength; ++i)
            {
                for (int j = startj; j < jLength; ++j)
                {
                    m_ExplicitMatrix.SetValue(i, j, curBlock.data[(i - starti) * (jLength - startj) + (j - startj)]);
                }
            }
        }
        return m_ExplicitMatrix;
    }

    private void GenericMatrixTimesVector(double[] a_Source, double[] a_Destination, bool transpose)
    {
        int blockCount = m_MatrixBlocks.Count;
        MatrixBlock curBlock;
        for (int index = 0; index < blockCount; ++index)
        {
            curBlock = m_MatrixBlocks[index];
            for (int j = 0; j < curBlock.jLength; ++j)
            {
                int globj = j + curBlock.j;
                for (int i = 0; i < curBlock.iLength; ++i)
                {
                    int globi = i + curBlock.i;
                    int k1 = !transpose ? globi : globj; //dest
                    int k2 = !transpose ? globj : globi; //source
                    int cellindex = i * curBlock.jLength + j; // cell (i,j) in matrix

                    a_Destination[k1] += curBlock.data[cellindex] * a_Source[k2];
                    if (double.IsNaN(a_Destination[k1]) || double.IsInfinity(a_Destination[k1]))
                    {
                        throw new Exception("NaN or Inf in BSM.");
                    }
                }
            }

        }
    }

    public void Clear()
    {
        m_m = 0;
        m_n = 0;
        m_MatrixBlocks.Clear();
        m_ExplicitMatrix = null;
    }
}
