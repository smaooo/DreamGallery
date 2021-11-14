using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


namespace StarterAssets
{
	[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
	[RequireComponent(typeof(PlayerInput))]
#endif
	public class FirstPersonController : MonoBehaviour
	{
		[Header("Player")]
		[Tooltip("Move speed of the character in m/s")]
		public float MoveSpeed = 4.0f;
		[Tooltip("Rotation speed of the character")]
		public float RotationSpeed = 1.0f;


		[Header("Cinemachine")]
		[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
		public GameObject CinemachineCameraTarget;
		[Tooltip("How far in degrees can you move the camera up")]
		public float TopClamp = 90.0f;
		[Tooltip("How far in degrees can you move the camera down")]
		public float BottomClamp = -90.0f;

		// cinemachine
		private float _cinemachineTargetPitch;

		// player
		private float _speed;
		private float _rotationVelocity;

		private float distance;

		private CharacterController _controller;
		private StarterAssetsInputs _input;
		private GameObject _mainCamera;

		private const float _threshold = 0.01f;

		// Collision
		public Collider otherCollider;

		// Player State Machine
		public enum States { Sitting, Standing, Walking, Interacting, Transiting};
		public States playerState;
		// Trigger State Machine
		private enum TriggerStates { SittingArea, Outside, InteractionArea, DoorArea, BoatArea};
		private TriggerStates triggerStates = TriggerStates.Outside;

		public GameObject wharfCollision;
		// HUD
		[Header("HUD")]
		public GameObject uiCanvas; // the HUD ui canvas
		public GameObject interactionMessageHolder; // messages for interaction keys (E)
		public GameObject focusMessageHolder; // messages for focus keys (Q)
		public GameObject warningMessageHolder; // messages for giving the state of focus charge
		public GameObject puzzleInteractionMessageHolder; // mesaages for interacting with puzzle (LMB)
		public GameObject puzzleSelectedMessageHolder; // messages for interacting with second tile (LMB + RMB)
		public GameObject errorMessage; // messages for giving errors to player
		public GameObject doorLockedMessageHolder; // message for telling the user door is locked
		public GameObject puzzleSolvedMessageHolder; // message for telling user puzzle is solved
		public GameObject EndingSeaTextHolder; // message for showing the ending text
		public GameObject UIHelpMessageHolder; // message for showing the keymap
		public GameObject pauseMenu; // puase menu ui
		public Animator crossFade; // cross fade animation 
		public Animator credits; // ending credits ui
		private bool gamePaused = false; // to determine if pause menu is active or not
		private List<GameObject> deactivatedUIs = new List<GameObject>(); // to keep record of deactivated uis for showing pause menu
		private bool creditsRolling = false;

		// Player position before sitting / interacting
		Vector3 prevPosition = new Vector3(-1.0168767f, 0.160002694f, 4.77066183f);

		private bool playerFocused = false; // to determine if player has focused in the beginning
		// Raycast Paintings
		private float focusTime = 5; // Focus Allowed Time
		private float focusTimer; // Focus Timer
		private float focusBufferTimer = 0f; // Buffer Timer
		private RaycastHit paintingHit;
		private int paintingsLayerMask = 1 << 7; // Layer Mask for paintings
		private bool hitPainting = false;

		// Raycast Puzzle Tiles
		private bool hitTiles = false; 
		private RaycastHit puzzleTileHit;
		private int puzzleTileLayerMask = 1 << 8; // Layer Mask for puzzle tiles
		private Transform latestTileHit = null;
		
		
		[Header("Puzzle")]
		// Puzzle Table Game Object
		public GameObject puzzleTable;
		public BarPuzzle barPuzzle;

		// Handeling tile swap
		private GameObject selectedPuzzleTile; // current holding tile
		private Transform tilePrevParent; // previous parent in case of cancelation
		private Vector3 tilePrevPosition; // previous position of the holding tile
		private Quaternion tilePrevRotation; // previous rotatuib of the holding tile
		private Quaternion swapTileRotation; // second tile rotation

		[Header("Post-processing Profile")]
		// Post-Processing Variables
		public PostProcessingManager postProcessingManager;
		public GameObject mysteryBox;
		public GameObject paintingsFilter;

		[Header("Bar Door")]
		public GameObject barDoor;
		private Quaternion doorOriginalRotation;
		// Post-Processing States
		private enum FocusMode {Focus, OutOfFocus};
		private FocusMode fMode = FocusMode.OutOfFocus;

		// Sounds
		[Header("Sounds")]
		public WalkingSound walkingSound;
		public TilesAudio tilesAudio;
		public DoorSounds doorSounds;
		public GameObject barAudioSound;
		public SeaSound seaSound;
		public AudioClip mainMenuSound;

		Vector2 mysEnter = Vector2.zero;
		private bool isInsideFirst = true;
		private bool isInsideSecond = false;
		private void Awake()
		{

			// get a reference to our main camera
			if (_mainCamera == null)
			{
				_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
			}
			// Set post processing for the begining of the scene

			
		}

		private void Start()
		{
			// Lock cursor to center of screen
			Cursor.lockState = CursorLockMode.Locked;
			// Set trigger state to sitting area
			triggerStates = TriggerStates.SittingArea;
			// Show message for pressing Q for focus
			focusMessageHolder.SetActive(true);
			// Unload main menu sound from memory
			mainMenuSound.UnloadAudioData();

			// Set character contorller
			_controller = GetComponent<CharacterController>();
			// Set input manager
			_input = GetComponent<StarterAssetsInputs>();

			// Setup Focus Timer
			focusTimer = focusTime;

			//Sit Player on chair for the start of game
			// Get the position of the selected chair from its relation to its child collision
			Transform parentObject = otherCollider.transform.parent;
			// Disable the chair collision
			parentObject.GetComponent<BoxCollider>().enabled = false;
			// Target position for player to sit and attach
			transform.position = new Vector3(parentObject.transform.position.x, transform.position.y, parentObject.transform.position.z);
			// Play Sitting Sound
			parentObject.GetChild(5).GetComponent<SittingSound>().PlaySound("sit");

			crossFade.SetTrigger("FadeIn");

		}

		private void Update()
		{

			// Interaction Button
			// Check if player is sitting
			if (playerState == States.Sitting)
			{
				// If E is pressed and he is sitting then stand up
				if (Input.GetKeyDown(KeyCode.E) && playerFocused == true)
                {

					// Remove the message from UI
					interactionMessageHolder.SetActive(false);
					// Call stand up function		
					Stand();
				}
				else if (Input.GetKeyDown(KeyCode.Q) && playerFocused == false)
                {
					// Change first layer post-processing
					postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOutFocusParameters);
					// Check if player has completely focused
					StartCoroutine(PlayerFocused());
					// Remove UI message form screen
					focusMessageHolder.SetActive(false);

				}

			}
			if (!playerFocused)
            {
				_input.look = Vector2.zero;
            }
			// Check if Player is Walking
			else if (playerState == States.Walking)
			{
				Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
				// Check if player is looking at a painting
				hitPainting = Physics.Raycast(ray, out paintingHit, 5f, paintingsLayerMask);
				// If player is in a chair's trigger box and E is pressed
				if (Input.GetKeyDown(KeyCode.E) && triggerStates == TriggerStates.SittingArea)
				{
					// Remove the message from UI
					interactionMessageHolder.SetActive(false);
					// Call sit function
					Sit();
				}

				// if player is in the Interaction area and E is pressed
				else if (Input.GetKeyDown(KeyCode.E) && triggerStates == TriggerStates.InteractionArea && barPuzzle.solved == false)
                {
					// Remove the message form UI
					interactionMessageHolder.SetActive(false);
					
					// Call the Interact function
					GoTOInteractionState();
                }

				// If player is in the Door area and E is pressed
				else if (Input.GetKeyDown(KeyCode.E) && triggerStates == TriggerStates.DoorArea)
                {
					// If puzzle is solved then door can be unlocked
					if (barPuzzle.solved == true)
                    {
						// Play Sound
						doorSounds.PlaySound("open");
						doorOriginalRotation = barDoor.transform.rotation;
						// Open Door
						StartCoroutine(RotateDoor("open"));
						// Fade out bar ambient music
						StartCoroutine(FadeOutBarSound());
                    }
					else
                    {
						// Message player that door is locked
						doorLockedMessageHolder.SetActive(true);
						// Hide "door is locked" message after 2 seconds
						StartCoroutine(HideDoorMessage(2f, "door"));

                    }
                }
				// If player is looking at paintings, prompt in ui that he can focus on painting
				else if (hitPainting && triggerStates == TriggerStates.Outside &&
					!paintingHit.collider.CompareTag("ExitSign") && gamePaused == false &&
					triggerStates != TriggerStates.BoatArea)
                {
					// Show press Q message in UI
					focusMessageHolder.SetActive(true);
					// Move character
					Move();
                }
				
				// If player is in no trigger box keep character in the walking state
				else
                {
					// Remove focus message from UI
					focusMessageHolder.SetActive(false);
					// Call the Move function
					Move();
				}
			}

			// If player in the interaction mode
			else if (playerState == States.Interacting)
            {
				// If E is pressed to exit interaction mode
				if (Input.GetKeyDown(KeyCode.E))
                {
					// Check if player is holding a puzzle tile
					if (selectedPuzzleTile != null)
                    {
						// Cancel the pick up
						PlaceTile("reverse");
						// Remove UI message
						puzzleInteractionMessageHolder.SetActive(false);
                    }
					// If player was looking at a tile and at the same moment has requested exit of the interaction area
					if (latestTileHit != null)
                    {
						// Turn tiles glow off
						latestTileHit.GetComponent<MeshRenderer>().material.SetFloat("GlowPower", 0);
						// Set latestTileHit to null
						latestTileHit = null;
					}
					// Remove ui message if it's active
					puzzleInteractionMessageHolder.SetActive(false);
					// Call the Stand function
					Stand();
                }
				// If player has solved the puzzle
				else if (barPuzzle.solved)
                {
					for (int i = 0; i < mysteryBox.transform.childCount; i++)
					{
						// Change mystery box shader to outside mode
						mysteryBox.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetInt("Inside", 0);
					}
					// Play door unlock sound
					doorSounds.PlaySound("unlock");
					// Show UI message that puzzle is solved
					puzzleSolvedMessageHolder.SetActive(true);
					// Remove interaction messages fro UI
					puzzleInteractionMessageHolder.SetActive(false);
					interactionMessageHolder.SetActive(false);

					// Load needed audios into memory
					seaSound.LoadAudio();
					doorSounds.doorOpen.LoadAudioData();
					doorSounds.doorClose.LoadAudioData();

					// Change player state to transiting
					playerState = States.Transiting;
					// Hide message after 2 seconds
					StartCoroutine(HideDoorMessage(2f, "puzzle"));
                }
				// If player isn't requesting to exit the interaction state
				else
                {
					doorSounds.doorUnlock.LoadAudioData();
					// Call Interact function to allow player solve the puzzle
					Interact();
                }
            }

			if (playerState == States.Walking)
            {
				
				if (focusBufferTimer > 0)
                {
					// Change the message in UI to the focus is charging
					warningMessageHolder.transform.GetChild(0).GetComponent<Text>().text = "Focus is Charging!";
					// Set the warning message visible in the UI
					warningMessageHolder.SetActive(true);
					// Decrement the charging timer
					focusBufferTimer -= Time.deltaTime;
					// Check if focus is charged
					if (focusBufferTimer < 1f)
                    {
						// If so, message player that focus is charged
						warningMessageHolder.transform.GetChild(0).GetComponent<Text>().text = "Focus is Charged!!";
						// Reset focus timer
						focusTimer = focusTime;
					}	
				}
			}

			// Focus Button
			// If player is walking and Q is pressed
			if (Input.GetKey(KeyCode.Q) && playerState == States.Walking)
            {
				// Check if focus is not charging
				if (focusBufferTimer < 0)
                {
					// Make Warning message visible in the UI
					warningMessageHolder.SetActive(true);
					// Call the Focusing function
					FocusOnPaintings();
                }

				// If focus is charging
				else
                {
					// If player is focusing
					if (fMode == FocusMode.Focus)
                    {
						// Set post-processing profile to out of focus
						fMode = FocusMode.OutOfFocus;
						postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOutFocusParameters);
                    }
					// Start the focus charging timer
					focusBufferTimer -= Time.deltaTime;
					// Change the message in UI to the focus is charging
					warningMessageHolder.transform.GetChild(0).GetComponent<Text>().text = "Focus is Charging!";
					// Set the warning message visible in the UI
					warningMessageHolder.SetActive(true);
					// Increment focus timer to its maximum value
				}
            }
			// If player has released the focus button and focus is not charging
			else if (!Input.GetKey(KeyCode.Q) && focusTimer < focusTime && focusBufferTimer < 0.1f)
            {
				// If player was in the focus mode
				if (fMode == FocusMode.Focus)
				{
					// Set the post-process profile to out of focus
					fMode = FocusMode.OutOfFocus;
					postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOutFocusParameters);
				}
				// Increment the focus timer to its original time
				focusTimer += Time.deltaTime;
				// Hide the warning message from the UI
				warningMessageHolder.SetActive(false);
				
            }
			// If player is in the puzzle interaction mode
			if (playerState == States.Interacting)
            {
				// If player has pressed the Left Mouse Button and is currently looking at a puzzle tile and there is picked-up tile
				if (Input.GetKeyDown(KeyCode.Mouse0) && hitTiles && selectedPuzzleTile == null && barPuzzle.IsLocked(puzzleTileHit.transform.tag) == false && gamePaused == false)
				{
					// Assign selected puzzle tile
					selectedPuzzleTile = latestTileHit.gameObject;
					// Call the pick up function
					PickUpTile();
				}

				// If player is holding a tile but isn't looking at another one for swap and has pressed left mouse button 
				else if (Input.GetKeyDown(KeyCode.Mouse0) && selectedPuzzleTile != null && hitTiles == false && gamePaused == false)
                {
					// Show error message
					StartCoroutine(ShowPuzzleErrorMessage());
                }

				// If player has pressed the Left Mouse Button and is currently holding a puzzle tile
				else if (Input.GetKeyDown(KeyCode.Mouse0) && selectedPuzzleTile != null && barPuzzle.IsLocked(puzzleTileHit.transform.tag) == false && gamePaused == false)
                {
					// If player is looking at a second tile for swapping
					if (hitTiles)
                    {
						// Swap the tiles
						PlaceTile("swap");
                    }
                }

				// If player has pressed Right Mouse Button to cancle his pick up action and is currently holding a tile
				else if (Input.GetKeyDown(KeyCode.Mouse1) && selectedPuzzleTile != null && gamePaused == false)
                {
					// Place the tile in its original place
					PlaceTile("reverse");
                }


			
			}
			if (Input.GetKeyDown(KeyCode.P) && creditsRolling == false && playerFocused == true)
            {
				if (pauseMenu.activeSelf)
                {
					Resume();

				}
				else
                {
					// Set look vector to zero 
					_input.look = Vector2.zero;
					// Set game pause true
					gamePaused = true;
					// Remove every visible ui element on screen
					for (int i = 0; i < uiCanvas.transform.childCount - 2; i++)
                    {
						if (uiCanvas.transform.GetChild(i).gameObject.activeSelf == true)
                        {
							uiCanvas.transform.GetChild(i).gameObject.SetActive(false);
							// remember the hidden objects
							deactivatedUIs.Add(uiCanvas.transform.GetChild(i).gameObject);

						}
                    }
				
					_input.cursorInputForLook = false;
					// Show the pause menu
					pauseMenu.SetActive(true);

					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				}
            }

			// If player is inside the outer edge of the mystery box
			if ((transform.position.x > -4.94f && transform.position.x < 0.84f) && (transform.position.z < -5.64f && transform.position.z > -9.69f))
			{
				// If player has just entered the first edge
				if (isInsideFirst)
				{
					isInsideFirst = false;
					// Set the enterance point
					mysEnter = new Vector2(transform.position.x, transform.position.z);

				}
				// If player is beyond second edge of mystery box borde
				if ((transform.position.x > -4.37f && transform.position.x < 0.25f) && (transform.position.z > -9.69 && transform.position.z < -6.24))
				{
					if (isInsideSecond)
                    {
						isInsideSecond = false;
						// Set player's enterance point from second edge
						mysEnter = new Vector2(transform.position.x, transform.position.z);
					}
					// Set shader float to 1 (Inside mystery box)
					for (int i = 0; i < mysteryBox.transform.childCount; i++)
					{
						mysteryBox.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("Inside_Outside",
								1);
					}
				}

				else
                {
					// Change shader float based on the player's location
					for (int i = 0; i < mysteryBox.transform.childCount; i++)
					{
						float value = Vector2.Distance(mysEnter.normalized, new Vector2(transform.position.x, transform.position.z).normalized);
						mysteryBox.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("Inside_Outside",
								value);
					}
				}

			}
			else
            {
				isInsideFirst = true;
				isInsideSecond = true;
				// Set Shader float to 0 (outside mystery box)
				for (int i = 0; i < mysteryBox.transform.childCount; i++)
				{
					mysteryBox.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetFloat("Inside_Outside",
							0);
				}
			} 
		}


