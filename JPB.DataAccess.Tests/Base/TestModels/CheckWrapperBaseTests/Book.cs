using JPB.DataAccess.ModelsAnotations;

namespace JPB.DataAccess.Tests.Base.TestModels.CheckWrapperBaseTests
{
	public class Book
	{
		[PrimaryKey]
		public int BookId { get; set; }

		public string BookName { get; set; }
	}
}