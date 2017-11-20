using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OcclusionController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
  Image interface_occlussion;
  enum State { visible, fading_out, fading_in, invisible };
  State state;
	// Use this for initialization
	void Start () {
		interface_occlussion = GameObject.Find("InterfaceOcclussion").GetComponent<Image>();
    state = State.visible;
  }
	
	// Update is called once per frame
	void Update () {
    update_visibility();
	}

  void update_visibility() {
    float delta_time = Time.deltaTime;
    Color c = interface_occlussion.color;

    if (state == State.fading_in) {
      float alpha = c.a - delta_time * 0.02f;
      interface_occlussion.color = new Color(c.r, c.g, c.b, alpha);
      Debug.Log("in " + alpha);
      if (alpha <= 0f) {
        state = State.visible;
        //interface_occlussion.transform.Translate(new Vector3(0f, -200f, 0f));
      }
    } else if (state == State.fading_out) {
      Debug.Log(delta_time);
      Debug.Log(c.a);
      float alpha = c.a - delta_time * 0.05f;
      interface_occlussion.color = new Color(c.r, c.g, c.b, alpha);
      Debug.Log("out " + alpha);
      if (alpha >= 1.0f) {
        state = State.invisible;
      }
    }
  }

  void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData) {
    Color c = interface_occlussion.color;
    interface_occlussion.color = new Color(c.r, c.g, c.b, 0f);

    state = State.fading_in;
  }

  void IPointerExitHandler.OnPointerExit(PointerEventData eventData) {
    //interface_occlussion.transform.Translate(new Vector3(0f, 200f, 0f));
    Color c = interface_occlussion.color;
    interface_occlussion.color = new Color(c.r, c.g, c.b, 1f);

    state = State.fading_out;
  }
}
