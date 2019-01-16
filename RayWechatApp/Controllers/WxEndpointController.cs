using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WxUtil;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Bot.Connector.DirectLine;
using System.Collections.Generic;
using RayWechatApp.Services;

namespace RayWechatApp.Controllers
{ 
    [Route("[controller]")]
    [ApiController]
    public class WxEndpointController : ControllerBase
    {
        private string WxToken;
        private string directLineSecret;
        private string botId;
        private ConversationMap conMap;
        private DirectLineClient botClient;
        public WxEndpointController(IConfiguration Configuration, ConversationMap ConMap)
        {
            WxToken = Configuration["Token"];
            directLineSecret = Configuration["DirectLineSecret"];
            botId = Configuration["BotId"];
            conMap = ConMap;
            botClient = new DirectLineClient(directLineSecret);

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
            // WX message validataion
            if (WXHelper.IsMessageFromWX(signature, nonce, timestamp, WxToken))
            {
                using (var reader = new StreamReader(Request.Body))
                {
                    var body = reader.ReadToEnd();

                    if (String.IsNullOrEmpty(body))
                    {
                        return "Failed to get message";
                    }

                    // Parse WX Message
                    WXMsg msg = WXHelper.ParseWXMsgFromBodyString(body);
                    string wxUserId = msg.FromUserName.Trim();

                    string responseXML = "";

                    // Only care about Text message
                    if (msg.MsgType == WXMsgType.Text)
                    {
                        if (!conMap.activeConversations.ContainsKey(wxUserId))
                        {
                            // Create a DirectlineCline and initialize the waltermark
                            var createdCon = await botClient.Conversations.StartConversationAsync();
                            conMap.activeConversations.Add(wxUserId, new ConversationInfo(createdCon, ""));
                        }

                        // Create a Bot Message Activity
                        Activity userMessage = new Activity
                        {
                            From = new ChannelAccount(wxUserId),
                            Text = msg.Content,
                            Type = ActivityTypes.Message
                        };
                        
                        // Post the message to Bot
                        var thisConverstaionID = conMap.activeConversations[wxUserId].Conversation.ConversationId;
                        await botClient.Conversations.PostActivityAsync(thisConverstaionID, userMessage);

                        // Get Activity Set from Bot
                        var activitySet = await botClient.Conversations.GetActivitiesAsync(thisConverstaionID, conMap.activeConversations[wxUserId].Waltermark);
                        conMap.activeConversations[wxUserId].Waltermark = activitySet.Watermark;

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
}