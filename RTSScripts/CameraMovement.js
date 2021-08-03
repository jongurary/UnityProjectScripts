import System.Collections;

public class CameraMovement extends MonoBehaviour{

public var fov = 60;
public var fovmax = 90;
public var min =30;
 
var ScrollSpeed:float = 15;
var ScrollEdge:float = 0.01;

private var HorizontalScroll:int = 1;
private var VerticalScroll:int = 1;
private var DiagonalScroll:int = 1;

var PanSpeed:float = 10;
var ZoomRange:Vector2 = Vector2(-5,5);
var CurrentZoom:float = 0;
var ZoomZpeed:float = 1;
var ZoomRotation:float = 1;
var rotationSpeed : float = 1.0;
var startX : float = 0;
var startZ : float = 0;
//var minCamHeight:float;
//var maxCamHeight:float;


//TODO replace this entire dumb script
function Start()

{   
	Camera.main.transform.localEulerAngles = new Vector3(65f, 0f, 0f);
    
 	Camera.main.transform.position.y = 350f;
 	//Terrain.activeTerrain.SampleHeight(GetComponent.<Camera>().transform.position)-30; //old method scaled by terrian height
 	Camera.main.transform.position.x = startX;
 	Camera.main.transform.position.z = startZ;
 	
 //	if(Camera.main.transform.position.y > 150f)
 //		Camera.main.transform.y = 100f;
 	
 //	minCamHeight = 65;
//	maxCamHeight = 75;
	
	//	gameObject.GetComponent(Camera).enabled=false;
	//	gameObject.GetComponent(AudioListener).enabled=false;
	
}

 

function Update () 

{


    //PAN
	//minCamHeight = 65;
	//maxCamHeight = 75;
    if ( Input.GetKey("mouse 2") )
    {
        //(Input.mousePosition.x - Screen.width * 0.5)/(Screen.width * 0.5)
   //     transform.Translate(Vector3.right * Time.deltaTime * PanSpeed * (Input.mousePosition.x - Screen.width * 0.5)/(Screen.width * 0.5), Space.World);
   //     transform.Translate(Vector3.forward * Time.deltaTime * PanSpeed * (Input.mousePosition.y - Screen.height * 0.5)/(Screen.height * 0.5), Space.World);
    } else {
        if ( Input.GetKey("right") || Input.mousePosition.x >= Screen.width * (1 - ScrollEdge) 
      		   && (Input.mousePosition.x <= Screen.width+50f) ){
			transform.Translate(Time.deltaTime * ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),0, Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
        //    transform.Translate(Vector3.right * Time.deltaTime * ScrollSpeed, Space.World);
        
        }else if ( Input.GetKey("left") || (Input.mousePosition.x <= Screen.width * ScrollEdge)
        		&& (Input.mousePosition.x >= -50f) ){
			transform.Translate(Time.deltaTime * -ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),0, Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
       //     transform.Translate(Vector3.right * Time.deltaTime * -ScrollSpeed, Space.World);
        }

        

        if ( Input.GetKey("up") || Input.mousePosition.y >= Screen.height * (1 - ScrollEdge) 
        		&& (Input.mousePosition.y <= Screen.height+50f) ){
            transform.Translate(Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),0, Time.deltaTime * ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
         //   transform.Translate(Vector3.forward * Time.deltaTime * ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
        } else if ( Input.GetKey("down") || Input.mousePosition.y <= Screen.height * ScrollEdge 
      		    && (Input.mousePosition.y >= -50f)) {
			transform.Translate(Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),0, Time.deltaTime * -ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
 //           transform.Translate(Vector3.forward * Time.deltaTime * -ScrollSpeed*System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
         //   transform.Translate(Vector3.forward * Time.deltaTime * -ScrollSpeed*System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), Space.World);
        }
    }

   // var fwd = transform.TransformDirection (Vector3.forward);
	//	if (Physics.Raycast (transform.position, fwd, 10)) {
	//		print (fwd);
	//	}

//ZOOM IN/OUT
    var fwd = transform.TransformDirection (Vector3.forward);
    if (Input.GetAxis("Mouse ScrollWheel")> 0){
		transform.Translate(
			2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
			2*Time.deltaTime * ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			Space.World
		);
	//	if((Terrain.activeTerrain.SampleHeight(camera.transform.position)+70)<40){
	//		transform.Translate(
	//		2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
	//		2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
	//		2*Time.deltaTime * -ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
	//		Space.World
	//	);
	//	}
		
		if(Camera.main.transform.rotation.eulerAngles.x <= 180){
		if (Physics.Raycast (transform.position, fwd, 50)) {
			transform.Translate(
			2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
			2*Time.deltaTime * -ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			Space.World
		);
		}
		}
		if(Camera.main.transform.rotation.eulerAngles.x >= 180){
		if (!(Physics.Raycast (transform.position, -fwd, 150))) {
			transform.Translate(
			2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
			2*Time.deltaTime * -ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			Space.World
		);
		}
	    }
//		Camera.main.fieldOfView = fov--;

	}

	if (Input.GetAxis("Mouse ScrollWheel")< 0){
		
		transform.Translate(
			2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
			2*Time.deltaTime * -ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180), 
			Space.World
			
			//0.2*System.Math.Pow((Terrain.activeTerrain.SampleHeight(camera.transform.position))/(System.Math.Cos((90-Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)),2)
		);
		
//		var fwd = transform.TransformDirection (Vector3.forward);
        
        if(Camera.main.transform.rotation.eulerAngles.x >= 180){
		if (Physics.Raycast (transform.position, -fwd, 50)) {
			transform.Translate(
			2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
			2*Time.deltaTime * ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			Space.World
		);
		}
		}
		
		if(Camera.main.transform.rotation.eulerAngles.x <= 180){
		if (!(Physics.Raycast (transform.position, fwd, 150))) {
			transform.Translate(
			2*Time.deltaTime * ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			2*Time.deltaTime * -ScrollSpeed *System.Math.Sin((Camera.main.transform.rotation.eulerAngles.x)*System.Math.PI/180)*System.Math.Pow(2.0,0.5),
			2*Time.deltaTime * ScrollSpeed *System.Math.Cos((Camera.main.transform.rotation.eulerAngles.y)*System.Math.PI/180),
			Space.World
		);
		}
		}
//		Camera.main.fieldOfView = fov++;

	}
	
	



// ROTATION

