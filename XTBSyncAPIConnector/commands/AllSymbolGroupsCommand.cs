using System;
using Newtonsoft.Json.Linq;

namespace xAPI.Commands
{
    using JSONObject = JObject;

    public class AllSymbolGroupsCommand : BaseCommand
    {
        [Obsolete("Not available in API any more")]
        public AllSymbolGroupsCommand(bool? prettyPrint)
            : base(new JSONObject(), prettyPrint)
        {
        }

        public override string CommandName
        {
            get { return "getAllSymbolGroups"; }
        }

        public override string[] RequiredArguments
        {
            get { return new string[] { }; }
        }
    }

}