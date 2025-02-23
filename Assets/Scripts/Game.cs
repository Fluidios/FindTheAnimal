using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public abstract class Game : MonoBehaviour
    {
        public abstract bool GameSaveExists {get;}
        public Action<bool> OnGameEnds;
        public abstract void StartGameLevel();
        public abstract void LoadGameLevelFromSave();
        public abstract void ClearGameLevel();
        public abstract void ClearSave();
    }
}
