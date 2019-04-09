namespace BSD.Benchmarks.Estimator
{
	public interface IAlgorithm
	{
	    void Prepare(int size);
		void Execute();
	}
}
