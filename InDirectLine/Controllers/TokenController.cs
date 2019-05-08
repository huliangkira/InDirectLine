
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Itminus.InDirectLine.Services.IDirectLineConnections;
using Itminus.InDirectLine.Models;
using Itminus.InDirectLine.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace Itminus.InDirectLine.Controllers{

    [ApiController]
    public class TokenController : Controller
    {
        private ILogger<TokenController> _logger;
        private readonly DirectLineHelper _helper;
        private readonly IDirectLineConnectionManager _connectionManager;
        private readonly TokenBuilder _tokenBuilder;
        private InDirectLineOptions _inDirectlineOption;

        public TokenController(ILogger<TokenController> logger, IOptions<InDirectLineOptions> opt, DirectLineHelper helper, IDirectLineConnectionManager connectionManager,TokenBuilder tokenBuilder)
        {
            this._logger= logger;
            this._helper = helper;
            this._connectionManager = connectionManager;
            this._tokenBuilder = tokenBuilder;
            this._inDirectlineOption = opt.Value;
        }


        [HttpPost("v3/directline/[controller]/generate")]
        public IActionResult Generate()
        {
            // according to https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-direct-line-3-0-authentication?view=azure-bot-service-4.0#generate-token-versus-start-conversation
            //     we don't start a conversation , we just issue a new token that is valid for specific conversation
            var conversationId = Guid.NewGuid().ToString();
            var claims = new List<Claim>();
            var expiresIn = this._inDirectlineOption.TokenExpiresIn;
            var token =  this._tokenBuilder.BuildToken(conversationId,claims, expiresIn);
            return new OkObjectResult(new DirectLineConversation{
                ConversationId = conversationId,
                Token = token,
                ExpiresIn = expiresIn,
            });
        }


        [Authorize]
        [HttpPost("v3/directline/[controller]/refresh")]
        public IActionResult Refresh()
        {
            var conversationId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == TokenBuilder.ClaimTypeConversationID).Value;
            var claims = new List<Claim>();
            var expiresIn = this._inDirectlineOption.TokenExpiresIn;
            var token =  this._tokenBuilder.BuildToken(conversationId,claims, expiresIn);
            return new OkObjectResult(new DirectLineConversation{
                ConversationId = conversationId,
                Token = token,
                ExpiresIn = expiresIn,
            });
        }


    }

}