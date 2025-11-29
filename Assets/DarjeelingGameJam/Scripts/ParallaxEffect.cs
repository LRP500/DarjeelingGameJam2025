using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using UnityEngine;
using Sirenix.OdinInspector;

[DisallowMultipleComponent]
public class ParallaxEffect : MonoBehaviour
{
    public bool dontUseParallax;
    float startPosX, startPosY, startPosZ;

    public float parallaxStrengthX;
    public float parallaxStrengthY;
    public bool usingZPos = true;
    public float strengthUsingZPos = 0.08f;
    public bool cameraMovement = true;
    public float pivotPoint;
    public bool useZephyrAsPivotPoint = true;
    //public bool noParallaxWithinThresholdAroundPivot;
    public float thresholdAroundPivot = 0; //1.5f;
    public float strengthUsingZPosWithThreshold = 0.007f;
    public float offsetFromZephyrIfPivotPoint;
    float originalStrength;
    float factorWhenUsingZPos;
    GameObject zephyr;
    GameObject parallaxTarget;
    [SerializeField] bool onlyMoveHorizontally;
    [SerializeField] bool onlyMoveVertically;
    [SerializeField] bool useSeparateXAndYStrengthsWithZPos;
    [SerializeField] Vector2 strengthsIfUsingSeparateXandYVals;
    [SerializeField] float forcingZPosTimeInterval = 0.1f;
    [SerializeField] bool showDetailedWarning;
    [SerializeField] bool canForceZPos = true;
    [SerializeField] float zDriftTolerance = 0.0001f;

    private Vector3 previousFrameOffset;
    private Vector3 previousFrameLocalOffset;
    float distanceX;
    float distanceY;
    Vector3 originalLocalPos;
    private Vector3 oldLocalPos;
    GameObject parentParallaxEffect;

    bool shouldForceZPos;

    Coroutine forceZPosCoroutine;

    Quaternion parentInverse;
    Quaternion parentRotation;

    MoveWithMouse moveWithMouse;

    bool canUpdate;

