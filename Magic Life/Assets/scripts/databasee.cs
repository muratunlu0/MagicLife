using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class databasee : MonoBehaviour
{
    protected Firebase.Auth.FirebaseAuth auth;
    protected Firebase.Auth.FirebaseAuth otherAuth;
    protected Dictionary<string, Firebase.Auth.FirebaseUser> userByAuth =
      new Dictionary<string, Firebase.Auth.FirebaseUser>();

    public GameObject change_urun_button_obje;
    public GameObject content_change_urun;
    public GameObject malzeme_change_panel;

    public GameObject sepet_paket_button_obje;
    public GameObject content_sepet;
    public GameObject sepet_panel;

    ArrayList sepet_title_list = new ArrayList();
    ArrayList sepet_fiyat_paket_total_list = new ArrayList();
    ArrayList sepet_fiyat_paket_olmayan_urun_list = new ArrayList();
    ArrayList sepet_paket_true_mi_list= new ArrayList();

    public GameObject paket_button_obje;
    public GameObject content_paketler;
    public GameObject paketler_profile;
    public Text paket_türü;
    public Text paket_ismi;


    public GameObject ürün_button_obje;
    public GameObject content_ürünler;
    public GameObject ürünler_profile;

    public Text paket_sepet_fiyatı;
    public GameObject profilim_panel;

    public GameObject title;

    [Header("YUKLENİYOR PANELİ İCİN DEGİSKENLER")]
    public GameObject yukleniyor_paneli;

    [Header("BİLDİRİM ATMA İLE İLGİLİ DEGİSKENLER")]
    public Text bildirim_yazisi;
    public int bildirim_suresi = 2;
    public GameObject toast_mesaj_paneli;

    public void intcagir_database()
    {
        InitializeFirebase();
    }
    protected virtual void InitializeFirebase()
    {
        FirebaseApp app = FirebaseApp.DefaultInstance;
        app.SetEditorDatabaseUrl("https://magiclife-b01c6.firebaseio.com/");
        if (app.Options.DatabaseUrl != null) app.SetEditorDatabaseUrl(app.Options.DatabaseUrl);
        yukleniyor_paneli.SetActive(false);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (malzeme_change_panel.active == true)
            {
                malzeme_change_panel.SetActive(false);
            }
            else if(ürünler_profile.active == true)
            {
                ürünler_profile.SetActive(false);
            }
            else if(paketler_profile.active == true)
            {
                paketler_profile.SetActive(false);
            }
            else if (sepet_panel.active == true)
            {
                sepet_panel.SetActive(false);
            }
            else if (profilim_panel.active == true)
            {
                profilim_panel.SetActive(false);
            }
            else
            {
                Application.Quit();
            }
        }
    }
    DataSnapshot gecici_paket_data;
    public void paketleri_listele(string ürün_türü_gecici)
    {
        paket_türü.text = ürün_türü_gecici;
        paketler_profile.SetActive(true);
        panel_content_destroy(content_paketler);
        if (yukleniyor_paneli.active == false)
        {
            yukleniyor_paneli.SetActive(true);
        }
        
        Debug.Log("dersler yükleniyor...");

        FirebaseDatabase.DefaultInstance
         .GetReference("/kategoriler/").Child(paket_türü.text)
         .GetValueAsync().ContinueWith(task =>
         {
             if (task.IsFaulted)
             {
                 yukleniyor_paneli.SetActive(false);
                 bildirim_create("Akış yenilenemedi");
             }
             else if (task.IsCompleted)
             {
                 gecici_paket_data = task.Result;
                  
                 if (gecici_paket_data.Exists)
                 {
                     foreach (var paket in gecici_paket_data.Children)
                     {
                         paket_obje_create(paket.Key, gecici_paket_data.Key, paket.Child("resim_url").Value.ToString());
                     }
                 }
                 else
                 {
                     bildirim_create("Paketler bulunamadı.");
                 }
                 yukleniyor_paneli.SetActive(false);
             }
         });
    }
    public string[] urunler;
    public int[] urunler_anlık_adet;
    public float[] urunler_adet_fiyat;

    private float paket_total_fiyat;
    private float paket_gecici_urun_fiyat;
    DataSnapshot tum_urunler;
    public void ürünleri_listele(Text paket_ismi_gecici)
    {
        paket_ismi.text = paket_ismi_gecici.text;
        ürünler_profile.SetActive(true);
        paket_total_fiyat = 0;
        paket_sepet_fiyatı.text = "";
        panel_content_destroy(content_ürünler);
        if (yukleniyor_paneli.active == false)
        {
            yukleniyor_paneli.SetActive(true);
        }

        FirebaseDatabase.DefaultInstance
         .GetReference("/tüm_ürünler/").OrderByChild("marka_ismi").EqualTo(paket_ismi.text)
         .GetValueAsync().ContinueWith(task =>
         {
             if (task.IsFaulted)
             {
                 yukleniyor_paneli.SetActive(false);
                 bildirim_create("Akış yenilenemedi");
             }
             else if (task.IsCompleted)
             {
                 tum_urunler = task.Result;
                 if (tum_urunler.Exists)
                 {
                     gecici_urunler = gecici_paket_data.Child(paket_ismi.text).Child("ürünler");
                     urunler = new string[gecici_urunler.ChildrenCount];
                     urunler_anlık_adet = new int[gecici_urunler.ChildrenCount];
                     urunler_adet_fiyat = new float[gecici_urunler.ChildrenCount];
                     foreach (var ürün in tum_urunler.Children)
                     {
                         if (ürün.Child("ürün_türü").Value.ToString() == paket_türü.text)
                         {
                             foreach (var malzeme in gecici_urunler.Children)
                             {
                                 if(malzeme.Child(ürün.Key).Exists && (urunler[Convert.ToInt32(malzeme.Key)] == "" || urunler[Convert.ToInt32(malzeme.Key)] == null))
                                 {
                                     if (float.TryParse(ürün.Child("fiyat").Value.ToString(), out paket_gecici_urun_fiyat))
                                     {
                                         urunler[Convert.ToInt32(malzeme.Key)] = ürün.Key;
                                         urunler_anlık_adet[Convert.ToInt32(malzeme.Key)] = Convert.ToInt32(malzeme.Child(ürün.Key).Value.ToString());
                                         urunler_adet_fiyat[Convert.ToInt32(malzeme.Key)] = paket_gecici_urun_fiyat;

                                         paket_total_fiyat += (paket_gecici_urun_fiyat * Convert.ToInt32(malzeme.Child(ürün.Key).Value.ToString()));
                                         urun_obje_create(ürün.Child("ürün_ismi").Value.ToString(), paket_gecici_urun_fiyat, malzeme.Child(ürün.Key).Value.ToString(), ürün.Child("resim_url").Value.ToString(), Convert.ToInt32(malzeme.Key));
                                     }
                                 }
                             }
                         }
                     }
                     paket_sepet_fiyatı.text = paket_total_fiyat.ToString() + " TL {Paket}";
                 }
                 else
                 {
                     bildirim_create("Paket icerigi bulunamadı.");
                 }
                 yukleniyor_paneli.SetActive(false);
             }
         });
    }
    public void urun_degistir_yenile(Text ürün_stok_kodu)
    {
        int malzeme_index = gecici_post.GetComponent<urun_button>().malzeme_index;

        if (gecici_urunler.Child(malzeme_index.ToString()).Child(urunler[malzeme_index]).Exists)
        {
            urunler[malzeme_index] = ürün_stok_kodu.text;
            urunler_anlık_adet[malzeme_index] = Convert.ToInt32(gecici_urunler.Child(malzeme_index.ToString()).Child(ürün_stok_kodu.text).Value.ToString());
            urunler_adet_fiyat[malzeme_index] = Convert.ToInt32(tum_urunler.Child(ürün_stok_kodu.text).Child("fiyat").Value.ToString());
            
            gecici_post.transform.Find("miktar").GetComponentInChildren<Text>().text = urunler_anlık_adet[malzeme_index].ToString();
            gecici_post.transform.Find("ürün_fiyatı").GetComponentInChildren<Text>().text = urunler_adet_fiyat[malzeme_index].ToString() + " TL";
            gecici_post.transform.Find("ürün_ismi").GetComponentInChildren<Text>().text = tum_urunler.Child(ürün_stok_kodu.text).Child("ürün_ismi").Value.ToString();
            
            gecici_post.GetComponent<urun_button>().miktar = urunler_anlık_adet[malzeme_index];
            gecici_post.GetComponent<urun_button>().adet_fiyatı = urunler_adet_fiyat[malzeme_index];
            malzeme_change_panel.SetActive(false);
            paket_create(0);
        }
    }
    private GameObject gecici_post;
    public void urun_miktar_yenile(int malzeme_index, int ürün_miktarı)
    {
        urunler_anlık_adet[malzeme_index] = ürün_miktarı;
        paket_create(0);
    }
    private int gecici_malzeme_Sayisi = 0;
    DataSnapshot gecici_urunler;
    float gecici_fazladan_urun_fiyatları_total = 0;
    public void paket_create(int sepete_ekle)
    {
        gecici_fazladan_urun_fiyatları_total = 0;
        paket_total_fiyat = 0;
        gecici_malzeme_Sayisi = 0;
        for (int i = 0; i < urunler.Length; i++)
        {
            if(gecici_urunler.Child(i.ToString()).Child(urunler[i]).Exists)
            {
                paket_total_fiyat += urunler_adet_fiyat[i] * urunler_anlık_adet[i];

                if(Convert.ToInt32(gecici_urunler.Child(i.ToString()).Child(urunler[i]).Value.ToString()) < urunler_anlık_adet[i])
                {
                    gecici_malzeme_Sayisi++;
                    gecici_fazladan_urun_fiyatları_total += (urunler_anlık_adet[i] - Convert.ToInt32(gecici_urunler.Child(i.ToString()).Child(urunler[i]).Value.ToString())) * urunler_adet_fiyat[i];
                }
                else if (Convert.ToInt32(gecici_urunler.Child(i.ToString()).Child(urunler[i]).Value.ToString()) == urunler_anlık_adet[i])
                {
                    gecici_malzeme_Sayisi++;
                }
            }
        }
        if(urunler.Length == gecici_malzeme_Sayisi)
        {
            if(gecici_fazladan_urun_fiyatları_total != 0)
            {
                paket_sepet_fiyatı.text = (paket_total_fiyat - gecici_fazladan_urun_fiyatları_total).ToString() + " TL Paket  |  + " + gecici_fazladan_urun_fiyatları_total + " TL";
            }
            else
            {
                paket_sepet_fiyatı.text = (paket_total_fiyat - gecici_fazladan_urun_fiyatları_total).ToString() + " TL Paket";
            }
            if (sepete_ekle == 1)
            {
                sepet_title_list.Add(paket_ismi.text + "  |  " + paket_türü.text + "  |  " + paket_sepet_fiyatı.text);
                sepet_fiyat_paket_total_list.Add(paket_total_fiyat);
                sepet_fiyat_paket_olmayan_urun_list.Add(gecici_fazladan_urun_fiyatları_total);
                sepet_paket_true_mi_list.Add("1");
                bildirim_create("Ürünler sepete Eklendi");
            }
        }
        else
        {
            paket_sepet_fiyatı.text = paket_total_fiyat.ToString() + " TL Paket değil";
            if (sepete_ekle == 1)
            {
                sepet_title_list.Add(paket_ismi.text + "  |  " + paket_türü.text + "  |  " + paket_sepet_fiyatı.text);
                sepet_fiyat_paket_total_list.Add(paket_total_fiyat);
                sepet_fiyat_paket_olmayan_urun_list.Add("0");
                sepet_paket_true_mi_list.Add("0");
                bildirim_create("Ürünler sepete Eklendi");
            }
        }
    }
    public void sepeti_bosalt()
    {
        panel_content_destroy(content_sepet);
        sepet_title_list.Clear();
        sepet_fiyat_paket_total_list.Clear();
        sepet_fiyat_paket_olmayan_urun_list.Clear();
        sepet_paket_true_mi_list.Clear();
        bildirim_create("Sepet boşaltıldı");
    }
    public Text nakit_total_fiyat;
    public Text taksit6_aylik_fiyat;
    public Text taksit6_total_fiyat;

    public Text taksit12_aylik_fiyat;
    public Text taksit12_total_fiyat;
     
    public Text taksit18_aylik_fiyat;
    public Text taksit18_total_fiyat;

    float paket_tutari = 0;
    int paket_dışı_tutar = 0;
    int paket_sayisi;
    public void Sepeti_gör()
    {
        paket_tutari = 0;
        paket_dışı_tutar = 0;
        paket_sayisi = 0;
        nakit_total_fiyat.text = "";
        taksit6_aylik_fiyat.text = "";
        taksit6_total_fiyat.text = "";

        taksit12_aylik_fiyat.text = "";
        taksit12_total_fiyat.text = "";

        taksit18_aylik_fiyat.text = "";
        taksit18_total_fiyat.text = "";

        if (sepet_title_list.Count > 0)
        {
            panel_content_destroy(content_sepet);

            for (int i = 0; i < sepet_title_list.Count; i++)
            {
                paket_urun_sepet_create(sepet_title_list[i].ToString(), i);
                if(sepet_paket_true_mi_list[i].ToString() == "1")
                {
                    paket_sayisi++;
                    paket_tutari += Convert.ToInt32(sepet_fiyat_paket_total_list[i].ToString()) - Convert.ToInt32(sepet_fiyat_paket_olmayan_urun_list[i].ToString());
                    paket_dışı_tutar += Convert.ToInt32(sepet_fiyat_paket_olmayan_urun_list[i].ToString());
                }
                else
                {
                    paket_dışı_tutar += Convert.ToInt32(sepet_fiyat_paket_total_list[i].ToString());
                }
            }
            Debug.Log("paket_sayisi: " + paket_sayisi);

            if (paket_sayisi == 2)
            {
                paket_tutari = paket_tutari - paket_tutari * 0.05f;
                Debug.Log("paket_sayisi_2: " + paket_tutari);
            }
            else if (paket_sayisi >= 3)
            {
                paket_tutari = paket_tutari - (paket_tutari * 0.10f);
                Debug.Log("paket_sayisi_3+: " + paket_tutari);
            }

            nakit_total_fiyat.text = ((paket_tutari + paket_dışı_tutar) - (paket_tutari + paket_dışı_tutar) * 0.05f).ToString("F2") + " TL";

            taksit6_total_fiyat.text = (paket_tutari + paket_dışı_tutar).ToString("F2") + " TL";
            taksit6_aylik_fiyat.text = ((paket_tutari + paket_dışı_tutar)/6).ToString("F2") + " TL";

            float gecici_fiyat = (paket_tutari + paket_dışı_tutar) + (paket_tutari + paket_dışı_tutar) * 0.06f;

            taksit12_total_fiyat.text = gecici_fiyat.ToString("F2") + " TL";
            taksit12_aylik_fiyat.text = (gecici_fiyat / 12).ToString("F2") + " TL";

            gecici_fiyat = gecici_fiyat + gecici_fiyat * 0.06f;

            taksit18_total_fiyat.text = gecici_fiyat.ToString("F2") + " TL";
            taksit18_aylik_fiyat.text = (gecici_fiyat / 18).ToString("F2") + " TL";

        }
        else
        {
            bildirim_create("Sepette ürün yok");
        }
    }
    //ArrayList sepet_title_list = new ArrayList();
    //ArrayList sepet_fiyat_paket_total_list = new ArrayList();
    //ArrayList sepet_fiyat_paket_olmayan_urun_list = new ArrayList();
    //ArrayList sepet_paket_true_mi_list = new ArrayList();
    public void malzeme_Get_list(int malzeme_index, GameObject post)
    {
        gecici_post = post;
        if (gecici_urunler.Child(malzeme_index.ToString()).ChildrenCount > 1)
        {
            panel_content_destroy(content_change_urun);
            malzeme_change_panel.SetActive(true);
            foreach (var urun_stok_kodu in gecici_urunler.Child(malzeme_index.ToString()).Children)
            {
                change_urun_create(tum_urunler.Child(urun_stok_kodu.Key).Child("ürün_ismi").Value.ToString(), Convert.ToInt32(tum_urunler.Child(urun_stok_kodu.Key).Child("fiyat").Value.ToString()), urun_stok_kodu.Key);
            }
        }
        else
        {
            bildirim_create("Ürünün başka seçeneği yoktur");
        }
    }
    private void paket_urun_sepet_create(string ürün_ismi, int urun_list_index) // malzeme index'indeki ürünleri listeler
    {
        GameObject post = Instantiate(sepet_paket_button_obje);
        post.transform.SetParent(content_sepet.transform);
        post.transform.GetComponent<RectTransform>().localScale = new Vector3(1.02f, 1.02f, 1.02f);

        post.transform.Find("ürün_list_index").GetComponentInChildren<Text>().text = urun_list_index.ToString();
        post.transform.Find("title").GetComponentInChildren<Text>().text = ürün_ismi;
    }
    private void change_urun_create(string ürün_ismi, int ürün_fiyatı, string urun_stok_kodu) // malzeme index'indeki ürünleri listeler
    {
        GameObject post = Instantiate(change_urun_button_obje);
        post.transform.SetParent(content_change_urun.transform);
        post.transform.GetComponent<RectTransform>().localScale = new Vector3(1.02f, 1.02f, 1.02f);

        //post.GetComponent<urun_button>().malzeme_index = malzeme_indexx;
        //post.GetComponent<urun_button>().miktar = Convert.ToInt32(miktar);
        //post.GetComponent<urun_button>().adet_fiyatı = ürün_fiyatı;

        post.transform.Find("urun_stok_kodu").GetComponentInChildren<Text>().text = urun_stok_kodu;
        post.transform.Find("ürün_ismi").GetComponentInChildren<Text>().text = ürün_ismi;
        post.transform.Find("ürün_fiyatı").GetComponentInChildren<Text>().text = ürün_fiyatı.ToString() + " TL";
    }
    private void paket_obje_create(string marka_ismi,string ürün_türü, string resim_url)
    {
        GameObject post = Instantiate(paket_button_obje);
        post.transform.SetParent(content_paketler.transform);
        post.transform.GetComponent<RectTransform>().localScale = new Vector3(1.02f, 1.02f, 1.02f);

        post.transform.Find("marka_ismi").GetComponentInChildren<Text>().text = marka_ismi;
        post.transform.Find("ürün_türü").GetComponentInChildren<Text>().text = ürün_türü;
        StartCoroutine(resim_download(post.transform.Find("Image").gameObject, resim_url));
    }
    private void urun_obje_create(string ürün_ismi, float ürün_fiyatı, string miktar, string resim_url, int malzeme_indexx)
    {
        GameObject post = Instantiate(ürün_button_obje);
        post.transform.SetParent(content_ürünler.transform);
        post.transform.GetComponent<RectTransform>().localScale = new Vector3(1.02f, 1.02f, 1.02f);

        post.GetComponent<urun_button>().malzeme_index = malzeme_indexx;
        post.GetComponent<urun_button>().miktar = Convert.ToInt32(miktar);
        post.GetComponent<urun_button>().adet_fiyatı = ürün_fiyatı;

        post.transform.Find("ürün_ismi").GetComponentInChildren<Text>().text = ürün_ismi;

        post.transform.Find("ürün_fiyatı").GetComponentInChildren<Text>().text = (ürün_fiyatı* post.GetComponent<urun_button>().miktar).ToString() + " TL";
        post.transform.Find("miktar").GetComponentInChildren<Text>().text = miktar;
        StartCoroutine(resim_download(post.transform.Find("Image").gameObject, resim_url));
    }
    IEnumerator resim_download(GameObject obje, string resim_url)
    {
        WWW www = new WWW(resim_url);
        yield return www;
        if (string.IsNullOrEmpty(www.error))
        {
            obje.transform.GetComponentInChildren<RawImage>().texture = new Texture2D(512, 512, TextureFormat.RGB24, false);

            www.LoadImageIntoTexture((Texture2D)obje.transform.GetComponentInChildren<RawImage>().texture);

            TextureScale.Bilinear((Texture2D)obje.transform.GetComponentInChildren<RawImage>().texture, 512, 512);
        }
    }
    private void debug_kapat()
    {
        toast_mesaj_paneli.SetActive(false);
    }
    public void bildirim_create(string mesaj)
    {
        toast_mesaj_paneli.SetActive(true);
        bildirim_yazisi.text = mesaj;
        Invoke("debug_kapat", bildirim_suresi);
    }
    public void panel_content_destroy(GameObject content)
    {
        if (content.transform.childCount != 0)
        {
            int i = 0;
            GameObject[] allChildren = new GameObject[content.transform.childCount];
            foreach (Transform child in content.transform)
            {
                allChildren[i] = child.gameObject;
                i += 1;
            }
            foreach (GameObject child in allChildren)
            {
                if (child.transform.name != "silme")
                {
                    Destroy(child.gameObject, 0);
                }
            }
        }
    }
}

