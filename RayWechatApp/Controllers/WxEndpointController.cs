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

namespace RayWechatApp.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WxEndpointController : ControllerBase
    {
        private string WxToken;
        public WxEndpointController(IConfiguration Configuration)
        {
            WxToken = Configuration["Token"];
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
        public string WxPost([FromQuery]string signature, [FromQuery]string nonce, [FromQuery]string timestamp)
        {
            ////if (IsMessageFromWX(signature, nonce, timestamp, WxToken))
            ////{
            ////}
            using (var reader = new StreamReader(Request.Body))
            {
                var body = reader.ReadToEnd();
                if(String.IsNullOrEmpty(body))
                {
                    return "Failed to get message";
                }
                
                WXMsg msg = WXHelper.ParseWXMsgFromBodyString(body);
                string wxId = msg.FromUserName;
                string responseXML = "";

                if (msg.MsgType == WXMsgType.Text)
                {
                    responseXML = WXHelper.ConstructWXTextMessage(msg, $"你说是{msg.Content}");
                }
                return responseXML;
            }
        }
    }

}