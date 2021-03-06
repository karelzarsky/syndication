using Newtonsoft.Json.Linq;

namespace xAPI.Commands
{
	using JSONObject = JObject;

	public class SymbolCommand : BaseCommand
	{
		public SymbolCommand(JSONObject arguments, bool prettyPrint) : base(arguments, prettyPrint)
		{
		}

		public override string CommandName
		{
			get
			{
				return "getSymbol";
			}
		}

		public override string[] RequiredArguments
		{
			get
			{
				return new[]{"symbol"};
			}
		}
	}
}