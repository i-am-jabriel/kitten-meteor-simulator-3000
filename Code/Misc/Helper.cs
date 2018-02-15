using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Helper
{

	public static bool between(this float f, float a, float b)
	{
		return f >= a && f <= b;
	}
    public static float getDistance(this Component a,Component b)
    {
        return a.transform.getDistance(b.transform);
    }
    public static float getDistance(this Transform a,Transform b)
    {
        return a.position.getDistance(b.position);
    }
    public static float getDistance(this Vector3 a,Vector3 b)
    {
        return Vector3.Distance(a, b);
    }
    public static float getRatio(this Vector2 v)
    {
        return v.y - v.x / v.y;
    }
    public static float getRatio(this Vector3 v)
    {
        return v.y - v.x / v.y;
    }
    public static void addToY(this Transform me,float val)
    {
        me.position = me.position.addToY(val);
    }
    public static Vector3 addToY(this Vector3 me,float val)
    {
        return new Vector3(me.x, me.y + val, me.z);
    }
    public static Vector3 setYTo(this Vector3 me, float val)
    {
        return new Vector3(me.x, val, me.z);
    }
    public static T addGetComponent<T>(this GameObject go) where T : Component
	{
		T mono = go.GetComponent<T>();
		if (mono == null) mono = go.AddComponent<T>();
		return mono;
	}
	public static T FindComponentInChildWithTag<T>(this Component parent, string tag) where T : Component
	{
		Transform t = parent.transform;
		foreach (Transform tr in t)
		{
            if (tr.tag == tag)
            {
                return tr.GetComponent<T>();
            }
            else
            {
                T val = tr.FindComponentInChildWithTag<T>(tag);
                if (val && val.transform.tag == tag && val != tr) return val;
            }
        }
		return null;
	}
    public static float getYAngleTo(this Transform a, Transform b)
    {
        return getYAngleTo(a, b.position);
    }
    public static float getYAngleTo(this Transform a, Vector3 b)
    {
        return a.getTheta(b).eulerAngles.y.fixAngle();
    }
    public static float getAngleFromPoints(Vector2 p1, Vector2 p2)
    {
        return Mathf.Atan2(p2.y - p1.y, p2.x - p1.x) * 180 / Mathf.PI;
    }
    public static Quaternion getXZTheta(this Transform a, Transform b)
    {
        return a.getTheta(new Vector3(b.position.x, a.position.y, b.position.z));
    }
    public static Quaternion getTheta(this Transform a, Vector3 b)
    {
        Quaternion oldTheta = a.rotation;
        a.LookAt(b);
        Quaternion theta = a.rotation;
        a.rotation = oldTheta;
        return theta;
    }
    public static Quaternion getTheta(this Transform a, Transform b)
    {
        return a.getTheta(b.position);
    }
    public static float fixAngle(this float theta)
    {
        while (theta > 180) theta -= 360;
        while (theta < -180) theta += 360;
        return theta;
    }
}
