namespace JPB.DataAccess.Query.Contracts
{
	/// <summary>
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <seealso cref="JPB.DataAccess.Query.Contracts.IQueryElement" />
	public interface IOrderdElementProducer<out T> : IQueryElement
	{
	}
}