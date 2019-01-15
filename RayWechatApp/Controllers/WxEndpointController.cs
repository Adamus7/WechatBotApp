using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using WxUtil;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.DirectLine;


namespace RayWechatApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WxEndpointController : ControllerBase
    {
        private string WxToken;
        private string directLineSecret;
        private string botId;
        private string fromUser;
        //private DirectLineClient botClient;
        //private Conversation testCon;
        public WxEndpointController(IConfiguration Configuration)
        {
            WxToken = Configuration["Token"];
            directLineSecret = Configuration["DirectLineSecret"];
            botId = Configuration["BotId"];
            fromUser = Configuration["fromUser"];
            myConMgr.botClient = new DirectLineClient(directLineSecret);
        }

        [HttpGet("")]
        public string ReturnEchostr([FromQuery]string signature, [FromQuery]string nonce, [FromQuery]string timestamp, [FromQuery]string echostr)
        {
            if (WXHelper.IsMessageFromWX(signature, nonce, timestamp, WxToken))
            {
                return echostr;
            }
            else
            {
                return "Failed to authenticate the request";
            }
        }
        [HttpPost("")]
        public async Task<string> WxPost([FromQuery]string signature, [FromQuery]string nonce, [FromQuery]string timestamp)
        {
            if (WXHelper.IsMessageFromWX(signature, nonce, timestamp, WxToken) || true)
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = reader.ReadToEnd();
                    if (String.IsNullOrEmpty(body))
                    {
                        return "Failed to get message";
                    }

                    WXMsg msg = WXHelper.ParseWXMsgFromBodyString(body);
                    string wxId = msg.FromUserName;
                    string responseXML = "";

                    if (msg.MsgType == WXMsgType.Text)
                    {
                        if (myConMgr.testCon == null)
                        {
                            myConMgr.testCon = await myConMgr.botClient.Conversations.StartConversationAsync();
                        }
                        Activity userMessage = new Activity
                        {
                            From = new ChannelAccount(fromUser),
                            Text = msg.Content,
                            Type = ActivityTypes.Message
                        };
                        await myConMgr.botClient.Conversations.PostActivityAsync(myConMgr.testCon.ConversationId, userMessage);

                        var activitySet = await myConMgr.botClient.Conversations.GetActivitiesAsync(myConMgr.testCon.ConversationId, null);
                        var activities = from x in activitySet.Activities
                                         where x.From.Id == botId
                                         select x;
                        var returnString = "";
                        foreach (Activity activity in activities)
                        {
                            returnString += activity.Text + "||";
                        }
                        responseXML = WXHelper.ConstructWXTextMessage(msg, returnString);
                    }
                    return responseXML;
                }
            }
            else
            {
                return "";
            }
        }

    }

    public static class myConMgr
    {
        public static DirectLineClient botClient;
        public static Conversation testCon;
    }
}