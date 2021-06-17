using UnityEngine;
using System.Collections;

namespace SwipeMenu
{
	/// <summary>
	/// Handles touches seperate from swipes. Supports mouse and mobile touch controls.
	/// If a menu item is selected and isn't centred, then the menu item is animated to centre. If
	/// a menu item is centred than its <see cref="MenuItem.OnClick"/> is invoked.
	/// </summary>
	public class TouchHandler : MonoBehaviour
	{
		/// <summary>
		/// If true, menu selection is handled.
		/// </summary>
		public bool handleTouches = true;

		/// <summary>
		/// The selected menu item has to be centred for selectiion to occur.
		/// </summary>
		public bool requireMenuItemToBeCentredForSelectiion = true;

		private SwipeHandler _swipeHandler;

		void Start ()
		{
			_swipeHandler = GetComponent<SwipeHandler> ();
		}

		void LateUpdate ()
		{
			if (!handleTouches)
				return;

			if (_swipeHandler && _swipeHandler.isSwiping) {
				return;
			}

            if (Input.GetMouseButtonUp (0))
            {
				CheckTouch (Input.mousePosition);
			}
		}

        private void CheckTouch (Vector3 screenPoint)
		{
			Ray touchRay = Camera.main.ScreenPointToRay (screenPoint);
			RaycastHit hit;
			
			Physics.Raycast (touchRay, out hit);
			
			if (hit.collider != null && hit.collider.gameObject.CompareTag ("MenuItem") && !CUtils.IsPointerOverUIObject()) {

				var item = hit.collider.GetComponent<MenuItem> ();

				if (Menu.Instance.MenuCentred (item)) {
					Menu.Instance.ActivateSelectedMenuItem (item);
				} else {
					Menu.Instance.AnimateToTargetItem (item);

					if (!requireMenuItemToBeCentredForSelectiion) {
						Menu.Instance.ActivateSelectedMenuItem (item);
					}
				}
			}
		}

	}
}
