﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMgr : MonoBehaviour {

    public void OnClickStartBtn()
    {
        Debug.Log("Click Button");
        Application.LoadLevel("scLevel01");
        Application.LoadLevelAdditive("scPlay");
    }

}
