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

	public BlockSparseMatrix()
	{
		m_MatrixBlocks = new List<MatrixBlock>();
	}

	public MatrixBlock CreateMatrixBlock(int a_i, int a_j, int a_iLength, int a_jLength)
	{
		MatrixBlock block = new MatrixBlock(a_i, a_j, a_iLength, a_jLength);
		m_MatrixBlocks.Add(block);
		return block;
	}

	public void MatrixTimesVector(float[] a_Source, float[] a_Destination)
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
					a_Destination[i] += curBlock.data[((i - curBlock.i) * curBlock.jLength) + (j - curBlock.j)] * a_Source[j];
				}
			}

		}
	}

	public void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination)
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
					a_Destination[j] += curBlock.data[((i - curBlock.i) * curBlock.jLength) + (j - curBlock.j)] * a_Source[i];
				}
			}

		}
	}
}
