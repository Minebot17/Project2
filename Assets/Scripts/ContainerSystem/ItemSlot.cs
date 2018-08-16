using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour {
	public const float DeltaForTransfer = 5f;
	[SerializeField]
	protected int slotIndex;
	[SerializeField]
	protected Vector2 slotSize;
	protected ItemStack stack;

	/// <summary>
	/// Индекс данного слота в контейнере
	/// </summary>
	public int SlotIndex => slotIndex;

	/// <summary>
	/// Размер хитбокса и рендера слота
	/// </summary>
	public Vector2 SlotSize => slotSize;

	/// <summary>
	/// Предмет в слоте
	/// </summary>
	public ItemStack Stack {
		get { return stack; }
		set { stack = value; }
	}

	/// <summary>
	/// Тут устанавливается рендер указанного предмета, задается в слот стак
	/// </summary>
	/// <param name="stack">Предмет, который будет в слоте</param>
	public void SetStack(ItemStack stack) {
		this.stack = stack;
		bool isBuffered = false;
		if (transform.childCount != 0) {
			if (ContainerManager.CurrentContainer.bufferSlot == transform.GetChild(0).gameObject) {
				isBuffered = true;
			}

			Destroy(transform.GetChild(0).gameObject);
		}
		if (stack == null)
			return;

		ItemInfo itemInfo = ItemManager.FindItemInfo(stack.ItemName);
		GameObject itemObject = new GameObject(itemInfo.ItemName);
		
		itemObject.transform.parent = transform;
		itemObject.transform.localPosition = new Vector3(0, 0, -171f);
		itemObject.transform.localScale = new Vector3(slotSize.x, slotSize.y, 1);

		itemObject.AddComponent<MeshRenderer>().material = new Material(GameManager.singleton.IconMaterial){ mainTexture = itemInfo.Icon };
		itemObject.AddComponent<MeshFilter>().mesh = GameManager.singleton.OnePlaneCenter;
		itemObject.AddComponent<Canvas>();
		if (itemInfo.MaxStackSize != 1) {
			GameObject text = new GameObject("stackSize");
			text.transform.parent = itemObject.transform;
			text.transform.localPosition = new Vector3(0, 0, -2);
			text.AddComponent<RectTransform>().sizeDelta = new Vector2(slotSize.x, slotSize.y);
			text.GetComponent<RectTransform>().localScale = new Vector3(1/slotSize.x, 1/slotSize.y, 1);
			text.AddComponent<Text>().text = stack.StackSize + "";
			text.GetComponent<Text>().alignment = TextAnchor.LowerRight;
			text.GetComponent<Text>().font = Font.CreateDynamicFontFromOSFont("Arial", 20);
		}

		if (isBuffered) {
			ContainerManager.CurrentContainer.bufferSlot = itemObject;
			Vector3 pos = Utils.GetMouseWorldPosition();
			itemObject.transform.position = new Vector3(pos.x, pos.y, 0);
			itemObject.transform.localPosition = new Vector3(itemObject.transform.localPosition.x, itemObject.transform.localPosition.y, -174f);
		}
	}

	// Ниже идет система переноса предметов из слотов
	private Vector3 lastMousePosition;
	private float sumDelta = 0;
	private void OnMouseDrag() {
		if (stack == null|| ContainerManager.CurrentContainer.IsGrabed())
			return;
		
		if (sumDelta > DeltaForTransfer) {
			sumDelta = 0;
			lastMousePosition = Vector3.zero;
			ContainerManager.CurrentContainer.GrabFromSlot(slotIndex);
		}

		if (lastMousePosition != Vector3.zero) {
			sumDelta += Vector3.Distance(Utils.GetMouseWorldPosition(), lastMousePosition);
		}

		lastMousePosition = Utils.GetMouseWorldPosition();
	}

	private void OnMouseExit() {
		sumDelta = 0;
		lastMousePosition = Vector3.zero;
	}

	private void OnMouseUp() {
		sumDelta = 0;
		lastMousePosition = Vector3.zero;
	}
}
