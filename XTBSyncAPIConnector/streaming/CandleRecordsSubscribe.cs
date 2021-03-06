﻿using Newtonsoft.Json.Linq;

namespace xAPI.Streaming
{
    using JSONObject = JObject;

    class CandleRecordsSubscribe
    {
        private string symbol;
        private string streamSessionId;
        
        public CandleRecordsSubscribe(string symbol, string streamSessionId)
        {
            this.streamSessionId = streamSessionId;
            this.symbol = symbol;
        }

        public override string ToString()
        {
            JSONObject result = new JSONObject();
            result.Add("command", "getCandles");
            result.Add("streamSessionId", streamSessionId);
            result.Add("symbol", symbol);
            return result.ToString();
        }
    }
}
