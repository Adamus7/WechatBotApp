// Copyright joji@microsoft.com

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace WxUtil
{
    public enum WXMsgType
    {
        Event,
        Text,
        Unknown
    }
    public class WXMsg
    {
        public string ToUserName { get; set; }
        public string FromUserName { get; set; }
        public int CreateTime { get; set; }
        public WXMsgType MsgType { get; set; }
        public string Content { get; set; }
        public string MsgId { get; set; }
        public string Event { get; set; }
        public string EventKey { get; set; }
    }
    public class WXMultipleNewsMessage
    {
        public List<WXSingleNewsMessage> NewsMessages { get; set; }
        public int ArticleCount
        {
            get
            {
                return NewsMessages.Count;
            }
        }
        public WXMultipleNewsMessage()
        {
            NewsMessages = new List<WXSingleNewsMessage>();
        }
        public void AddMessage(WXSingleNewsMessage message)
        {
            NewsMessages.Add(message);
        }
    }
    public class WXSingleNewsMessage
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string PicUrl { get; set; }
        public string Url { get; set; }
        public WXSingleNewsMessage(string title, string description, string picUrl, string url)
        {
            Title = title;
            Description = description;
            Url = url;
            if (!picUrl.ToLower().StartsWith("http") && picUrl != "")
            {
                PicUrl = $"http://csssh.eastasia.cloudapp.azure.com/static/images/{picUrl}";
            }
            else
            {
                PicUrl = picUrl;
            }
        }
    }
    public static class WXHelper
    {
        public static bool IsMessageFromWX(string signature, string nonce, string timestamp, string wxToken)
        {
            string[] tempArr = { nonce, timestamp, wxToken };
            Array.Sort(tempArr);
            string tempStr = string.Join("", tempArr);
            var sha1 = SHA1.Create();
            byte[] hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(tempStr));
            string calculatedSignature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            return calculatedSignature == signature;
        }
        public static string ConstructWXTextMessage(WXMsg msg, string replyText)
        {
            var createTime = Math.Round((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
            string xml = $"<xml><ToUserName><![CDATA[{msg.FromUserName}]]></ToUserName><FromUserName><![CDATA[{msg.ToUserName}]]></FromUserName><CreateTime>{createTime}</CreateTime><MsgType><![CDATA[text]]></MsgType><Content><![CDATA[{replyText}]]></Content></xml>";
            return xml;
        }
        public static string ConstructWXNewsMessage(WXMsg msg, WXMultipleNewsMessage newsMessages)
        {
            var itemString = "";
            for (int i = 0; i < newsMessages.ArticleCount; i++)
            {
                itemString += $"<item><Title><![CDATA[{newsMessages.NewsMessages[i].Title}]]></Title> <Description><![CDATA[{newsMessages.NewsMessages[i].Description}]]></Description><PicUrl><![CDATA[{newsMessages.NewsMessages[i].PicUrl}]]></PicUrl><Url><![CDATA[{newsMessages.NewsMessages[i].Url}]]></Url></item>";
            }
            var createTime = Math.Round((DateTime.Now.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalSeconds);
            string xml = $"<xml><ToUserName><![CDATA[{msg.FromUserName}]]></ToUserName><FromUserName><![CDATA[{msg.ToUserName}]]></FromUserName><CreateTime>{createTime }</CreateTime><MsgType><![CDATA[news]]></MsgType><ArticleCount>{newsMessages.ArticleCount}</ArticleCount><Articles>{itemString}</Articles></xml>";
            return xml;
        }
        public static WXMsg ParseWXMsgFromBodyString(string body)
        {
            WXMsg msg = new WXMsg();
            var doc = new XmlDocument();
            doc.LoadXml(body);
            var toUserName = doc.SelectSingleNode("/xml/ToUserName").InnerText;
            var fromUserName = doc.SelectSingleNode("/xml/FromUserName").InnerText;
            var createTime = int.Parse(doc.SelectSingleNode("/xml/CreateTime").InnerText);
            var msgTypeString = doc.SelectSingleNode("/xml/MsgType").InnerText;
            msg.ToUserName = toUserName;
            msg.FromUserName = fromUserName;
            msg.CreateTime = createTime;
            switch (msgTypeString)
            {
                case "event":
                    msg.MsgType = WXMsgType.Event;
                    msg.Event = doc.SelectSingleNode("/xml/Event").InnerText;
                    msg.EventKey = doc.SelectSingleNode("/xml/EventKey").InnerText;
                    break;
                case "text":
                    msg.MsgType = WXMsgType.Text;
                    msg.Content = doc.SelectSingleNode("/xml/Content").InnerText;
                    msg.MsgId = doc.SelectSingleNode("/xml/MsgId").InnerText;
                    break;
                default:
                    msg.MsgType = WXMsgType.Unknown;
                    break;
            }
            return msg;
        }
    }
}
