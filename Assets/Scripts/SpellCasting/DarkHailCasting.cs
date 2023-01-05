using Assets.Scripts.Spells;
using System.Collections;
using UnityEngine;
using static Assets.Scripts.Managers.UI.UImanager;

namespace Assets.Scripts.SpellCasting
{
  [RequireComponent(typeof(LineRenderer))]
  public class DarkHailCasting : MonoBehaviour
  {
    public SpellCastingState State = SpellCastingState.Ready;

    public GameObject darkFireBall;
    public GameObject aimCircle;

    public int energyCost;
    public float cooldownTime;
    public float radius;

    public float step;

    private Vector3 destination;
    private float height;

    Coroutine Aiming = null;
    GameObject aimDarkHail = null;
    LineRenderer lr = null;

    public void Awake()
    {
      lr = GetComponent<LineRenderer>();
    }

    //After mana cost, this spell as no requirement
    protected bool TryCast()
    {
      if (Aiming != null) return false;

      Aiming = StartCoroutine(Aim());
      return true;
    }

    //For now it follows the mouse.
    //In later developments this method has to change so that it works
    //both for the mouse and the AI.
    IEnumerator Aim()
    {
      State = SpellCastingState.Channeling;
      aimDarkHail = Instantiate(aimCircle, transform.position, aimCircle.transform.rotation);

      while (true)
      {
        Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out var hit, Mathf.Infinity, 1 << LayerMask.NameToLayer("Ground"));

        var distance = Vector3.Distance(hit.point, transform.position);
        var location = hit.point;

        if (distance > radius)
        {
          location = hit.point - transform.position;
          location.Normalize();
          location = transform.position + location * radius;
        }

        aimDarkHail.transform.position = location;
        destination = location;

        RenderArc();

        yield return null;
      }
    }

    //Stop aiming and shoot darkFireBall
    public void Shoot()
    {
      //Clean Aiming coroutine
      StopCoroutine(Aiming);
      Aiming = null;

      var instance = Instantiate(darkFireBall, transform.position, default);
      instance.TryGetComponent<DarkFireball>(out var script);
      script.destination = destination;
      script.trajectoryHeight = height;

      lr.positionCount = 0;
      Destroy(aimDarkHail);
      aimDarkHail = null;

      StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
      State = SpellCastingState.Cooldown;
      onSpellCooldown.Invoke("DarkHail", cooldownTime);
      yield return new WaitForSeconds(cooldownTime);
      State = SpellCastingState.Ready;
    }

    public void Stop()
    {
      if (Aiming != null)
      {
        StopCoroutine(Aiming);
        Aiming = null;

        Destroy(aimDarkHail);
        aimDarkHail = null;

        lr.positionCount = 0;

        StartCoroutine(Cooldown());
      }
    }

    public void RenderArc()
    {
      var direction = destination - transform.position;
      var groundDirection = new Vector3(direction.x, 0, direction.z);

      var targetPos = new Vector3(groundDirection.magnitude, direction.y, 0);

      height = targetPos.y + targetPos.magnitude / 2f;
      height = Mathf.Max(0.01f, height);

      CalculatePathWithHeight(targetPos, height, out var v0, out var angle, out var time);
      DrawPath(groundDirection.normalized, v0, angle, time, step);
    }

    private void CalculatePathWithHeight(Vector3 targetPos, float h, out float v0, out float angle, out float time)
    {
      var xt = targetPos.x;
      var yt = targetPos.y;
      var g = -Physics.gravity.y;

      var b = Mathf.Sqrt(2 * g * h);
      var a = -0.5f * g;
      var c = -yt;

      var tplus = (-b + 1 * Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
      var tminus = (-b - 1 * Mathf.Sqrt(b * b - 4 * a * c)) / (2 * a);
      time = tplus > tminus ? tplus : tminus;

      angle = Mathf.Atan(b * time / xt);
      v0 = b / Mathf.Sin(angle);
    }

    private void DrawPath(Vector3 direction, float v0, float angle, float time, float step)
    {
      step = Mathf.Max(0.01f, step);
      lr.positionCount = (int)(time / step) + 2;
      var count = 0;
      for (float i = 0; i < time; i += step, count++)
      {
        var x = v0 * i * Mathf.Cos(angle);
        var y = v0 * i * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(i, 2);

        lr.SetPosition(count, direction * x + Vector3.up * y);
      }

      var xfinal = v0 * time * Mathf.Cos(angle);
      var yfinal = v0 * time * Mathf.Sin(angle) - 0.5f * -Physics.gravity.y * Mathf.Pow(time, 2);
      lr.SetPosition(count, direction * xfinal + Vector3.up * yfinal);
    }
  }
}
