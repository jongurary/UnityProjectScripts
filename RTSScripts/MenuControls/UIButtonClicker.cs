using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIButtonClicker : MonoBehaviour {

	public Texture2D cursorTexture;
	
	public Button[] buttons = new Button[40];
	//Scenes
	const int MAIN_MENU_SCENE = 0;
	const int TEST_SCENE = 1;
	const int CAMPAIGN_MISSION_1_SCENE = 2;

	//Main menu buttons
	const int MAIN_TEST = 1;
	const int MAIN_CAMPAIGN= 2;
	
	//Campaign sub-menu
	const int CAMP_BACK = 10;
	const int CAMP_CAMPAIGN_MISSION_1 = 11;


	public GameObject[] subMenus = new GameObject[10];
	const int MENU_MAIN = 0;
	const int MENU_CAMPAIGN = 1;


	int nextScene = 0; //gets passed to scene loader corountine to load next scene asynchronously
	bool loadingScene=false; //is a scene currently loading, prevents more than one at a time
	public GameObject loadScreen; //the "loading" UI that gets enabled/disabled when loading a scene

	void Start()
	{

		Cursor.SetCursor (cursorTexture, new Vector2(cursorTexture.width / 2, cursorTexture.height / 2), CursorMode.Auto);
		buttons[MAIN_TEST].onClick.AddListener(delegate {loadScene(TEST_SCENE);} );
		buttons[MAIN_CAMPAIGN].onClick.AddListener(delegate {swapMenus(MENU_MAIN, MENU_CAMPAIGN);} );

		buttons[CAMP_BACK].onClick.AddListener(delegate {swapMenus(MENU_CAMPAIGN, MENU_MAIN);} );
		buttons[CAMP_CAMPAIGN_MISSION_1].onClick.AddListener(delegate {loadScene(CAMPAIGN_MISSION_1_SCENE);} );
	}

	void swapMenus(int current, int next){
		subMenus [current].SetActive (false);
		subMenus [next].SetActive (true);
	}

	/**
	 * Loads the scene associated with the given number. See build settings for scene numbers.
	 * Scene numbers should be assigned as constants at the top of this class.
	 */
	void loadScene(int sceneNumber)
	{
		//Output this to console when the Button2 is clicked
		Debug.Log("Loading: " + sceneNumber);

		loadingScene = true;
		loadScreen.SetActive(true);

		nextScene = sceneNumber;
		// ...and start a coroutine that will load the desired scene.
		StartCoroutine(LoadSceneASync());

	}
	
	void ButtonClicked(int buttonNo)
	{
		//Output this to console when the Button3 is clicked
		Debug.Log("Button clicked = " + buttonNo);
	}
	
	IEnumerator LoadSceneASync() {
		

		AsyncOperation async = Application.LoadLevelAsync(nextScene);
		Text loadingText = loadScreen.GetComponentInChildren<Text> ();

		float time=0;
		while (!async.isDone) {
			//Strobes the text during load
			if (loadingScene && time>.25) {
				loadingText.color = Random.ColorHSV();
				time=0;
			}
			time+=Time.deltaTime;
			yield return null;
		}

		loadingScene = false;
		loadScreen.SetActive(false);
		SceneManager.LoadScene (nextScene);
	}

}
