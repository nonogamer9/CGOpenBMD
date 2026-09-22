using System;
using UnityEngine;

[Serializable]
public class rotate : MonoBehaviour
{
	public int speed;

	public float friction;

	public float lerpSpeed;

	public float xDeg;

	public float yDeg;

	private Quaternion fromRotation;

	private Quaternion toRotation;

	public virtual void Update()
	{
		if (Input.GetMouseButton(0))
		{
			RotateTransform(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
		}
		else
		{
			RotateTransform(0f, 0f);
		}
	}

	public virtual void RotateTransform(float xNum, float yNum)
	{
		xDeg -= xNum * (float)speed * friction;
		yDeg -= yNum * (float)speed * friction;
		fromRotation = transform.rotation;
		toRotation = Quaternion.Euler(yDeg, xDeg, 0f);
		transform.rotation = Quaternion.Lerp(fromRotation, toRotation, Time.deltaTime * lerpSpeed);
	}

	public virtual void Main()
	{
	}
}
