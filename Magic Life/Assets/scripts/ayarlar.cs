using UnityEngine;

public class ayarlar : MonoBehaviour
{
    void Start()
    {
        Screen.fullScreen = false;
    }
    public void instagram()
    {
        Application.OpenURL("https://www.instagram.com/vip_ata_aof/");
    }
    public void facebook()
    {
        Application.OpenURL("https://www.facebook.com/groups/atauniaofcikmissorular/");
    }
    public void puan_ver()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.app.examm");
    }
    public void website()
    {
        Application.OpenURL("http://www.zaferfotokopi.com/");
    }
}