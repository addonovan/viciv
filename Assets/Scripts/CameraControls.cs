using UnityEngine;

/// <summary>
/// This adds special functionality to the camera.
/// 
/// This has a lot of arbitrary constants in here.
/// </summary>
public class CameraControls : MonoBehaviour
{

    //
    // Constants
    //

    /// <summary>
    /// A scalar for movement to make it not horrendously slow.
    /// </summary>
    private const float MOVEMENT_SPEED = 25f;

    /// <summary>
    /// A scalar for mouse scrolling, used for change the rotation of the camera.
    /// </summary>
    private const float MOUSE_SCROLLRATE = -45f;

    /// <summary>
    /// A scalar for the zoom rate, used for the movemnet of the camera as it zooms.
    /// </summary>
    private const float ZOOM_RATE = 35f;

    /// <summary>
    /// A value used to scale the additional movement speed gained when zooming in and
    /// out of the map.
    /// </summary>
    private const float ANGLE_MOVEMENT_SCALAR = 4f;

    /// <summary>
    /// An arbitraty coefficient analogous to friction. This removes some of the built up
    /// zoom energy.
    /// </summary>
    private const float ZOOM_FRICTION = 7.5f;

    /// <summary>
    /// The minimum pitch the camera can go to.
    /// </summary>
    private const float MIN_PITCH = 20f;

    /// <summary>
    /// The maximum pitch the camera can go to.
    /// </summary>
    private const float MAX_PITCH = 90f;

    /// <summary>
    /// The highest the camera can go before being blocked.
    /// </summary>
    private const float MAX_HEIGHT = 500f;

    /// <summary>
    /// The height at which the camera will stop pitching and shift to zoom alone.
    /// </summary>
    private const float ZOOM_SHIFT_HEIGHT = 100f;

    /// <summary>
    /// The lowest the camera can go before being blocked.
    /// </summary>
    private const float MIN_HEIGHT = 25f;

    /// <summary>
    /// The constant factor, a, for the zoom quadratic function that will
    /// achieve a height of ZOOM_SHIFT_HEIGHT and a minimum of MIN_HEIGHT.
    /// </summary>
    private const float ZOOM_FUNCTION_CONSTANT = ( ZOOM_SHIFT_HEIGHT - MIN_HEIGHT ) / ( ( MAX_PITCH - 5f ) * ( MAX_PITCH - 5f ) );

    //
    // Unity Properties
    //

    /// <summary>
    /// The total energy that has been built up while zooming.
    /// 
    /// Mouse scroll deltas are added this this and slowly removed via the zoom "friction",
    /// and this is the value used to determine how much to scroll during an update.
    /// </summary>
    public float zoomEnergy = 0;

    //
    // Properties
    //

    /// <summary>
    /// The rotation of the camera around the x axis.
    /// </summary>
    public float pitch
    {
        get
        {
            return cameraRotation.x;
        }
    }

    //
    // Fields
    //

    /// <summary>
    /// The rotation we're going to use for our camera. Because using the default
    /// eulerAngle things for some reason will only give values between 90 and 270.
    /// And they're also mirrored? Like, scrolling in for 180° will bring you from 90°
    /// to 0° to 270° (as it should); but so will scrolling out. So this is what
    /// we're doing instead.
    /// 
    /// The problem is all because of Quaternions. A lot of problems are because of
    /// Quaternions.
    /// 
    /// Quaternions, man, how do they work??
    /// </summary>
    private Vector3 cameraRotation = new Vector3( 0f, 0f, 0f );

    //
    // Unity Hooks
    //

    void Start()
    {
        cameraRotation.x = transform.eulerAngles.x; // initialize this to whatever it starts at

        // center the camera at first
        Vector3 position = transform.position;
        position.x = World.xLength / 2f;
        position.z = World.zLength / 2f;
        transform.position = position;

        Debug.Log( "Center of world: " + position );

        // set this up so we can use white to see the UI in the editor
        Camera.main.backgroundColor = Color.black;
    }

    void Update()
    {
        // Moves the camera around
        MoveCamera();

        // scroll the camera (also angles it )
        ScrollCamera( Controls.zoomDelta );
    }

    //
    // Actions
    //

    /// <summary>
    /// Moves the camera around and keeps it in bounds.
    /// </summary>
    private void MoveCamera()
    {
        // when the user zooms in and out, the camera must move faster and slower as it gets further out and closer in (respectively)
        float movementFromAngle = ANGLE_MOVEMENT_SCALAR * MOVEMENT_SPEED * Mathf.Sin( transform.eulerAngles.x * Mathf.Deg2Rad );

        // calculate the total movement value here (because writing it four times it reeeaaaalllllyyyy annoying if it needs
        // to be changed ever
        float movement = Time.deltaTime * ( MOVEMENT_SPEED + movementFromAngle );

        // move faster when left shift is placed
        if ( Controls.sprint )
        {
            movement *= 3f;
        }

        // move along the camera's x axis (which is also the world's x axis)
        if ( Controls.moveLeft )
        {
            transform.Translate( -movement, 0f, 0f );
        }
        else if ( Controls.moveRight )
        {
            transform.Translate( movement, 0f, 0f );
        }

        // move along the WORLD'S z axis (as the camera angle can change, and doing the vector calculations for that is annoying)
        if ( Controls.moveForward )
        {
            transform.Translate( 0f, 0f, movement, World.transform );
        }
        else if ( Controls.moveBackward )
        {
            transform.Translate( 0f, 0f, -movement, World.transform );
        }

        // keep the camera in bounds
        Vector3 position = transform.position;

        position.x = Mathf.Max( position.x, -50f ); // correct it for going too far to the left
        position.x = Mathf.Min( position.x, World.xLength + 100f ); // too far to the right

        // accounts for the angle of the camera and how it views the screen
        float distanceToView = 0f;

        // if this gets too low, the camera starts spazzing out, as Lim(x -> Inf)[Cot(x)]=Inf
        if ( cameraRotation.x > 15f )
        {
            distanceToView = position.y / ( Mathf.Tan( Mathf.Deg2Rad * cameraRotation.x ) );
        }

        position.z = Mathf.Max( position.z, -distanceToView ); // too far backward
        position.z = Mathf.Min( position.z, World.zLength - distanceToView ); // too far forwards

        // update the camera's position
        transform.position = position;
    }

