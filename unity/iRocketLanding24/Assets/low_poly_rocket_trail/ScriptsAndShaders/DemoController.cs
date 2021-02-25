using UnityEngine;
using System.Collections;

public class DemoController : MonoBehaviour {

	public Transform[] rocket_prefab = new Transform[3];
	private float apply_force_timer = 0f;
	private Transform cur_rocket ;
	private int cur_rocket_n=4;

	// Use this for initialization
	void Awake () {
		Init();
	}

	void OnGUI(){
		if (GUI.Button(new Rect(50,50,100,20),"Restart"))
			Init();
		if (GUI.Button(new Rect(250,50,50,20),"<-")){
			cur_rocket_n--;
			if (cur_rocket_n<0)
				cur_rocket_n = 5;
			Init();
		}
		GUI.Label(new Rect(325,50,150,20),cur_rocket_n==0 ? "Basic exhaust" : cur_rocket_n==1 ? "Basic exhaust with glow" :  cur_rocket_n==2 ? "no smoke" :  cur_rocket_n==3 ? "no smoke with glow":  cur_rocket_n==4 ? "Long trail" :"Long trail with glow");
		if (GUI.Button(new Rect(500,50,50,20),"->")){
			cur_rocket_n++;
			if (cur_rocket_n>5)
				cur_rocket_n = 0;
			Init();
		}

		
	}
		

	void Init(){
		if (cur_rocket!=null)
			Destroy(cur_rocket.gameObject);
		
		cur_rocket = Instantiate(rocket_prefab[cur_rocket_n],new Vector3(0f,0f,0f),Random.rotation) as Transform;
		//cur_rocket = Instantiate(rocket_prefab[cur_rocket_n],new Vector3(0f,0f,0f),Quaternion.identity) as Transform;
		//cur_rocket.transform.eulerAngles = new Vector3(0f,90f,0f);
		cur_rocket.GetComponent<Rigidbody>().velocity = new Vector3(0f,0f,0f);

		//this.transform.eulerAngles = force_dir;
		apply_force_timer = 3f;
	}
	// Update is called once per frame
	void Update () {
		if (apply_force_timer>0f){
			cur_rocket.GetComponent<Rigidbody>().AddForce(cur_rocket.transform.forward*100f);
			//cur_rocket.GetComponent<Rigidbody>().AddForce(cur_rocket.transform.up*15f);

			//cur_rocket.GetComponent<Rigidbody>().AddTorque(-cur_rocket.transform.right*.5f);
			apply_force_timer-=Time.deltaTime;
		}
	}
}
