using UnityEngine;

public abstract class ImplicitMatrix
{
    // These functions must be implemented in the inheriting classes.
    protected abstract void MatrixTimesVectorImpl(float[] a_Source, float[] a_Destination);

    protected abstract void MatrixTransposeTimesVectorImpl(float[] a_Source, float[] a_Destination);

    public abstract int getM();

    public abstract int getN();

	public abstract float getValue (int i, int j);

    // Perform safety checks in abstract class.
    public void MatrixTimesVector(float[] a_Source, float[] a_Destination)
    {
        VerifyCorrectSize(a_Source, a_Destination);
        VerifyValidValues(a_Source);
        MatrixTimesVectorImpl(a_Source, a_Destination);
    }

    public void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination)
    {
        VerifyCorrectTransposeSize(a_Source, a_Destination);
        VerifyValidValues(a_Source);
        MatrixTransposeTimesVectorImpl(a_Source, a_Destination);
    }

    private void VerifyCorrectSize(float[] a_Source, float[] a_Destination)
    {
        if (a_Destination.Length != getM())
        {
            throw new System.Exception("Output size wrong! Should be " + getM() + ",but is " + a_Destination.Length);
        }
        if (a_Source.Length != this.getN())
        {
            throw new System.Exception("Input size wrong! Should be " + getN() + ",but is " + a_Source.Length);
        }
    }

    private void VerifyCorrectTransposeSize(float[] a_Source, float[] a_Destination)
    {
        if (a_Destination.Length != getN())
        {
            throw new System.Exception("Output size wrong! Should be " + getN() + ",but is " + a_Destination.Length);
        }
        if (a_Source.Length != getM())
        {
            throw new System.Exception("Input size wrong! Should be " + getM() + ",but is " + a_Source.Length);
        }
    }

    private void VerifyValidValues(float[] a_Source)
    {
        for (int i = 0; i < a_Source.Length; ++i)
        {
            if (float.IsNaN(a_Source[i]) || float.IsInfinity(a_Source[i]))
            {
                throw new System.Exception("Source vector did not validate: NaN or Inf found.");
            }
        }
    }

	public virtual void printX()
	{
		string[,] xs = new string[getM(), getN()];
		for (int i = 0; i < getM(); i++)
		{
			for (int j = 0; j < getN(); j++)
			{
				xs [i, j] = "" + getValue (i, j).ToString() + ", ";
			}
		}

		string x = "";
		for (int i = 0; i < getM(); i++)
		{
			string line = "";
			for (int j = 0; j < getN(); j++)
			{
				line = line + xs[i, j];
			}
			x = x + line + "\n";
		}
		Debug.Log(x);
	}
}
