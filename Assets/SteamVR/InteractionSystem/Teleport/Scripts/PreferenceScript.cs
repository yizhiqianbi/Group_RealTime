using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*public enum Preference
{
    Close = 1,
    Normal = 2,
    Far = 3,
    //STEP = 1
}*/
public class PreferenceScript : NetworkBehaviour
{
    [SyncVar]
    private Preference preference = Preference.Normal;

    [Command]
    public void CmdChangePreference(Preference preference)
    {
        this.preference = preference;
    } 

    public Preference GetPreference()
    {
        return preference;
    }
}
