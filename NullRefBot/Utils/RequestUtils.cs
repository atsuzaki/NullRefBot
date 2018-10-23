using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using RestSharp;

namespace NullRefBot.Utils
{
    class RequestUtils
    {
		public static async Task<T> ExecuteAsync<T> ( RestRequest req ) where T : new() {
			var client = new RestClient();
			client.BaseUrl = new Uri( Bot.Instance.Config.DatabaseIP );

			var response = await client.ExecuteTaskAsync<T>( req );

			if( response.ErrorException != null ) {
				const string message = "Error retrieving response.  Check inner details for more info.";
				var twilioException = new ApplicationException( message, response.ErrorException );
				throw twilioException;
			}

			return response.Data;
		}

        //Returns the whole response instead of just the data. Better naming?
		public static async Task<IRestResponse<T>> ExecuteAsyncRaw<T> ( RestRequest req ) where T : new() {
			var client = new RestClient();
			client.BaseUrl = new Uri( Bot.Instance.Config.DatabaseIP );

			return await client.ExecuteTaskAsync<T>( req );
		}

        //TODO: temp
        public static async Task<IRestResponse> ExecuteAsyncRawTemp ( RestRequest req ) {
			var client = new RestClient();
			client.BaseUrl = new Uri( Bot.Instance.Config.DatabaseIP );

			return await client.ExecuteTaskAsync( req );
		}

    }
}
