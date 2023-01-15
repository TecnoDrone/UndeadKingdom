using UnityEngine;

//https://www.stevestreeting.com/2019/02/22/enemy-health-bars-in-1-draw-call-in-unity/
/// <summary>
/// Displays a configurable health bar for any object with a Damageable as a parent
/// </summary>
public class HealthBar : MonoBehaviour
{

  MaterialPropertyBlock matBlock;
  MeshRenderer meshRenderer;
  Camera mainCamera;
  Entity entity;

  private void Awake()
  {
    meshRenderer = GetComponent<MeshRenderer>();
    matBlock = new MaterialPropertyBlock();
    // get the damageable parent we're attached to
    entity = GetComponentInParent<Entity>();
  }

  private void Start()
  {
    // Cache since Camera.main is super slow
    mainCamera = Camera.main;
  }

  private void Update()
  {
    // Only display on partial health
    if (entity.life < entity.maxLife)
    {
      meshRenderer.enabled = true;
      AlignCamera();
      UpdateParams();
    }
    else
    {
      meshRenderer.enabled = false;
    }
  }

  private void UpdateParams()
  {
    meshRenderer.GetPropertyBlock(matBlock);
    matBlock.SetFloat("_Fill", entity.life / (float)entity.maxLife);
    meshRenderer.SetPropertyBlock(matBlock);
  }

  private void AlignCamera()
  {
    if (mainCamera != null)
    {
      var camXform = mainCamera.transform;
      var forward = transform.position - camXform.position;
      forward.Normalize();
      var up = Vector3.Cross(forward, camXform.right);
      transform.rotation = Quaternion.LookRotation(forward, up);
    }
  }

}