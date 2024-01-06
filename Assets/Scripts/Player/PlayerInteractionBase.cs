using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerInteractionBase
{
    // List of objects that give the player a flip upon contact
    private readonly string[] m_FlipTagList = { "simplePlatform" };

    // Platform tag, platform direction, platform gameObject name
    private readonly List<Tuple<GameObject, int>> m_TouchingObjects = new ();

    public bool OnCollisionEnter2D(Collision2D collision, Vector3 pos)
    {
        // Adds the encountered object to the list
        int dir = FindCollisionDirection(collision, pos);
        m_TouchingObjects.Add(new Tuple<GameObject, int>(collision.gameObject, dir));

        // Check if the platform hit is eligible to give a flip
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            return true;
        }

        return false;
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        // Remove the object kura is not touching anymore from the list
        m_TouchingObjects.RemoveAt(m_TouchingObjects.FindIndex(e => e.Item1 == collision.gameObject));
    }

    // Function that finds the feet platform, returns the first platform that is below kura's legs
    // (relative to current kura gravity)
    public Tuple<GameObject, int> GiveFeetPlatform(int gravityDirection)
    {
        // NOTE: this probably should return all feet platforms, but irrelevant for now

        foreach (Tuple<GameObject, int> platform in m_TouchingObjects)
        {
            if ((gravityDirection == 1 && platform.Item2 == 2) ||
                (gravityDirection == -1 && platform.Item2 == 0))
            {
                return platform;
            }
        }

        return null;
    }

    public bool IsOnFloor(int gravityDirection)
    {
        return m_TouchingObjects.Any(platform => (gravityDirection == 1 && platform.Item2 == 2) ||
                                                 (gravityDirection == -1 && platform.Item2 == 0));
    }

    public bool IsKissWall()
    {
        return m_TouchingObjects.Any(platform => platform.Item2 == 1);
    }

    // Just finds which direction the hit object is relative to kura (not relative to kura's direction, aka. absolute)
    private int FindCollisionDirection(Collision2D collision, Vector3 pos)
    {
        // Direction is absolute
        // 0 - up, 1 -right, 2 - down, 3 - left
        List<(double, int)> directions = new ();

        if (collision.contacts.Length != 2)
        {
            Debug.Log("HELP HELP HELP HELP HELP BAD COLLISION");
            return -1;
        }

        double dx = collision.contacts[0].point.x - pos.x + collision.contacts[1].point.x - pos.x;
        double dy = collision.contacts[0].point.y - pos.y + collision.contacts[1].point.y - pos.y;

        directions.Add((dy, 0));
        directions.Add((dx, 1));
        directions.Add((-dy, 2));
        directions.Add((-dx, 3));

        directions.Sort(delegate ((double, int) a, (double, int) b) { return b.Item1.CompareTo(a.Item1); });

        return directions[0].Item2;
    }

    public float GetPeriodicDamageFromPlatmorms()
    {
        float damage = 0f;

        PlatformBasicScript platformData;
        foreach (Tuple<GameObject, int> platform in m_TouchingObjects)
        {
            // check if it's a platform (doesn't work right now ?)
            if (false)
                continue;

            platformData = platform.Item1.GetComponent<PlatformBasicScript>();
            if (platformData.isDamageTimerRuning)
            {
                platformData.currentTime += Time.fixedDeltaTime;
                if (platformData.currentTime >= platformData.platformData.deltaTimeDamage)
                {
                    platformData.isDamageTimerRuning = false;
                    platformData.currentTime = 0;
                }
            }
            else
            {
                damage += platformData.platformData.damage;
                platformData.isDamageTimerRuning = true;
            }
        }

        return damage;
    }
}
