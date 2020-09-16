using Microsoft.AspNetCore.Http;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using sentTeamMessage.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace sentTeamMessage.Controllers
{
    public class ValuesController : ApiController
    {
        public static void getNewTokenIfNeeded()
        {

            var now = DateTime.Now;
            var Expired = DateTime.Compare(now, Global.ExpiredToken);
            if (Expired > 0)
            {
                string appId = ConfigurationManager.AppSettings["ida:AppId"];
                string appSecret = ConfigurationManager.AppSettings["ida:AppSecret"];
                string redirectUri = ConfigurationManager.AppSettings["ida:RedirectUri"];
                string authority = "https://login.microsoftonline.com/72ff950c-ba3d-469e-a595-cff0b6d8e621";
                string resource = "https://graph.microsoft.com";
                string clientId = appId;
                string _userName = "tal.qvtest@qnvision.com";
                string password = "A6v53kr4.";
                UserPasswordCredential userPasswordCredential = new UserPasswordCredential(_userName, password);
                AuthenticationContext authContext = new AuthenticationContext(authority);
                var result = authContext.AcquireTokenAsync(resource, clientId, userPasswordCredential).Result;
                Global.ExpiredToken = result.ExpiresOn.ToLocalTime().DateTime;
                Global.GraphClient = new GraphServiceClient(
                    new DelegateAuthenticationProvider(
                        (requestMessage) =>
                        {
                            Global._accessToken = authContext.AcquireTokenSilentAsync(resource, clientId).Result.AccessToken;
                            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", Global._accessToken);
                            return Task.FromResult(0);
                        }));
            }
        }
        private async Task<string> getConversationId(string RecipientUserId)
        {
            Global.Messages = await Global.GraphClient.Me.Chats.Request().GetAsync();
            var aMessagesEnum = Global.Messages.GetEnumerator();
            aMessagesEnum.MoveNext();
            var oMessage = aMessagesEnum.Current;
            var sId = "Not Found";
            while (oMessage != null)
            {
                var SearchForsId = oMessage.Id;
                if (SearchForsId.Contains(RecipientUserId))
                {
                    return SearchForsId;
                    
                }
                aMessagesEnum.MoveNext();
                oMessage = aMessagesEnum.Current;
            }
            if (sId.Equals("Not Found"))
            {
                //Error - thread id Was not found
                return "error";
            }
            return "";
        }
        // GET api/values
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }
        [Route("api/Values/{id}/{UserName}/{msgText}")]
        public async Task<CachedUser> Get(int id, string userName, string msgText)
        {
            getNewTokenIfNeeded();
            Global.sValue = userName + "@qnvision.com";

            Global.RecipientUser = await Global.GraphClient.Users[Global.sValue].Request().Select(u => new {
                u.DisplayName,
                u.Mail,
                u.UserPrincipalName,
                u.Id
            })
                .GetAsync();
            var RecipientUserId = Global.RecipientUser.Id;
            

            //var ConversationId = "19:16ff31b8-93b0-478e-9b9e-56b27ff76e1c_1f36f515-6c6d-4798-bd50-7be3abaaadf5@unq.gbl.spaces";
            var ConversationId = await getConversationId(RecipientUserId);
            var chatMessage = new ChatMessage
            {
                Body = new ItemBody
                {
                    Content = msgText
                }
            };
            await Global.GraphClient.Users[Global.user.Id]
                             .Chats[ConversationId]
                             .Messages
                             .Request()
                             .AddAsync(chatMessage);
            return new CachedUser
            {
                Avatar = string.Empty,
                ID = Global.user.Id,
                DisplayName = Global.user.DisplayName,
                Email = string.IsNullOrEmpty(Global.user.Mail) ?
                     Global.user.UserPrincipalName : Global.user.Mail
            };
        }

        // POST api/values
       
        public void Post([FromBody] string value)
        {
        }
        [Route("api/Values/{id}/{UserName}")]
        public async Task<IHttpActionResult> PostChatMessage([FromBody]ChatMessage chat,string userName) 
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid data.");
            }
            chat.Body = new ItemBody
            {
                ContentType = BodyType.Html,
                Content = "<a href="+chat.Summary+">Press here</a> to approve or decline"
            };

            getNewTokenIfNeeded();
            Global.sValue = userName + "@qnvision.com";
            Global.RecipientUser = await Global.GraphClient.Users[Global.sValue].Request().Select(u => new {
                u.DisplayName,
                u.Mail,
                u.UserPrincipalName,
                u.Id
            }).GetAsync();
            var RecipientUserId = Global.RecipientUser.Id;
            var ConversationId = await getConversationId(RecipientUserId);
            await Global.GraphClient.Users[Global.user.Id]
                 .Chats[ConversationId]
                 .Messages
                 .Request()
                 .AddAsync(chat);

            return Ok();

        }
        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
