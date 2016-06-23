using System;
using System.Collections.Generic;

public class BlockSparseMatrix : ImplicitMatrix
{
    public class MatrixBlock
    {
        public int i;
        public int j;
        public int iLength;
        public int jLength;
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

    public BlockSparseMatrix()// give the ammount of particles
    {
        m_MatrixBlocks = new List<MatrixBlock>();
        m_m = 0;
        m_n = 0;
    }

    public MatrixBlock CreateMatrixBlock(int a_i, int a_j, int a_iLength, int a_jLength)
    {
        MatrixBlock block = new MatrixBlock(a_i, a_j, a_iLength, a_jLength);
        m_MatrixBlocks.Add(block);
        m_m = Math.Max(m_m, a_j + a_jLength);
        return block;
    }

    //a_Destination = M*a_Source
    //|a_Destination| = m && |a_Source| = n shold hold
    public void MatrixTimesVector(float[] a_Source, float[] a_Destination)
    {
        if (a_Destination.Length != m_m)
        {
            throw new Exception("Output size wrong!");
        }

        VerifyValidVectors(a_Source, a_Destination);
        int blockCount = m_MatrixBlocks.Count;
        MatrixBlock curBlock;
        for (int index = 0; index < blockCount; ++index)
        {
            curBlock = m_MatrixBlocks[index];
            for (int j = curBlock.j; j < curBlock.jLength; ++j)
            {
                for (int i = curBlock.i; i < curBlock.iLength; ++i)
                {
                    a_Destination[i] += curBlock.data[((i - curBlock.i) * curBlock.jLength) + (j - curBlock.j)] * a_Source[j];
                    if (float.IsNaN(a_Destination[i]) || float.IsNaN(a_Destination[i]))
                    {
                        throw new System.Exception("NaN or Inf in BSM.");
                    }
                }
            }

        }
    }


    public void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination)
    {
        VerifyValidVectors(a_Source, a_Destination);
        int blockCount = m_MatrixBlocks.Count;
        MatrixBlock curBlock;
        for (int index = 0; index < blockCount; ++index)
        {
            curBlock = m_MatrixBlocks[index];
            for (int j = curBlock.j; j < curBlock.jLength; ++j)
            {
                for (int i = curBlock.i; i < curBlock.iLength; ++i)
                {
                    a_Destination[j] += curBlock.data[((i - curBlock.i) * curBlock.jLength) + (j - curBlock.j)] * a_Source[i];
                    if (float.IsNaN(a_Destination[i]) || float.IsNaN(a_Destination[i]))
                    {
                        throw new System.Exception("NaN or Inf in BSM.");
                    }
                }
            }

        }
    }

    private void VerifyValidVectors(float[] a_Source, float[] a_Destination)
    {
        for (int i = 0; i < a_Source.Length; ++i)
        {
            if (float.IsNaN(a_Source[i]) || float.IsInfinity(a_Source[i]))
            {
                throw new System.Exception("Source vector did not validate: NaN or Inf found.");
            }
        }
        for (int i = 0; i < a_Destination.Length; ++i)
        {
            if (a_Destination[i] != 0)
            {
                throw new System.Exception("Destination vector was not zero");
            }
        }
    }
}