    /// <summary>
    /// Scrolls the camera based on the amount that the mouse wheel has scrolled.
    /// </summary>
    /// <param name="delta">The mouse wheel's y delta.</param>
    private void ScrollCamera( float delta )
    {
        // add our new delta
        zoomEnergy += delta;

        // cap the energy
        if ( zoomEnergy > 4f )
        {
            zoomEnergy = 4f;
        }
        else if ( zoomEnergy < -4f )
        {
            zoomEnergy = -4f;
        }

        // almost everything takes this
        float timeAndEnergy = Time.deltaTime * zoomEnergy;

        // calculate the common movement factor
        float movement = ZOOM_SHIFT_HEIGHT * timeAndEnergy;

        // the current position of the camera
        Vector3 position = transform.position;

        // only rotate the camera if we're not going to cause the camera to drop out of zoom mode
        if ( position.y < ZOOM_SHIFT_HEIGHT )
        {
            cameraRotation.x += MOUSE_SCROLLRATE * timeAndEnergy;
        }

        // actually update the rotation of the camera
        ZoomStatus status = GetZoomStatus();

        // clamp the rotation to [minPitch, maxPitch]
        if ( cameraRotation.x < MIN_PITCH ) cameraRotation.x = MIN_PITCH;
        else if ( cameraRotation.x > MAX_PITCH ) cameraRotation.x = MAX_PITCH;

        // if we're in free mode (both pitch and zoom can change) OR we're too low, but also zooming out
        if ( status == ZoomStatus.FREE )
        {
            // add the extra information just for this method
            movement += timeAndEnergy * ( 45f * Mathf.Sin( transform.eulerAngles.x * Mathf.Deg2Rad ) );

            transform.Translate( 0f, 0f, movement );

            position.y = ZOOM_FUNCTION_CONSTANT * Mathf.Pow( cameraRotation.x - 5, 2 ) + MIN_HEIGHT;
        }
        // if we're in zoom only mode OR we're too high, but also zooming in
        else if ( status == ZoomStatus.ZOOM_ONLY )
        {
            // subtraction because zoomEnergy < 0 when zooming out (i.e going UP)
            position.y -= movement;

            cameraRotation.x = MAX_PITCH;
        }
        else if ( status == ZoomStatus.TOO_LOW )
        {
            zoomEnergy = 0f;
            position.y = Mathf.Max( position.y, MIN_HEIGHT );
            cameraRotation.x = Mathf.Max( cameraRotation.x, MIN_PITCH );
        }
        else if ( status == ZoomStatus.TOO_HIGH )
        {
            zoomEnergy = 0f;
            position.y = Mathf.Min( position.y, MAX_HEIGHT );
            cameraRotation.x = Mathf.Min( cameraRotation.x, MAX_PITCH );
        }

        // update our transform
        transform.position = position;
        transform.eulerAngles = cameraRotation;

        // bring it closer to zero
        if ( zoomEnergy < 0 )
        {
            zoomEnergy = Mathf.Min( 0f, zoomEnergy + ( ZOOM_FRICTION * Time.deltaTime ) );
        }
        else if ( zoomEnergy > 0 )
        {
            zoomEnergy = Mathf.Max( 0f, zoomEnergy - ( ZOOM_FRICTION * Time.deltaTime ) );
        }
    }

    /// <summary>
    /// Enforces the rules that the pitch must be between 5° and 85° (inclusive).
    /// </summary>
    /// <returns>If the rotation of the camera was modified.</returns>
    private ZoomStatus GetZoomStatus()
    {
        cameraRotation.x %= 360; // always do this just in case

        int pitch = Mathf.RoundToInt( cameraRotation.x );

        // ensure the camera doesn't go too low
        if ( pitch <= MIN_PITCH || pitch >= 270 )
        {
            // if we're too low, but zooming out, then we're not bounded
            if ( zoomEnergy < 0f ) return ZoomStatus.FREE;

            return ZoomStatus.TOO_LOW;
        }
        // ensure the camera doesn't go too high
        else if ( transform.position.y >= MAX_HEIGHT )
        {
            // if we're too high, but zooming in, then we're not bounded
            if ( zoomEnergy > 0f ) return ZoomStatus.ZOOM_ONLY;

            return ZoomStatus.TOO_HIGH;
        }
        // let the camera zoom out (no pitch changes) after a certain height
        else if ( transform.position.y >= ZOOM_SHIFT_HEIGHT )
        {
            return ZoomStatus.ZOOM_ONLY;
        }

        // we've hit none of the barriers, we're free to do anything
        return ZoomStatus.FREE;
    }

    /// <summary>
    /// The current status of zooming.
    /// 
    /// FREE: The camera is able to zoom & pitch
    /// ZOOM_ONLY: The camera may only zoom
    /// TOO_LOW: The camera can't zoom in anymore
    /// TOO_HIGH: The camera can't zoom out anymore
    /// </summary>
    private enum ZoomStatus
    {
        FREE,
        ZOOM_ONLY,
        TOO_LOW,
        TOO_HIGH
    }

}
