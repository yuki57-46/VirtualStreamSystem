using UnityEngine;

public class VRMBodyController : MonoBehaviour
{
    [SerializeField] private VRMLoder vrmLoder;
    private Animator animator;

    [Header("Body Rotation (default: A-Pose")]
    // 左腕上部の回転
    public Vector3 leftArmRotation = new Vector3(0.0f, 0.0f, 30.0f);
    // 右腕上部の回転
    public Vector3 rightArmRotation = new Vector3(0.0f, 0.0f, -30.0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var vrmModel = vrmLoder.VRMModel;

        if (vrmModel != null)
        {
            animator = vrmModel.GetComponent<Animator>();

            SetAPose();
        }

    }

    // Update is called once per frame
    void Update()
    {

        //SetAPose();

    }

    public void SetAPose()
    {
        if (animator == null)
        {
            animator = vrmLoder.VRMModel.GetComponent<Animator>();
            if (animator == null)
                Debug.LogWarning("animator is null");
            return;
        }

        Transform leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        Transform rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);

        if (leftArm != null)
        {
            leftArm.localEulerAngles = leftArmRotation;
        }
        if (rightArm != null)
        {
            rightArm.localEulerAngles = rightArmRotation;
        }

    }
}
