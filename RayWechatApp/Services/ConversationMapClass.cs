using System.Collections.Generic;
using Microsoft.Bot.Connector.DirectLine;

namespace RayWechatApp.Services
{
    public class ConversationMap
    {
        public ConversationMap()
        {
            activeConversations = new Dictionary<string, ConversationInfo>();
        }
        public Dictionary<string, ConversationInfo> activeConversations { get; set; }
    }
    public class ConversationInfo
    {
        public ConversationInfo(Conversation converstaion, string waltermark)
        {
            Conversation = converstaion;
            Waltermark = waltermark;
        }
        public Conversation Conversation { get; set; }
        public string Waltermark { get; set; }
    }
}