    public GameObject ParentParallaxEffect { get => parentParallaxEffect; set => parentParallaxEffect = value; }
    public float DistanceX { get => distanceX; set => distanceX = value; }
    public float DistanceY { get => distanceY; set => distanceY = value; }
    public Quaternion ParentInverse { get => parentInverse; set => parentInverse = value; }
    public Quaternion ParentRotation { get => parentRotation; set => parentRotation = value; }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(WaitUntilCanUseParallax());
        //if (dontUseParallax) return;        
    }

    IEnumerator WaitUntilCanUseParallax()
    {
        yield return new WaitUntil(() => !dontUseParallax);
        Initialize();
    }

    void Initialize()
    {
        if (transform.parent != null)
        {
            if (transform.parent.GetComponentInParent<ParallaxEffect>() != null)
            {
                ParentParallaxEffect = transform.parent.GetComponentInParent<ParallaxEffect>().gameObject;
            }
        }

        previousFrameOffset = transform.position;
        previousFrameLocalOffset = transform.localPosition;

        originalLocalPos = transform.localPosition;

        /* if (zephyr == null)
        { */
        zephyr = UtilitiesReferences.Instance.Zephyr.gameObject;
        //}

        //moveWithMouse = zephyr.GetComponentInChildren<MoveWithMouse>();
        moveWithMouse = FindObjectOfType<MoveWithMouse>();

        /* if (parallaxTarget == null)
        { */
        parallaxTarget = moveWithMouse.gameObject;
        //}

        originalStrength = strengthUsingZPos;

        startPosX = transform.position.x;
        startPosY = transform.position.y;
        startPosZ = transform.position.z;

        canUpdate = true;
    }

    public void UpdateStrength(float _newStrength)
    {
        strengthUsingZPos = _newStrength;
        originalStrength = strengthUsingZPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (!canUpdate) return;
        if (dontUseParallax || !moveWithMouse.canMoveWithMouse) return;

        if (ParentParallaxEffect == null)
        {
            float distanceCameraX;
            float distanceCameraY;

            if (usingZPos)
            {
                if (useZephyrAsPivotPoint)
                {
                    pivotPoint = zephyr.transform.position.z + offsetFromZephyrIfPivotPoint;
                }

                if (Math.Abs(pivotPoint - transform.position.z) <= thresholdAroundPivot)
                {
                    strengthUsingZPos = strengthUsingZPosWithThreshold;
                }
                else
                {
                    strengthUsingZPos = originalStrength;
                }

                float strengthParallaxX;
                float strengthParallaxY;

                if (useSeparateXAndYStrengthsWithZPos)
                {
                    strengthParallaxX = strengthsIfUsingSeparateXandYVals.x;
                    strengthParallaxY = strengthsIfUsingSeparateXandYVals.y;
                }
                else
                {
                    strengthParallaxX = strengthUsingZPos;
                    strengthParallaxY = strengthUsingZPos;
                }

                distanceCameraX = parallaxTarget.transform.localPosition.x * (pivotPoint - transform.position.z) * strengthParallaxX;
                distanceCameraY = parallaxTarget.transform.localPosition.y * (pivotPoint - transform.position.z) * strengthParallaxY;

                //factorWhenUsingZPos = Mathf.Abs(transform.position.z) * Mathf.Sign(pivotPoint - transform.position.z) * strengthUsingZPos;

                DistanceX = distanceCameraX;
                DistanceY = distanceCameraY;
            }
            else
            {
                DistanceX = parallaxTarget.transform.localPosition.x * parallaxStrengthX;
                DistanceY = parallaxTarget.transform.localPosition.y * parallaxStrengthY;
            }

            if (cameraMovement)
            {
                if (onlyMoveHorizontally)
                {
                    DistanceY = 0;
                }
                if (onlyMoveVertically)
                {
                    DistanceX = 0;
                }

                Vector3 absoluteOffset = ComputeParallaxOffset(); // <-- your existing final calculation

                Vector3 offsetDelta = absoluteOffset - previousFrameOffset;
                offsetDelta.z = 0f;

                transform.position += offsetDelta;

                /* if (gameObject.name == "Sol 3 (1)")
                {
                    //Debug.Log($"{offsetDelta.z:F7}");
                    //Debug.Log(Mathf.Abs(offsetDelta.z) > 0);
                } */

                // can't do this otherwise we get some approximations over the time and some offsets between assets
                /* if (offsetDelta.sqrMagnitude > Mathf.Pow(10, -5))
                {
                    transform.position += offsetDelta;
                } */

                /* if (gameObject.name == "Sol 3 (1)")
                {
                    Debug.Log($"{transform.position.z - previousFrameOffset.z:F6}");
                    Debug.Log(Mathf.Abs(transform.position.z - previousFrameOffset.z) > 0);
                    transform.position += new Vector3(0, 0, previousFrameOffset.z - transform.position.z);
                } */

                /* if (gameObject.name == "Mini ground 6 (1)")
                { */
                if (Mathf.Abs(transform.position.z - previousFrameOffset.z) > 0 && canForceZPos)
                {
                    //Debug.Log("original z pos: " + originalLocalPos.z);
                    shouldForceZPos = true;
                    if (forceZPosCoroutine == null)
                    {
                        forceZPosCoroutine = StartCoroutine(ForcingZPos());
                    }
                    if (gameObject.transform.parent != null)
                    {
                        if (showDetailedWarning)
                        {
                            Debug.Log("Parallax effect wargning: object " + gameObject.name + " has a moving z transform. Consider updating z scale of " + gameObject.transform.parent.name + " to 1");
                        }
                        else
                        {
                            //Debug.Log("Parallax effect wargning. Consider updating z scale of " + gameObject.transform.parent.name + " to 1");
                        }
                    }
                    else
                    {
                        Debug.Log("Parallax effect wargning:object " + gameObject.name + " has a moving z transform. No parent found");
                    }
                }
                //}

                previousFrameOffset = absoluteOffset;
            }
        }
        else
        {
            float distanceCameraX;
            float distanceCameraY;

            if (usingZPos)
            {
                if (useZephyrAsPivotPoint)
                {
                    pivotPoint = zephyr.transform.position.z + offsetFromZephyrIfPivotPoint;
                }

                if (Math.Abs(pivotPoint - transform.position.z) <= thresholdAroundPivot)
                {
                    strengthUsingZPos = strengthUsingZPosWithThreshold;
                }
                else
                {
                    strengthUsingZPos = originalStrength;
                }

                float strengthParallaxX;
                float strengthParallaxY;

                if (useSeparateXAndYStrengthsWithZPos)
                {
                    strengthParallaxX = strengthsIfUsingSeparateXandYVals.x;
                    strengthParallaxY = strengthsIfUsingSeparateXandYVals.y;
                }
                else
                {
                    strengthParallaxX = strengthUsingZPos;
                    strengthParallaxY = strengthUsingZPos;
                }

                distanceCameraX = parallaxTarget.transform.localPosition.x * (ParentParallaxEffect.transform.position.z - transform.position.z) * strengthParallaxX * Mathf.Sign(ParentParallaxEffect.transform.lossyScale.x) / Mathf.Abs(ParentParallaxEffect.transform.lossyScale.x);
                distanceCameraY = parallaxTarget.transform.localPosition.y * (ParentParallaxEffect.transform.position.z - transform.position.z) * strengthParallaxY * Mathf.Sign(ParentParallaxEffect.transform.lossyScale.y) / Mathf.Abs(ParentParallaxEffect.transform.lossyScale.y);
                //factorWhenUsingZPos = Mathf.Abs(transform.position.z) * Mathf.Sign(pivotPoint - transform.position.z) * strengthUsingZPos;

                DistanceX = distanceCameraX;
                DistanceY = distanceCameraY;
            }
            else
            {
                DistanceX = parallaxTarget.transform.localPosition.x * parallaxStrengthX;
                DistanceY = parallaxTarget.transform.localPosition.y * parallaxStrengthY;
            }

            if (cameraMovement)
            {
                if (onlyMoveHorizontally)
                {
                    DistanceY = 0;
                }
                if (onlyMoveVertically)
                {
                    DistanceX = 0;
                }

                Vector3 absoluteOffset = ComputeParallaxLocalOffset();

                Vector3 offsetDelta = absoluteOffset - previousFrameLocalOffset;
                offsetDelta.z = 0;

                ParentInverse = Quaternion.Inverse(ParentParallaxEffect.transform.rotation);
                ParentRotation = ParentParallaxEffect.transform.rotation;
                Vector3 localOffset = ParentInverse * offsetDelta;

                localOffset.z = 0f;
                transform.localPosition += localOffset;

                /* if (localOffset.sqrMagnitude > Mathf.Pow(10, -5))
                {
                    transform.localPosition += localOffset;
                } */

                if (Mathf.Abs(transform.localPosition.z - previousFrameLocalOffset.z) > 0 && canForceZPos)
                {
                    //Debug.Log("original z pos: " + originalLocalPos.z);
                    shouldForceZPos = true;
                    if (forceZPosCoroutine == null)
                    {
                        forceZPosCoroutine = StartCoroutine(ForcingZPos());
                    }
                    if (gameObject.transform.parent != null)
                    {
                        if (showDetailedWarning)
                        {
                            Debug.Log("Parallax effect wargning: object " + gameObject.name + " has a moving z transform. Consider updating z scale of " + gameObject.transform.parent.name + " to 1");
                        }
                        else
                        {
                            Debug.Log("Parallax effect wargning. Consider updating z scale of " + gameObject.transform.parent.name + " to 1");
                        }
                    }
                    else
                    {
                        Debug.Log("Parallax effect wargning:object " + gameObject.name + " has a moving z transform. No parent found");
                    }
                }

                previousFrameLocalOffset = absoluteOffset;
            }
        }


    }

    public Vector3 ComputeParallaxOffset()
    {
        Vector3 something = new Vector3();

        something = new Vector3(startPosX + DistanceX,
                    startPosY + DistanceY,
                    transform.position.z);

        return something;
    }

    public Vector3 ComputeParallaxLocalOffset()
    {
        Vector3 something = new Vector3();

        something = new Vector3(originalLocalPos.x + DistanceX,
                    originalLocalPos.y + DistanceY,
                    transform.localPosition.z);

        return something;
    }

    // because of float point precision issues when parenting objects in objects with scale != 1, we do this to make sure z pos stays the same (not every frame so that transform can still be updated by other scripts / manually)
    IEnumerator ForcingZPos()
    {
        while (shouldForceZPos)
        {
            yield return new WaitForSeconds(forcingZPosTimeInterval);

            if (!canForceZPos)
                continue;

            if (ParentParallaxEffect == null)
            {
                float currentZ = transform.position.z;
                float delta = currentZ - startPosZ;

                if (Mathf.Abs(delta) <= zDriftTolerance)
                {
                    // Very small change -> treat as float drift and snap back
                    transform.position = new Vector3(transform.position.x, transform.position.y, startPosZ);
                }
                else
                {
                    // Big change -> assume intentional, update our reference Z
                    startPosZ = currentZ;
                }
            }
            else
            {
                float currentLocalZ = transform.localPosition.z;
                float delta = currentLocalZ - originalLocalPos.z;

                if (Mathf.Abs(delta) <= zDriftTolerance)
                {
                    // Small drift -> snap
                    transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, originalLocalPos.z);
                }
                else
                {
                    // Intentional move -> update reference
                    originalLocalPos.z = currentLocalZ;
                }
            }
        }

        // if we ever stop forcing Z in the future
        forceZPosCoroutine = null;
    }

    [Button]
    public void SetZ(float newZ)
    {
        if (ParentParallaxEffect == null)
        {
            Vector3 pos = transform.position;
            pos.z = newZ;
            transform.position = pos;

            startPosZ = newZ;
            previousFrameOffset.z = newZ;
        }
        else
        {
            Vector3 localPos = transform.localPosition;
            localPos.z = newZ;
            transform.localPosition = localPos;

            originalLocalPos.z = newZ;
            previousFrameLocalOffset.z = newZ;
        }
    }



    [Button]
    public void ResetPos()
    {
        dontUseParallax = true;
        transform.position = new Vector3(startPosX, startPosY, startPosZ);
    }

    [Button]
    public void ResetStartingPos()
    {
        Initialize();

        /* startPosX = transform.position.x;
        startPosY = transform.position.y;
        startPosZ = transform.position.z;

        originalLocalPos = transform.localPosition; */
    }

}
