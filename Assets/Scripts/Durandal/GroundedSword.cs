using UnityEngine;

public class GroundedSword : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        // Searches the object or its child graphics for the Animator component
        anim = GetComponentInChildren<Animator>();
    }

    public void TriggerVanish()
    {
        if (anim != null)
        {
            anim.SetTrigger("Vanish"); // Fires the transition to the vanish animation
        }
        else
        {
            Destroy(gameObject); // Safety fallback
        }
    }

    // This is the function you will select inside the Animation Event timeline window!
    public void DestroyAfterVanishEvent()
    {
        Destroy(gameObject); // Cleanly destroys the sword prefab once it's done fading
    }
}