    if (Input.GetKey("left ctrl") && Input.GetMouseButton(1)){
		var h : float = rotationSpeed * Input.GetAxis ("Mouse X");
		var v : float = rotationSpeed * Input.GetAxis ("Mouse Y");
		transform.Rotate (0, h, 0, Space.World);
		transform.Rotate (v, 0, 0);

		if((Camera.main.transform.rotation.eulerAngles.x >= 90) &&(Camera.main.transform.rotation.eulerAngles.x <= 180)){
			transform.Rotate (-v, 0, 0);
		}
		if(((Camera.main.transform.rotation.eulerAngles.x >= 180)&&(Camera.main.transform.rotation.eulerAngles.x <= 270))||(Camera.main.transform.rotation.eulerAngles.x < 0) ){
			transform.Rotate (-v, 0, 0);
		}

    	if((Camera.main.transform.rotation.eulerAngles.z >= 160)&&(Camera.main.transform.rotation.eulerAngles.z <= 200)){
    		transform.Rotate (-v, 0, 0);
    	}
	}	
// Following terraint (optional)
	//	if(Terrain.activeTerrain.SampleHeight(camera.transform.position)+70<minCamHeight){
	 //   	Camera.main.transform.position.y = minCamHeight;
	//    }
	//    if(Terrain.activeTerrain.SampleHeight(camera.transform.position)+70>maxCamHeight){
	//    	Camera.main.transform.position.y = maxCamHeight;
	//    }
}
}
		
    
//        var horiz : float = Input.GetAxis("Horizontal");
// transform.Translate(Vector3(horiz,0,0));
// 		var vert : float = Input.GetAxis("Vertical");
// transform.Translate(Vector3(0,0,vert));
 
 //Mouse ScrollWheel
//}
