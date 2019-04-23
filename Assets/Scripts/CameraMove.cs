using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
	public float moveSpeed = 5.0f;
	public float zoomSpeed = 3.0f;

	Vector3 startPos;
	float startZoom;

	// Use this for initialization
	void Start()
	{
		startPos = transform.position;
		startZoom = GetComponent<Camera>().orthographicSize;
	}

	// Update is called once per frame
	void Update()
	{
		float currZoom = GetComponent<Camera>().orthographicSize;
		Vector3 delta = new Vector3();
		if (Input.GetKey(KeyCode.UpArrow))
		{
			delta.y += moveSpeed * currZoom / startZoom * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.DownArrow))
		{
			delta.y -= moveSpeed * currZoom / startZoom * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.RightArrow))
		{
			delta.x += moveSpeed * currZoom / startZoom * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			delta.x -= moveSpeed * currZoom / startZoom * Time.deltaTime;
		}
		transform.position += delta;

		float zoomDelta = 0;
		if (Input.GetKey(KeyCode.Equals))
		{
			zoomDelta -= zoomSpeed * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.Minus))
		{
			zoomDelta += zoomSpeed * Time.deltaTime;
		}
		GetComponent<Camera>().orthographicSize += zoomDelta;

		if (Input.GetKeyDown(KeyCode.R))
		{
			transform.position = startPos;
			GetComponent<Camera>().orthographicSize = startZoom;
		}
	}
}
