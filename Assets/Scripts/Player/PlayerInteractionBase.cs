using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class PlayerInteractionBase
{
    private PlayerGeneralBase m_GeneralBase;
    private PlayerMovementBase m_MovementBase;

    // List of objects that give the player a flip upon contact
    private readonly string[] m_FlipTagList = { "simplePlatform" };

    // Platform tag, platform direction, platform gameObject name
    private List<Tuple<GameObject, int>> m_TouchingObjects = new List<Tuple<GameObject, int>>();

    public void ConnectComponents(PlayerMovementBase movementBase, PlayerGeneralBase generalBase)
    {
        m_MovementBase = movementBase;
        m_GeneralBase = generalBase;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        //if (!(IsClient && IsOwner))
        //    return;

        // Check if the platform hit is eligible to give a flip
        if (Array.Exists(m_FlipTagList, element => collision.gameObject.CompareTag(element)))
        {
            m_MovementBase.GiveFlip();
        }

        // Adds the encountered object to the list
        int dir = FindCollisionDirection(collision);
        m_TouchingObjects.Add(new Tuple<GameObject, int>(collision.gameObject, dir));

        /*
        foreach (ContactPoint2D contact in collision.contacts)
        {
            Debug.DrawLine(new Vector3(contact.point.x, contact.point.y, transform.position.z), transform.position, Color.green, 2, false);
        }
        */
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        //if (!(IsClient && IsOwner))
        //    return;

        // Remove the object kura is not touching anymore from the list
        m_TouchingObjects.RemoveAt(m_TouchingObjects.FindIndex(e => e.Item1 == collision.gameObject));
    }

    // Function that finds the feet platform, returns the first platform that is below kura's legs
    // (relative to current kura gravity)
    public Tuple<GameObject, int> GiveFeetPlatform()
    {
        // NOTE: this probably should return all feet platforms, but irrelevant for now

        foreach (Tuple<GameObject, int> platform in m_TouchingObjects)
        {
            KuraTransfromData data = m_MovementBase.GetTransformData();
            if ((data.gravityDirection == 1 && platform.Item2 == 2) ||
                (data.gravityDirection == -1 && platform.Item2 == 0))
            {
                return platform;
            }
        }

        return null;
    }

    public bool IsOnFloor()
    {
        KuraTransfromData data = m_MovementBase.GetTransformData();
        return m_TouchingObjects.Any(platform => (data.gravityDirection == 1 && platform.Item2 == 2) ||
                                                 (data.gravityDirection == -1 && platform.Item2 == 0));
    }

    public bool IsKissWall()
    {
        return m_TouchingObjects.Any(platform => platform.Item2 == 1);
    }

    // Just finds which direction the hit object is relative to kura (not relative to kura's direction, aka. absolute)
    private int FindCollisionDirection(Collision2D collision)
    {
        // Direction is absolute
        // 0 - up, 1 -right, 2 - down, 3 - left
        List<(double, int)> directions = new ();

        if (collision.contacts.Length != 2)
        {
            Debug.Log("HELP HELP HELP HELP HELP BAD COLLISION");
            return -1;
        }

        KuraTransfromData data = m_MovementBase.GetTransformData();
        double dx = collision.contacts[0].point.x - data.position.x + collision.contacts[1].point.x - data.position.x;
        double dy = collision.contacts[0].point.y - data.position.y + collision.contacts[1].point.y - data.position.y;

        directions.Add((dy, 0));
        directions.Add((dx, 1));
        directions.Add((-dy, 2));
        directions.Add((-dx, 3));

        directions.Sort(delegate ((double, int) a, (double, int) b) { return b.Item1.CompareTo(a.Item1); });

        return directions[0].Item2;
    }

    protected void TakePeriodicDamageFromPlatmorms()
    {
        PlatformBasicScript platformData;
        foreach (Tuple<GameObject, int> platform in m_TouchingObjects)
        {
            // check if it's a platform (doesn't work right now ?)
            if (false)
                return;

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
                m_GeneralBase.Damage(platformData.platformData.damage);
                platformData.isDamageTimerRuning = true;
            }
        }
    }
}
