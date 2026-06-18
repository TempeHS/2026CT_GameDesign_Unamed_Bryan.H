using UnityEngine;

public class PackagePickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();

        if (player != null)
        {
            player.PickUpPackage();
            gameObject.SetActive(false); // or Destroy(gameObject)
        }
    }
}