using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LifeLike.Views{
public class ListView : MonoBehaviour {
#region HANDLER ItemLoaded

		public delegate void OnItemLoadedHandler (ListItemBase item);

		public OnItemLoadedHandler onItemLoaded;

		public void ItemLoaded(ListItemBase item, bool clear = false)
		{
			if (onItemLoaded != null)
			{
				onItemLoaded(item);

				if (clear)
				{
					onItemLoaded = null;
				}
			}
		}

		#endregion

		#region HANDLER ItemSelected

		public delegate void OnItemSelectedHandler (ListItemBase item);

		public OnItemSelectedHandler onItemSelected;

		public void ItemSelected(ListItemBase item, bool clear = false)
		{
			if (onItemSelected != null)
			{
				onItemSelected(item);

				if (clear)
				{
					onItemSelected = null;
				}
			}
		}

		#endregion
	
		private enum ScrollDirection
		{
			NEXT,
			PREVIOUS
		}


		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _viewport;

		[SerializeField]
		private RectTransform _content;

		[SerializeField]
		private float _spacing;

		[SerializeField]
		private bool _fitItemToViewport;

		[SerializeField]
		private bool _centerOnItem;

		[SerializeField]
		private float _changeItemDragFactor;


		private List<ListItemBase> _itemsList;

		private float _itemSize;
		private float _lastPosition;

		private int _itemsTotal;
		private int _itemsVisible;

		private int _itemsToRecycleBefore;
		private int _itemsToRecycleAfter;

		private int _currentItemIndex;
		private int _lastItemIndex;

		private Vector2 _dragInitialPosition;


		public void Start(){
			//	_scrollRect=this.GetComponent<ScrollRect>();
		//		_viewport=this.GetComponentsInChildren<RectTransform>().FirstOrDefault(p=>p.name=="Viewport");
		//		_content=this.GetComponentsInChildren<RectTransform>().FirstOrDefault(p=>p.name=="Content");
		}

		public void Create(int items, ListItemBase listItemPrefab)
		{				
				_scrollRect.vertical = true;
				_scrollRect.horizontal = false;

				_content.anchorMin = new Vector2 (0, 1);
				_content.anchorMax = new Vector2 (1, 1);

				if (_fitItemToViewport)
				{
					listItemPrefab.Size = new Vector2(listItemPrefab.Size.x, _viewport.rect.height);
				}

				_itemSize = listItemPrefab.Size.y;

				_content.sizeDelta = new Vector2(0, _itemSize * items + _spacing * (items - 1));
	
			if (_centerOnItem)
			{
				_scrollRect.inertia = false;
			}


			_itemsVisible = Mathf.CeilToInt (GetViewportSize () / _itemSize);

			int itemsToInstantiate = _itemsVisible;

			if (_itemsVisible == 1)
			{
				itemsToInstantiate = 5;
			}
			else if (itemsToInstantiate < items)
			{
				itemsToInstantiate *= 2;
			}

			if(itemsToInstantiate > items)
			{
				itemsToInstantiate = items;
			}

			_itemsList = new List<ListItemBase> ();

			for (int i = 0; i < itemsToInstantiate; i++)
			{
				ListItemBase item = CreateNewItem (listItemPrefab, i, _itemSize);
				item.onSelected = HandleOnSelectedHandler;
				item.Index = i;

				_itemsList.Add(item);

				ItemLoaded (item);
			}
				
			_itemsTotal = items;

			_lastItemIndex = _itemsList.Count - 1;

			_itemsToRecycleAfter = _itemsList.Count - _itemsVisible;


			_scrollRect.onValueChanged.AddListener ((Vector2 position) => 
			{
				if(!_centerOnItem)
				{
					Recycle();
				}
			});	
		}

		private ListItemBase CreateNewItem(ListItemBase prefab, int index, float dimension)
		{
			GameObject instance = (GameObject)Instantiate (prefab.gameObject, Vector3.zero, Quaternion.identity);
			instance.transform.SetParent (_content.transform);
			instance.transform.localScale = Vector3.one;
			instance.SetActive (true);

			float position = index * (dimension + _spacing) + dimension / 2;

			RectTransform rectTransform = instance.GetComponent <RectTransform> ();

	
				rectTransform.anchorMin = new Vector2 (0, 1);
				rectTransform.anchorMax = new Vector2 (1, 1);
				rectTransform.anchoredPosition = new Vector2 (0, -position);
				rectTransform.offsetMin = new Vector2 (0, rectTransform.offsetMin.y);
				rectTransform.offsetMax = new Vector2 (0, rectTransform.offsetMax.y);
	
			return instance.GetComponent <ListItemBase> ();
		}


		void HandleOnSelectedHandler (ListItemBase item)
		{
			ItemSelected (item);
		}


		private void Recycle()
		{
			if(_lastPosition == -1)
			{
				_lastPosition = GetContentPosition ();

				return;
			}

			int displacedRows = Mathf.FloorToInt (Mathf.Abs (GetContentPosition() - _lastPosition) / _itemSize);

			if (displacedRows == 0)
			{
				return;
			}

			ScrollDirection direction = GetScrollDirection ();

			for (int i = 0; i < displacedRows; i++)
			{
				switch (direction)
				{
				case ScrollDirection.NEXT:

					NextItem ();

					break;

				case ScrollDirection.PREVIOUS:

					PreviousItem ();

					break;
				}

				if (direction == ScrollDirection.NEXT)
				{
					_lastPosition += _itemSize + _spacing;
				}
				else
				{
					_lastPosition -= _itemSize + _spacing;
				}
			}
		}

		private void NextItem()
		{
			if (_itemsToRecycleBefore >= (_itemsList.Count - _itemsVisible) / 2 && _lastItemIndex < _itemsTotal - 1)
			{
				_lastItemIndex++;

				RecycleItem (ScrollDirection.NEXT);
			}
			else
			{
				_itemsToRecycleBefore++; 
				_itemsToRecycleAfter--;
			}
		}

		private void PreviousItem()
		{
			if (_itemsToRecycleAfter >= (_itemsList.Count - _itemsVisible) / 2 && _lastItemIndex > _itemsList.Count - 1)
			{
				RecycleItem (ScrollDirection.PREVIOUS);

				_lastItemIndex--;
			}
			else
			{
				_itemsToRecycleBefore--; 
				_itemsToRecycleAfter++;
			}
		}

		private void RecycleItem(ScrollDirection direction)
		{
			ListItemBase firstItem = _itemsList [0];
			ListItemBase lastItem = _itemsList [_itemsList.Count - 1];

			float targetPosition = (_itemSize + _spacing);

			switch(direction)
			{
			case ScrollDirection.NEXT:
					firstItem.Position = new Vector2 (firstItem.Position.x, lastItem.Position.y - targetPosition);
				firstItem.Index = _lastItemIndex;
				firstItem.transform.SetAsLastSibling ();

				_itemsList.RemoveAt (0);
				_itemsList.Add (firstItem);

				ItemLoaded (firstItem);
				break;

			case ScrollDirection.PREVIOUS:


				lastItem.Position = new Vector2 (lastItem.Position.x, firstItem.Position.y + targetPosition);

				lastItem.Index = _lastItemIndex - _itemsList.Count;
				lastItem.transform.SetAsFirstSibling ();

				_itemsList.RemoveAt (_itemsList.Count - 1);
				_itemsList.Insert (0, lastItem);

				ItemLoaded (lastItem);
				break;
			}

			Canvas.ForceUpdateCanvases();
		}


		public void OnDragBegin (BaseEventData eventData)
		{
			if(_centerOnItem)
			{
				_dragInitialPosition = ((PointerEventData)eventData).position;
			}
		}

		public void OnDragEnd (BaseEventData eventData)
		{
			if(_centerOnItem)
			{
				float delta = GetDragDelta (_dragInitialPosition, ((PointerEventData)eventData).position);

				if (_itemsList != null && Mathf.Abs (delta) > _itemSize * _changeItemDragFactor)
				{
					if (Mathf.Sign (delta) == -1 && _currentItemIndex < _itemsTotal - 1)
					{
						NextItem ();

						_currentItemIndex++;
					}
					else if (Mathf.Sign (delta) == 1 && _currentItemIndex > 0)
					{
						_currentItemIndex--;

						PreviousItem ();
					}
				}

				CenterOnItem (_currentItemIndex);
			}
		}

		public void CenterOnItem (int index)
		{
			StartCoroutine (CenterOnItemCoroutine (index));	
		}

		private IEnumerator CenterOnItemCoroutine (int index)
		{
			yield return new WaitForEndOfFrame ();

			if (_itemsList != null && _itemsList.Count > 0)
			{
				float positionX = 0;
				float positionY = 0;

					positionY = -(index * (_itemSize + _spacing));

				_content.anchoredPosition = new Vector2 (positionX, positionY);
			}
			else
			{
				#if UNITY_EDITOR || DEVELOPMENT_BUILD
				Debug.Log("CENTER ON ITEM BUT ITEMS LIST IS NULL");
				#endif
			}
		}


		public void Destroy()
		{
			_scrollRect.verticalNormalizedPosition = 1;

			if(_itemsList != null)
			{
				for (int i = 0; i < _itemsList.Count; i++)
				{
					Destroy (_itemsList [i].gameObject);
				}

				_itemsList.Clear ();
				_itemsList = null;
			}

			_lastPosition = -1;
		}



		#region UTILS

		private float GetContentPosition()
		{
				return _content.anchoredPosition.y;
		}

		private float GetViewportSize()
		{
				return _viewport.rect.height;
		}

		private ScrollDirection GetScrollDirection()
		{
			return _lastPosition > GetContentPosition() ? ScrollDirection.PREVIOUS : ScrollDirection.NEXT;				
		}

		private float GetDragDelta(Vector2 initial, Vector2 current)
		{
				return (current.y - initial.y) * -1;
		}

		#endregion
}
}
