public abstract class ImplicitMatrix
{
    abstract void MatrixTimesVector2(float[] a_Source, float[] a_Destination);

    abstract void MatrixTransposeTimesVector2(float[] a_Source, float[] a_Destination);

	abstract int getM();

	abstract int getN();

	//Perform safetychecks in super class
    void MatrixTimesVector(float[] a_Source, float[] a_Destination)
	{
		if (a_Destination.Length != this.getM())
        {
            throw new Exception("Output size wrong! Should be " + m_m + ",but is " + a_Destination.Length);
        }
		if (a_Source.Length != this.getN())
        {
            throw new Exception("Input size wrong! Should be " + m_n + ",but is " + a_Source.Length);
        }

        VerifyValidVectors(a_Source, a_Destination);
		MatrixTimesVector2( a_Source, a_Destination);
	}


	void MatrixTransposeTimesVector(float[] a_Source, float[] a_Destination)
	{
		if (a_Destination.Length != this.getN())
        {
            throw new Exception("Output size wrong! Should be " + m_n + ",but is " + a_Destination.Length);
        }
		if (a_Source.Length != this.getM())
        {
            throw new Exception("Input size wrong! Should be " + m_m + ",but is " + a_Source.Length);
        }

        VerifyValidVectors(a_Source, a_Destination);
		MatrixTransposeTimesVector2(a_Source, a_Destination);
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
