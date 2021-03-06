﻿using Newtonsoft.Json.Linq;

namespace xAPI.Streaming
{
    using JSONObject = JObject;

    class TradeRecordsSubscribe
    {
        private string streamSessionId;

        public TradeRecordsSubscribe(string streamSessionId)
        {
            this.streamSessionId = streamSessionId;
        }

        public override string ToString()
        {
            JSONObject result = new JSONObject();
            result.Add("command", "getTrades");
            result.Add("streamSessionId", streamSessionId);
            return result.ToString();
        }
    }
}
