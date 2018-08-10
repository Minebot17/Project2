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
		if (transform.childCount != 1) {
			Destroy(transform.GetChild(1).gameObject);
			transform.GetChild(0).GetComponent<Text>().text = "";
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

		if (itemInfo.MaxStackSize != 1)
			transform.GetChild(0).GetComponent<Text>().text = stack.StackSize+"";
	}

	// Ниже идет система переноса предметов из слотов
	private bool toTransfer;
	private Vector3 lastMousePosition;
	private float sumDelta = 0;
	private void OnMouseDrag() {
		if (stack == null || !toTransfer || ContainerManager.CurrentContainer.IsGrabed())
			return;
		
		if (sumDelta > DeltaForTransfer) {
			sumDelta = 0;
			lastMousePosition = Vector3.zero;
			toTransfer = false;
			ContainerManager.CurrentContainer.GrabFromSlot(slotIndex);
		}

		if (lastMousePosition != Vector3.zero) {
			sumDelta += Vector3.Distance(Utils.GetMouseWorldPosition(), lastMousePosition);
		}

		lastMousePosition = Utils.GetMouseWorldPosition();
	}

	private void OnMouseDown() {
		sumDelta = 0;
		lastMousePosition = Vector3.zero;
		toTransfer = true;
	}

	private void OnMouseExit() {
		sumDelta = 0;
		lastMousePosition = Vector3.zero;
		toTransfer = false;
	}

	private void OnMouseUp() {
		sumDelta = 0;
		lastMousePosition = Vector3.zero;
		toTransfer = false;
	}
}
