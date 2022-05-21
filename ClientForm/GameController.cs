using System.Collections.Generic;
using System.Linq;

namespace ClientForm
{
    enum Status
    {
        Alive,
        InDead,
        Healing,
        Dead
    }
    internal class GameController
    {



        internal int stage = 0;
        int mafia = 0;
        int comisar = 0;
        int doctor = 0;
        int civil = 0;
        internal Role UserRole;
        internal List<Role> RoleList;
        List<Status> StatusList;
        internal int _playerIndex;

        public GameController(List<Role> roleList, int playerIndex)
        {

            RoleList = roleList;
            _playerIndex = playerIndex;
            StatusList = new List<Status>();
            foreach (Role role in roleList)
                StatusList.Add(Status.Alive);

            UserRole = RoleList[_playerIndex];
            if (RoleList.Count < 6)
            {
                mafia = 1;
                comisar = 1;
                doctor = 1;
                civil = RoleList.Count - 3;
            }
            else
            {
                {
                    mafia = 2;
                    comisar = 1;
                    doctor = 1;
                    civil = RoleList.Count - 4;
                }
            }
        }


    }
}
