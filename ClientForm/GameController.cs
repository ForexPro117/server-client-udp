using System.Collections.Generic;

namespace ClientForm
{
    enum Status
    {
        Alive,
        InDead,
        InPrison,
        Healing,
        Dead
    }
    internal class GameController
    {



        internal int stage = 0;
        internal int mafia = 0;
        internal int comisar = 0;
        internal int doctor = 0;
        internal int civil = 0;
        internal Role UserRole;
        internal List<Role> RoleList;
        List<Status> StatusList;
        internal Status playerStatus = Status.Alive;
        internal int _playerIndex;
        internal List<int> selectorList;
        bool choice = false;
        int choicePosition = -1;

        public GameController(List<Role> roleList, int playerIndex)
        {

            RoleList = roleList;
            _playerIndex = playerIndex;
            StatusList = new List<Status>();
            selectorList = new List<int>();
            foreach (Role role in roleList)
            {
                StatusList.Add(Status.Alive);
                selectorList.Add(0);
            }

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
        public void SetChoice(int position)
        {
            if (choicePosition != -1)
            {
                if (choicePosition != position && choice != false)
                {
                    selectorList[choicePosition]--;
                    choice = false;
                }
            }




            if (!choice)
            {
                selectorList[position]++;
                choice = true;
            }
            else
            {
                selectorList[position]--;
                choice = false;
            }

            choicePosition = position;
        }
        public void SetStatus(int playerPosition, Status status)
        {
            if (playerPosition == _playerIndex)
                playerStatus = status;
            StatusList[playerPosition] = status;
        }
        public bool CanAnyAction(int playerPosition)
        => this.StatusList[playerPosition] != Status.InDead &&
            this.StatusList[playerPosition] != Status.InPrison;

        public void ClearSelector()
        {
            for (int i = 0; i < selectorList.Count; i++)
            {
                selectorList[i] = 0;
            }
        }

    }
}
