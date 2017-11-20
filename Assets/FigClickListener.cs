using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FigClickListener : MonoBehaviour, IPointerClickHandler {
  public int frame_jump;
  ControllerScript cs;

	// Use this for initialization
	void Start () {
    cs = GameObject.Find("MasterController").GetComponent<ControllerScript>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

  void IPointerClickHandler.OnPointerClick(PointerEventData eventData) {
    cs.jump_to_frame(frame_jump);
  }
}
