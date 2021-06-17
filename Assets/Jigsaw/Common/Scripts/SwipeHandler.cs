using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SwipeMenu
{
	/// <summary>
	/// Handles swiping and flicking. Includes mouse and mobile support.
	/// </summary>
	public class SwipeHandler : MonoBehaviour
	{
		/// <summary>
		/// If true, swipes will be handled.
		/// </summary>
		public bool handleSwipes = true;

		/// <summary>
		/// Flicks are classed as swipes but with a force greater than SwipeHandler#requiredForceForFlick.
		/// </summary>
		public bool handleFlicks = true;

		/// <summary>
		/// The required force for a swipe to be classes as a flick.
		/// </summary>
		public float requiredForceForFlick = 7f; 
	
		public enum FlickType
		{
			Inertia,
			MoveOne
		}
		/// <summary>
		/// The type of flick. Inertia scrolls kinematically, MoveOne moves the menu in the x direction by one for each flick.
		/// </summary>
		public FlickType flickType = FlickType.Inertia;

		/// <summary>
		/// Once a swipe or flick has finished this will move the menu closest to the centre, to the centre.
		/// </summary>
		public bool lockToClosest = true;

        /// <summary>
        /// Limits the maximum force applied when swiping.
        /// </summary>
        public float maxForce = 15f;

		private Vector3 finalPosition, startpos, endpos, oldpos;
		private float  startTime, mouseMove, force, length;
		private bool SW;

		/// <summary>
		/// Gets a value indicating whether this <see cref="SwipeMenu.SwipeHandler"/> is swiping.
		/// </summary>
		/// <value><c>true</c> if is swiping; otherwise, <c>false</c>.</value>
		public bool isSwiping {
			get {
				return SW || Mathf.Abs(length) > 0.05f;
			}
		}

		void Update ()
		{
            HandleMouseSwipe();
        }

		private void HandleMobileSwipe ()
		{

            if (Input.touchCount > 0) {

				if (Input.GetTouch (0).phase == TouchPhase.Began) {
					startTime = Time.time;
					finalPosition = Vector3.zero;
					length = 0;
					SW = false;
					Vector2 touchDeltaPosition = Input.GetTouch (0).position;
					startpos = new Vector3 (touchDeltaPosition.x, 0, touchDeltaPosition.y);
					oldpos = startpos;
				}   

				if (Input.GetTouch (0).phase == TouchPhase.Moved) {
					SW = true;

					Vector2 touchDeltaPosition = Input.GetTouch (0).position;
					Vector3 pos = new Vector3 (touchDeltaPosition.x, 0, touchDeltaPosition.y);

					if (handleSwipes && pos.x != oldpos.x) {
						var f = pos - oldpos;

						var l = f.x < 0 ? (f.magnitude * Time.deltaTime) : -(f.magnitude * Time.deltaTime);
					
						l *= .2f;

						Menu.Instance.Constant (l);
					}

					oldpos = pos;
				}
			
				if (Input.GetTouch (0).phase == TouchPhase.Canceled) {
					SW = false;
				}
			
				if (Input.GetTouch (0).phase == TouchPhase.Stationary) {
					SW = false;
				}

				if (Input.GetTouch (0).phase == TouchPhase.Ended) {
					if (SW && handleFlicks) {
						Vector2 touchPosition = Input.GetTouch (0).position;
						endpos = new Vector3 (touchPosition.x, 0, touchPosition.y);
						finalPosition = endpos - startpos;
						length = finalPosition.x < 0 ? -(finalPosition.magnitude * Time.deltaTime) : (finalPosition.magnitude * Time.deltaTime);

						length *= .35f;

						var force = length / (Time.time - startTime);

                        force = Mathf.Clamp(force, -maxForce, maxForce);

                        if (handleFlicks && Mathf.Abs (force) > requiredForceForFlick) {
							Menu.Instance.Inertia (-length);
						}  
					}

					if (lockToClosest) {
						Menu.Instance.LockToClosest ();
					}
				}
			}
		}

        private bool CheckTouch(Vector3 screenPoint)
        {
            Ray touchRay = Camera.main.ScreenPointToRay(screenPoint);
            RaycastHit hit;

            Physics.Raycast(touchRay, out hit);
            return (hit.collider != null && hit.collider.gameObject.CompareTag("MenuItem"));
        }

        private void HandleMouseSwipe ()
		{
            if (Input.GetMouseButtonDown (0) && CheckTouch(Input.mousePosition) && !CUtils.IsPointerOverUIObject()) {
				startTime = Time.time;
				finalPosition = Vector3.zero;
				length = 0;

				startpos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -1.83f));
                oldpos = startpos;
                SW = true;
			}

			if (Input.GetMouseButtonUp (0) && SW) {
                SW = false;
				endpos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -1.83f));
                finalPosition = endpos - startpos;
				length = finalPosition.x < 0 ? (-finalPosition.magnitude) : (finalPosition.magnitude);

				force = length / (Time.time - startTime);

                if (handleFlicks && Mathf.Abs (force) > requiredForceForFlick) {

					if (flickType == FlickType.Inertia) {
                        Menu.Instance.Inertia (length);
					} else {
						if (length > 0) {
							Menu.Instance.MoveLeftRightByAmount (1);
						} else {
							Menu.Instance.MoveLeftRightByAmount (-1);
						}
					}
				} else if (lockToClosest && force != 0) {
					Menu.Instance.LockToClosest ();
				}
			}

            if (SW)
            {
                Vector3 pos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -1.83f));

                if (handleSwipes && pos.x != oldpos.x)
                {
                    var f = pos - oldpos;
                    var length = f.x < 0 ? (-f.magnitude ) : (f.magnitude);

                    Menu.Instance.Constant(length);
                }

                oldpos = pos;
            }
        }
	}
}