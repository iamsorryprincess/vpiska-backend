using System;
using System.Collections.Generic;
using System.Linq;
using Vpiska.Domain.Event.Models;

namespace Vpiska.IntegrationTests.Event
{
    public sealed class EventEqualityComparer : IEqualityComparer<Domain.Event.Event>
    {
        public bool Equals(Domain.Event.Event? x, Domain.Event.Event? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id == y.Id &&
                   x.OwnerId == y.OwnerId &&
                   x.Name == y.Name &&
                   x.Address == y.Address &&
                   CheckCoordinates(x.Coordinates, y.Coordinates) &&
                   CheckMediaLinks(x.MediaLinks, y.MediaLinks) &&
                   CheckChatData(x.ChatData, y.ChatData) &&
                   CheckUsersInfo(x.Users, y.Users);
        }

        public int GetHashCode(Domain.Event.Event obj)
        {
            return HashCode.Combine(obj.Id, obj.OwnerId, obj.Name, obj.Address, obj.Coordinates, obj.MediaLinks, obj.ChatData, obj.Users);
        }

        private static bool CheckCoordinates(Coordinates coordinates1, Coordinates coordinates2) =>
            Math.Abs(coordinates1.X - coordinates2.X) < Math.E &&
            Math.Abs(coordinates1.Y - coordinates2.Y) < Math.E;

        private static bool CheckMediaLinks(List<string> mediaLinks1, List<string> mediaLinks2) =>
            mediaLinks1.Count == mediaLinks2.Count &&
            mediaLinks1.All(mediaLinks2.Contains);

        private static bool CheckChatData(List<ChatMessage> chatMessages1, List<ChatMessage> chatMessages2) =>
            chatMessages1.Count == chatMessages2.Count &&
            chatMessages1.All(a => chatMessages2.Exists(b => CheckChatData(a, b)));

        private static bool CheckUsersInfo(List<UserInfo> userInfos1, List<UserInfo> userInfos2) =>
            userInfos1.Count == userInfos2.Count &&
            userInfos1.All(a => userInfos2.Exists(b => CheckUserInfo(a, b)));

        private static bool CheckChatData(ChatMessage chatMessage1, ChatMessage chatMessage2) =>
            chatMessage1.UserId == chatMessage2.UserId &&
            chatMessage1.UserName == chatMessage2.UserName &&
            chatMessage1.UserImageId == chatMessage2.UserImageId &&
            chatMessage1.Message == chatMessage2.Message;

        private static bool CheckUserInfo(UserInfo userInfo1, UserInfo userInfo2) => userInfo1.UserId == userInfo2.UserId;
    }
}