		// Focus on paintings if player is looking at a painting and is staying in a specific range from it
		private void FocusOnPaintings()
        {
			// If focus is not charging
			if (focusTimer > 0)
            {
				
				// Decrement the focus timer to zero
				focusTimer -= Time.deltaTime;
				// Set the warning message to show the remaining time of focus
				warningMessageHolder.transform.GetChild(0).GetComponent<Text>().text = "Focus Timer: " + ((int)focusTimer).ToString() + "s";

				// Create Ray from center of the screen for casting 
				Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
				// Check if player is looking at a painting
				hitPainting = Physics.Raycast(ray, out paintingHit, 5f, paintingsLayerMask);

				// If player is looking at a painting and it is out of focus
				if (hitPainting && fMode == FocusMode.OutOfFocus)
                {
					// Call the chaning profile function to set profile to focus mode
					fMode = FocusMode.Focus;
					postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOnFocusParameters);
				}
				// If player is not looking at a painting and it was focusing previously
				else if (!hitPainting && fMode == FocusMode.Focus)
                {
					// Call the chaning profile function to set profile to out of focus mode
					fMode = FocusMode.OutOfFocus;
					postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOutFocusParameters);
				}
            }
			// If focus is charging
			else
            {
				// Call the chaning profile function to set profile to out of focus mode
				fMode = FocusMode.OutOfFocus;
				postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOutFocusParameters);
				// Hide warning message showing focus timer in the UI
				warningMessageHolder.SetActive(false);
				// Set the charging timer
				focusBufferTimer = 10f;
            }
        }

		private void OnTriggerEnter(Collider other)
		{
			// Check if player has entered in a chair trigger and he is not sitting currently
			if (other.CompareTag("sittable") && playerState != States.Sitting)
            {
				// Set the interaction message in the UI visible
				interactionMessageHolder.SetActive(true);
				// Set the Interaction message to show "to sit" message
				interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Sit!";
				// Change player trigger state to sitting area state
				triggerStates = TriggerStates.SittingArea;
				// Remember the other collider for attaching to the chair
				otherCollider = other;
			}
			// Check if player has entered the Bar Puzzle trigger box
			else if (other.CompareTag("BarPuzzle") && playerState == States.Walking && barPuzzle.solved == false)
            {
				// Set the interaction message in the UI to visible
				interactionMessageHolder.SetActive(true);
				// Change the interaction message in the UI to show "to Interact!" message
				interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Interact!";
				// Change player trigger state to interaction area state
				triggerStates = TriggerStates.InteractionArea;
				// Remember the other collider for attaching to the chair
				otherCollider = other;
			}
			// If player has entered puzzle area
			else if (other.CompareTag("PuzzleAreaTrigger"))
            {
				// Change post-processing profiles
				postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.FirstLayer, postProcessingManager.puzzleFirstLayerParameters);
				postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.puzzleSecondLayerParameters);
				// Disable camrea overlay for paintings
				paintingsFilter.SetActive(false);
			
            }
			else if (other.CompareTag("Door"))
            {
				// Set the interaction message in the UI to visible
				interactionMessageHolder.SetActive(true);
				// Change the interaction message in the UI to show "to Interact!" message
				interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Open!";
				// Change trigger state to door area
				triggerStates = TriggerStates.DoorArea;
			}
			else if (other.CompareTag("Wharf"))
            {
				crossFade.SetTrigger("reversefade");
				// Play door sound
				doorSounds.PlaySound("close");
				// Unload audio clip from memory
				doorSounds.RemoveClip();
				// Close door after exiting the bar
				StartCoroutine(RotateDoor("close"));
				// Disable Paintings filiter
				paintingsFilter.SetActive(false);			
				// Change trigger state to boat area
				triggerStates = TriggerStates.BoatArea;
				// Activate Wharf collision to not allow player go back to the bar
				wharfCollision.SetActive(true);
				
            }
		}
        private void OnTriggerStay(Collider other)
        {
			// If player is still in the sitting position (Mostly for a situtation when the game begins in with the sitting mode)
            if (other.CompareTag("sittable") && playerState == States.Sitting && gamePaused == false)
            {
				// Change the trigger state to sitting area mode
				triggerStates = TriggerStates.SittingArea;
				if (playerFocused == true)
                {
					// Set interaction message in the UI to visible
					interactionMessageHolder.SetActive(true);
					// Change the interaction message in the UI to "to Stand Up!"
					interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Stand Up!";
				}
				
			}
			else if (other.CompareTag("sittable") && playerState == States.Walking && gamePaused == false)
            {
				// Change the trigger state to sitting area mode
				triggerStates = TriggerStates.SittingArea;
				if (playerFocused == true)
				{
					// Set interaction message in the UI to visible
					interactionMessageHolder.SetActive(true);
					// Change the interaction message in the UI to "to Stand Up!"
					interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Sit!";
				}
			}
			// If player is in the interaction mode with puzzle table
			else if (other.CompareTag("BarPuzzle") && gamePaused == false)
            {
				if (playerState == States.Interacting && barPuzzle.solved == false)
                {
					// Set interaction message in UI to visible\
					interactionMessageHolder.SetActive(true);
					// Change the interaction message in the UI to "leave the Puzzle!"
					interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Leave!";
					
				}
				else if (playerState == States.Walking && barPuzzle.solved == false && triggerStates != TriggerStates.BoatArea && gamePaused == false)
                {
					// Set interaction message in UI to visible\
					interactionMessageHolder.SetActive(true);
					// Change the interaction message in the UI to "leave the Puzzle!"
					interactionMessageHolder.transform.GetChild(1).GetComponent<Text>().text = "to Interact!";

				}
				
				// Set triggerStates to InteractionArea
				triggerStates = TriggerStates.InteractionArea;
			}
			else if (other.CompareTag("Wharf"))
            {
				// Change trigger state to boat area
				triggerStates = TriggerStates.BoatArea;

			}
		}
        private void OnTriggerExit(Collider other)
        {
			// If player has exited the trigger area, remove interaction message from the UI
			if (interactionMessageHolder.activeInHierarchy)
            {
				interactionMessageHolder.SetActive(false);
            }
			// Change the trigger state to outside mode
			triggerStates = TriggerStates.Outside;

			// If player has exited the puzzle area
			if (other.CompareTag("PuzzleAreaTrigger"))
            {
				// Change post processing profiles
				postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.FirstLayer, postProcessingManager.generalBWParameters);
				postProcessingManager.InitiateLerping(PostProcessingManager.TargetProfile.SecondLayer, postProcessingManager.paintingOutFocusParameters);
				// Activate paintings filter
				paintingsFilter.SetActive(true);
			}
		}

		// Resume the game from pause menu
		public void Resume()
        {
			// Set game pause to false
			gamePaused = false;
			_input.cursorInputForLook = true;
			// Hide pause menu
			pauseMenu.SetActive(false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
			// make hidden ui elements visible again
			for (int i = 0; i < deactivatedUIs.Count; i++)
			{
				deactivatedUIs[i].SetActive(true);
			}
			// empty hidden ui elements
			deactivatedUIs.Clear();
		}

		public void ShowControls()
        {
			Resume();
			StartCoroutine(ShowKeysUI());

		}
		private void LateUpdate()
		{
			CameraRotation();
		}


		private void CameraRotation()
		{
			// if there is an input
			if (_input.look.sqrMagnitude >= _threshold)
			{
				_cinemachineTargetPitch += _input.look.y * RotationSpeed * Time.deltaTime;
				_rotationVelocity = _input.look.x * RotationSpeed * Time.deltaTime;

				// clamp our pitch rotation
				_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

				// Update Cinemachine camera target pitch
				CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0.0f, 0.0f);

				// rotate the player left and right
				transform.Rotate(Vector3.up * _rotationVelocity);
			}
		}

		private void Move()
		{
			// make sure character is on the ground while walking
			transform.position = new Vector3(transform.position.x, 0.1600027f, transform.position.z);
			// Get movement input
			_input.recognizeMove = true;
			// set target speed based on move speed
			float targetSpeed = MoveSpeed;

			// if there is no input, set the target speed to 0
			if (_input.move == Vector2.zero) targetSpeed = 0.0f;

			// a reference to the players current horizontal velocity
			float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;

			float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;
			_speed = targetSpeed;

			// normalise input direction
			Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y).normalized;

			// if there is a move input rotate player when the player is moving	
			if (_input.move != Vector2.zero)
			{
				// move
				inputDirection = transform.right * _input.move.x + transform.forward * _input.move.y;
			}

			// move the player
			_controller.Move(inputDirection.normalized * (_speed * Time.deltaTime));
			distance += (inputDirection.normalized * (_speed * Time.deltaTime)).z;
			if(_speed == 1.5f && walkingSound.sourceIsPlaying == false)
            {
				walkingSound.PlaySound();
            }
		}

		// Player's sitting aciton
		private void Sit()
		{
			// Set players state to transiting
			playerState = States.Transiting;
			// Disable movement
			_input.recognizeMove = false;
			// Get the position of the selected chair from its relation to its child collision
			Transform parentObject = otherCollider.transform.parent;
			// Save player's current position for further use in Stand function
			prevPosition = new Vector3(transform.position.x, 0.1600027f, transform.position.z);
			// Disable the chair collision
			parentObject.GetComponent<BoxCollider>().enabled = false;
			// Play Sitting Sound
			parentObject.GetChild(5).GetComponent<SittingSound>().PlaySound("sit");
			// Move player toward the target position
			StartCoroutine(MoveTowardChair(parentObject));
			interactionMessageHolder.SetActive(false);
			
		}
		
		// Player's standing action (exiting from sitting, interaction, etc. states)
		private void Stand()
		{
			// Set player state to transiting
			playerState = States.Transiting;
			// If player was sitting
			if (triggerStates == TriggerStates.SittingArea)
            {
				Transform parentObject = otherCollider.transform.parent;
				StartCoroutine(MoveAwayFromChair(parentObject));
			}
			// If player is in interaction mode
			else
            {
				
				StartCoroutine(MoveAwayFromChair(null));
				

			}
			// Change player state to walking
		}

		// Transition function to bring player into interaction state
		private void GoTOInteractionState()
		{
			// set player state to transiting
			playerState = States.Transiting;
			// Save player's current position for later use in state change
			prevPosition = new Vector3(transform.position.x, 0.1600027f, transform.position.z);
			// Target position for player
			Vector3 interactionPos = new Vector3(puzzleTable.transform.position.x, transform.position.y, puzzleTable.transform.position.z + puzzleTable.transform.forward.z / 2);
			// Go to interaction position
			StartCoroutine(GoToInteractionStanding(interactionPos));
		}

		// Player's interaction with puzzle in the bar
		private void Interact()
        {

			Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width/2,Screen.height/2,0));
			// Raycast player look and puzzle tiles
			hitTiles = Physics.Raycast(ray, out puzzleTileHit, 5f,puzzleTileLayerMask);
			// If player is looking at a puzzle tile
			if (hitTiles && barPuzzle.IsLocked(puzzleTileHit.transform.tag) == false && gamePaused == false)
            {
				// Save the current collided tile 
				latestTileHit = puzzleTileHit.transform;
				// Turn Tiles Glow on
				latestTileHit.GetComponent<MeshRenderer>().material.SetFloat("GlowPower", 20);
				if (selectedPuzzleTile == null)
                {
					// Set puzzle interaction message in UI to visible
					puzzleInteractionMessageHolder.SetActive(true);
				}
				
			}
			// If player is not looking at any of the tiles and there isn't any tiles assigned to latestTileHit
			else if (hitTiles == false && latestTileHit != null)
            {
				if (latestTileHit.GetComponent<MeshRenderer>() != null)
                {
					// Turn Tile glow off
					latestTileHit.GetComponent<MeshRenderer>().material.SetFloat("GlowPower", 0);
				}
				// Set the latest hit object to null
				latestTileHit = null;
				// Set puzzle interaction message in UI to invisible
				puzzleInteractionMessageHolder.SetActive(false);
			}
		}

		// Pick up the selected puzzle tile for swaping with another one
		private void PickUpTile()
        {
			// Save tile previous parent
			tilePrevParent = selectedPuzzleTile.transform.parent;
			tilePrevPosition = selectedPuzzleTile.transform.position;
			tilePrevRotation = selectedPuzzleTile.transform.rotation;
			
			// Change tile parent 
			selectedPuzzleTile.transform.parent = transform.GetChild(0).GetChild(0);
			// Set tile local position to zero
			selectedPuzzleTile.transform.localPosition = new Vector3(0, 0, 0);
			puzzleInteractionMessageHolder.SetActive(false);
			puzzleSelectedMessageHolder.SetActive(true);
			tilesAudio.PlaySound("pick");
        }

		// Place tile based on the given state
		private void PlaceTile(string state)
        {
			puzzleSelectedMessageHolder.SetActive(false);

			switch (state)
            {
				// Send back the holding tile to its original place
				case "reverse":
					selectedPuzzleTile.transform.parent = tilePrevParent;
					selectedPuzzleTile.transform.position = tilePrevPosition;
					selectedPuzzleTile.transform.rotation = tilePrevRotation;
					selectedPuzzleTile = null;
					break;
				// Swap the holding tile with the selected one
				case "swap":
					// Swap tiles in the puzzle tiles array in the BarPuzzle class.
					barPuzzle.SwapTilesInArray(selectedPuzzleTile, latestTileHit.gameObject);

					// Saving second tile rotation in temp variables
					swapTileRotation = latestTileHit.rotation;

					// Place holding tile in its new place
					selectedPuzzleTile.transform.parent = latestTileHit.parent;
					selectedPuzzleTile.transform.position = latestTileHit.position;
					selectedPuzzleTile.transform.rotation = tilePrevRotation;

					// Place the second tile to its new place 
					latestTileHit.parent = tilePrevParent;
					latestTileHit.position = tilePrevPosition;
					latestTileHit.rotation = swapTileRotation;
					// Turn Tiles glow off
					latestTileHit.GetComponent<MeshRenderer>().material.SetFloat("GlowPower", 0);
					selectedPuzzleTile.GetComponent<MeshRenderer>().material.SetFloat("GlowPower", 0);

					// Set selected puzzle tile to null
					selectedPuzzleTile = null;
					break;
				default:
					break;
            }
			tilesAudio.PlaySound("put");

		}

		private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}


		private IEnumerator MoveTowardChair(Transform target)
		{
			// Remove ui message
			interactionMessageHolder.SetActive(false);
			// Set player's target position
			Vector3 targetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
			while (transform.position != targetPosition)
            {
				// Update Frame
				yield return new WaitForEndOfFrame();
				// Update timer
				
				// Move player toward chair
				transform.position = Vector3.MoveTowards(transform.position, targetPosition, 0.5f * Time.deltaTime);
				// If player is on target position
				if (transform.position == targetPosition)
                {
					// Change player's state to sitting
					playerState = States.Sitting;
					break;
                }
			}
		}	

		private IEnumerator GoToInteractionStanding(Vector3 position)
        {
			// While player is not in the position
			while (transform.position != position)
            {
				// Update frame
				yield return new WaitForEndOfFrame();
				// Move player to the position
				transform.position = Vector3.MoveTowards(transform.position, position, 0.5f * Time.deltaTime);
				// If player is in the position
				if (transform.position == position)
                {
					// Change player state to interacting
					playerState = States.Interacting;
					break;
                }
            }
        }
		private IEnumerator MoveAwayFromChair(Transform chair)
        {
			// Remove ui message 
			interactionMessageHolder.SetActive(false);
			// While player position is not his previous one
            while (transform.position != prevPosition)
			{
				// Update Frame
				yield return new WaitForEndOfFrame();
				// Move player away from its position
				transform.position = Vector3.MoveTowards(transform.position, prevPosition, 0.5f * Time.deltaTime);
				// If player's position is similar to his prevoius
				if (transform.position == prevPosition)
                {
					if (chair != null)
                    {
						// Activate the chair collision
						chair.GetComponent<BoxCollider>().enabled = true;
						// Play standing sound
						chair.GetChild(5).GetComponent<SittingSound>().PlaySound("stand");
						// Hide ui message
						interactionMessageHolder.SetActive(false);
					}
					// Change player's state to walking
					playerState = States.Walking;
					break;
                }
            }
        }

		// Hide door is loacked message after 2 seconds
		private IEnumerator HideDoorMessage(float timer, string message)
        {
			GameObject uiMessage = null;
			if (message == "door")
			{
				uiMessage = doorLockedMessageHolder;
				while (timer > 0.1f)
				{
					// Update Frame
					yield return new WaitForEndOfFrame();
					// Update timer
					timer -= Time.deltaTime;
					// if timer is finished
					if (timer < 0.1f)
					{
						// Hide message
						uiMessage.SetActive(false);
						break;
					}
				}
			}
			else if (message == "puzzle")
			{
				// Exit Interaction Mode
				uiMessage = puzzleSolvedMessageHolder;
				yield return StartCoroutine(MoveAwayFromChair(null));
				uiMessage.SetActive(false);
			}			
        }

		// Open door with lerp
		private IEnumerator RotateDoor(string state)
		{
			float timer = 0f;

			
			while (timer < 5f)
			{
				yield return new WaitForEndOfFrame();

				timer += Time.deltaTime;
				if (state == "open")
                {
					barDoor.transform.rotation = Quaternion.Lerp(barDoor.transform.rotation, Quaternion.Euler(0, -120.607f, 0), Mathf.Clamp01(timer / 20));
                }
				if (state == "close")
                {
					barDoor.transform.rotation = Quaternion.Lerp(barDoor.transform.rotation, doorOriginalRotation, Mathf.Clamp01(timer / 20));
				}

			}
		}

		private IEnumerator FadeOutBarSound()
        {
			float timer = 0f;

			AudioSource barSource = barAudioSound.GetComponent<AudioSource>();
			while (timer < 2f)
            {
				yield return new WaitForEndOfFrame();
				timer += Time.deltaTime;
				barSource.volume = Mathf.Lerp(1, 0, Mathf.Clamp01(timer));
				
				
				if (timer >= 2)
                {
					barSource.clip.UnloadAudioData();
					seaSound.PlaySound();
					StartCoroutine(EndingText());
					break;
                }
            }
        }

		
		private IEnumerator EndingText()
        {
			float timer = 0;
			EndingSeaTextHolder.SetActive(true);
			
			while (timer < 10)
            {
				yield return new WaitForEndOfFrame();
				if (gamePaused == false)
                {
					timer += Time.deltaTime;
                }

				if (timer >= 10)
                {
					creditsRolling = true;
					// Remove ending text from UI
					EndingSeaTextHolder.SetActive(false);
					// Fade out to black and credits
					crossFade.enabled = true;
					crossFade.SetTrigger("start");
					// Wait for 3 seconds
					yield return new WaitForSeconds(3);
					// Show credits
					credits.gameObject.SetActive(true);
					credits.SetTrigger("roll");
					// Wiat for 22 seconds
					yield return new WaitForSeconds(20);
					// Fade out sea sound
					seaSound.InitiateFadeOut();
					break;
                }
            }

		}
		private IEnumerator PlayerFocused()
        {
			float timer = 0;

			while (timer < 2f)
            {
				yield return new WaitForEndOfFrame();
				timer += Time.deltaTime;

				if (timer >= 2f)
                {
					yield return StartCoroutine(ShowKeysUI());
					playerFocused = true;
					break;
                }
            }
        }
		private IEnumerator ShowKeysUI()
        {
			float timer = 0;
			UIHelpMessageHolder.SetActive(true);

			while (timer < 5f)
            {
				yield return new WaitForEndOfFrame();

				timer += Time.deltaTime;
				if (pauseMenu.activeSelf == true)
                {
					UIHelpMessageHolder.SetActive(false);
					timer -= Time.deltaTime;
                }
				else
                {
					UIHelpMessageHolder.SetActive(true);
                }
				if (timer >= 5)
                {
					UIHelpMessageHolder.SetActive(false);
					break;
                }
            }
        }

		private IEnumerator ShowPuzzleErrorMessage()
        {
			errorMessage.SetActive(true);
			float timer = 0f;

			while (timer < 2f)
            {
				yield return new WaitForEndOfFrame();
				timer += Time.deltaTime;

				if (timer >= 2f)
                {
					errorMessage.SetActive(false);
					break;
                }
            }
        }

		
	
	}
}