using Microsoft.Graph;
using System;
using System.Net.Http;
using System.Web;

namespace sentTeamMessage
{
    public class Global
    {
        public static string sValue;
        private static GraphServiceClient graphClient;
        public static string _accessToken;
        public static string sMsgValue;
        public static Microsoft.Graph.User user;
        public static Microsoft.Graph.IUserChatsCollectionPage Messages;
        public static Microsoft.Graph.User RecipientUser;
        public static string GetId;
        public static readonly HttpClient client = new HttpClient();
        public static DateTime ExpiredToken;
        public static GraphServiceClient GraphClient { get => graphClient; set => graphClient = value; }
    }
}
