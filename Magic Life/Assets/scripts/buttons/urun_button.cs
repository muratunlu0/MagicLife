using UnityEngine;
using UnityEngine.UI;

public class urun_button : MonoBehaviour
{

	public int miktar = 0;
	public float adet_fiyatı = 0;
	public int malzeme_index = -1;
	public void miktar_art()
	{
		miktar++;
		miktar_islem();
	}
	public void miktar_azalt()
	{
		if (miktar > 0)
		{
			miktar--;
			miktar_islem();
		}
	}
	private void miktar_islem()
	{
		if (miktar == 0)
		{
			gameObject.transform.Find("miktar").GetComponentInChildren<Text>().text = "-";
			gameObject.transform.Find("ürün_fiyatı").GetComponentInChildren<Text>().text = "--- TL";
		}
		else
		{
			gameObject.transform.Find("miktar").GetComponentInChildren<Text>().text = miktar.ToString();
			gameObject.transform.Find("ürün_fiyatı").GetComponentInChildren<Text>().text = (miktar * adet_fiyatı).ToString() + " TL";
		}
		GameObject.Find("firebase-message").GetComponent<databasee>().urun_miktar_yenile(malzeme_index, miktar);
	}
	public void malzeme_get_list()
	{
		GameObject.Find("firebase-message").GetComponent<databasee>().malzeme_Get_list(malzeme_index, gameObject);
	}
}
