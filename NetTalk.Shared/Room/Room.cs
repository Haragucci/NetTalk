using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTalk.Shared
{
    public class Room
    {
        public string RoomId { get; set; }
        public string Name { get; set; }
        public string Creator { get; set; }
        public List<string> Operators { get; set; }
        public List<string> BannedUsers { get; set; }
        public List<string> JoinedUsers { get; set; }

        public bool Exists(string userId)
        {
            foreach (var JoinedID in JoinedUsers)
            {
                if (JoinedID == userId)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
