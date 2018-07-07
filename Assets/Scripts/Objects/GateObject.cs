using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpawnedData))]
public class GateObject : MonoBehaviour {
	[SerializeField]
	private GameObject body;
	[SerializeField]
	private GameObject positiveCorner;
	[SerializeField]
	private GameObject negativeCorner;
	[SerializeField]
	private GameObject positiveBorder;
	[SerializeField]
	private GameObject negativeBorder;
	[SerializeField]
	private GameObject positiveShadow;
	[SerializeField]
	private GameObject negativeShadow;
	[SerializeField]
	private GameObject forwardShadow;
	[SerializeField]
	private GameObject positiveCheckPoint;
	[SerializeField]
	private GameObject negativeCheckPoint;

	private bool enable = true;
	public int gateType;

	public void Start() {
		string[] data = GetComponent<SpawnedData>().spawnedData;
		gateType = int.Parse(data[0]);
		if (gateType == 2)
			enable = false;

		if (enable) {
			GetComponent<BoxCollider2D>().enabled = false;
			forwardShadow.SetActive(false);
		}
		else {
			for (int i = 0; i < transform.childCount; i++)
				transform.GetChild(i).gameObject.SetActive(false);

			forwardShadow.SetActive(true);
			GetComponent<PolygonCollider2D>().enabled = false;
			GetComponent<BoxCollider2D>().enabled = true;
			return;
		}

		PolygonCollider2D collider = transform.parent.parent.gameObject.GetComponent<PolygonCollider2D>();
		bool isPositiveBorder = collider.OverlapPoint(positiveCheckPoint.transform.position);
		bool isNegativeBorder = collider.OverlapPoint(negativeCheckPoint.transform.position);

		positiveBorder.SetActive(isPositiveBorder);
		positiveCorner.SetActive(!isPositiveBorder);
		negativeBorder.SetActive(isNegativeBorder);
		negativeCorner.SetActive(!isNegativeBorder);
		// TODO добавить убирание объектов из даты
	}

	public bool Enable {
		set {
			enable = value;
			Start();
		}
		get { return enable; }
	}
}
